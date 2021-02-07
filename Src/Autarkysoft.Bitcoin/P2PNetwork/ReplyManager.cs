﻿// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.Blockchain.Blocks;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Autarkysoft.Bitcoin.P2PNetwork
{
    /// <summary>
    /// Implementation of a reply manager to handle creation of new <see cref="Message"/>s to return in response to
    /// received <see cref="Message"/>s.
    /// Implements <see cref="IReplyManager"/>.
    /// </summary>
    public class ReplyManager : IReplyManager
    {
        /// <summary>
        /// Initializes a new instanse of <see cref="ReplyManager"/> using the given parameters.
        /// </summary>
        /// <param name="ns">Node status</param>
        /// <param name="cs">Client settings</param>
        public ReplyManager(INodeStatus ns, IClientSettings cs)
        {
            nodeStatus = ns;
            settings = cs;
        }


        private readonly INodeStatus nodeStatus;
        private readonly IClientSettings settings;


        private Message[] GetSettingsMessages(Message extraMsg)
        {
            var result = new List<Message>(6);
            if (!(extraMsg is null))
            {
                result.Add(extraMsg);
            }

            if (!settings.HasNeededServices(nodeStatus.Services))
            {
                nodeStatus.SignalDisconnect();
                return null;
            }

            // We want the other node to respond to our initial settings quickly to check headers.
            // TODO: this may be a bad thing to enfoce on all nodes. Maybe force it based on Blockchain.State
            nodeStatus.StartDisconnectTimer(TimeConstants.OneMin_Milliseconds);

            //result.Add(new Message(new GetAddrPayload(), settings.Network));

            if (nodeStatus.ProtocolVersion > Constants.P2PBip31ProtVer)
            {
                // We don't bother sending ping to a node that doesn't support nonce in ping/pong messages.
                // This will set default value for latency and this node will be ignored when latency is used later.
                result.Add(GetPingMsg());
            }

            if (nodeStatus.ProtocolVersion >= Constants.P2PBip130ProtVer)
            {
                result.Add(new Message(new SendHeadersPayload(), settings.Network));
            }

            // Always send GetHeaders message during handshake
            result.Add(GetLocatorMessage());


            if (settings.Blockchain.State == BlockchainState.Synchronized)
            {
                // We don't listen or relay anything when client is not yet fully synchronized
                // FeeFilter
                if (settings.Relay && nodeStatus.ProtocolVersion >= Constants.P2PBip133ProtVer)
                {
                    result.Add(new Message(new FeeFilterPayload(settings.MinTxRelayFee * 1000), settings.Network));
                }

                // Addr (protocol versions we connect to support this message type)
                if (settings.Relay)
                {
                    IPAddress myIp = settings.GetMyIP();
                    if (!IPAddress.IsLoopback(myIp))
                    {
                        uint time = (uint)settings.Time.Now;
                        var myAddr = new NetworkAddressWithTime(settings.Services, myIp, settings.ListenPort, time);
                        result.Add(new Message(new AddrPayload(new NetworkAddressWithTime[1] { myAddr }), settings.Network));
                    }
                }
            }

            return result.Count == 0 ? null : result.ToArray();
        }


        private Message GetLocatorMessage()
        {
            BlockHeader[] headers = settings.Blockchain.GetBlockHeaderLocator();
            if (headers.Length > GetHeadersPayload.MaximumHashes)
            {
                // This should never happen but since IBlockchain is a dependency we have to check it here
                // to prevent an exception being thrown.
                BlockHeader[] temp = new BlockHeader[GetHeadersPayload.MaximumHashes];
                Array.Copy(headers, 0, temp, 0, temp.Length);
                headers = temp;
            }
            return new Message(new GetHeadersPayload(settings.ProtocolVersion, headers, null), settings.Network);
        }

        private Message[] GetMissingBlockMessage()
        {
            settings.Blockchain.SetMissingBlockHashes(nodeStatus);
            return new Message[]
            {
               new Message(new GetDataPayload(nodeStatus.InvsToGet.ToArray()), settings.Network)
            };
        }

        /// <inheritdoc/>
        public Message GetPingMsg()
        {
            long nonce = settings.Rng.NextInt64();
            // TODO: latency may have a small error this way (maybe the following line should be moved to Node class)
            // Chances of nonce being repeated is 1 in 2^64 which is why the returned bool is ignored here
            nodeStatus.StorePing(nonce);
            return new Message(new PingPayload(nonce), settings.Network);
        }

        /// <inheritdoc/>
        // Node constructor sets the IP and port on INodeStatus
        // TODO: this is bitcoin-core's behavior, it can be changed if needed
        public Message GetVersionMsg() => GetVersionMsg(new NetworkAddress(0, nodeStatus.IP, nodeStatus.Port));

        /// <inheritdoc/>
        public Message GetVersionMsg(NetworkAddress recvAddr)
        {
            var ver = new VersionPayload()
            {
                Version = settings.ProtocolVersion,
                Services = settings.Services,
                Timestamp = settings.Time.Now,
                ReceivingNodeNetworkAddress = recvAddr,
                // TODO: IP and port zero are bitcoin-core's behavior, it can be changed if needed
                TransmittingNodeNetworkAddress = new NetworkAddress(settings.Services, IPAddress.IPv6Any, 0),
                Nonce = (ulong)settings.Rng.NextInt64(),
                UserAgent = settings.UserAgent,
                StartHeight = settings.Blockchain.Height,
                Relay = settings.Relay
            };
            return new Message(ver, settings.Network);
        }

        private bool Deser<T>(byte[] data, out T pl) where T : IMessagePayload, new()
        {
            pl = new T();
            if (pl.TryDeserialize(new FastStreamReader(data), out string error))
            {
                return true;
            }
            else
            {
                nodeStatus.AddSmallViolation();
                return false;
            }
        }

        /// <inheritdoc/>
        public Message[] GetReply(Message msg)
        {
            if (!msg.TryGetPayloadType(out PayloadType plt))
            {
                // Undefined payload type (this is a violation since other node knows our protocol version)
                nodeStatus.AddSmallViolation();
                nodeStatus.UpdateTime();
                return null;
            }

            if (nodeStatus.HandShake != HandShakeState.Finished && plt != PayloadType.Version && plt != PayloadType.Verack)
            {
                nodeStatus.AddMediumViolation();
                nodeStatus.UpdateTime();
                return null;
            }

            Message[] result = null;

            switch (plt)
            {
                // TODO: write the missing parts inside each "if" to use the deserialized object
                case PayloadType.Addr:
                    if (Deser(msg.PayloadData, out AddrPayload nodeAddresses))
                    {
                        settings.UpdateNodeAddrs(nodeAddresses.Addresses);
                    }
                    break;
                case PayloadType.AddrV2:
                    break;
                case PayloadType.Alert:
                    // Alert messages are ignored
                    // TODO: add violation if the protocol version is above the one that disabled this type
                    break;
                case PayloadType.Block:
                    if (settings.Blockchain.State == BlockchainState.HeadersSync)
                    {
                        // Don't process any blocks when syncing the headers from one node (initial sync)
                        nodeStatus.UpdateTime();
                        return null;
                    }

                    if (Deser(msg.PayloadData, out BlockPayload blk))
                    {
                        if (!settings.Blockchain.ProcessBlock(blk.BlockData, nodeStatus))
                        {
                            nodeStatus.AddMediumViolation();
                        }
                    }
                    break;
                case PayloadType.BlockTxn:
                    if (Deser(msg.PayloadData, out BlockTxnPayload blkTxn))
                    {

                    }
                    break;
                case PayloadType.CFCheckpt:
                    break;
                case PayloadType.CFHeaders:
                    break;
                case PayloadType.CFilter:
                    break;
                case PayloadType.CmpctBlock:
                    if (Deser(msg.PayloadData, out CmpctBlockPayload cmBlk))
                    {

                    }
                    break;
                case PayloadType.FeeFilter:
                    if (Deser(msg.PayloadData, out FeeFilterPayload feeFilter))
                    {
                        // Note that settings.Relay is not checked, we may not relay transactions but we can send our own txs
                        // that have to honor the other node's fee preference.
                        if (!nodeStatus.Relay)
                        {
                            // A node that doesn't relay txs doesn't need a fee filter!
                            nodeStatus.AddSmallViolation();
                        }
                        // TODO: set the following constant in Constants as MaxTxRelayFee (?)
                        else if (feeFilter.FeeRate >= 444000_000UL)
                        {
                            // Don't waste time on nodes that set their MinRelayTxFee to such a high value that fee of a
                            // small tx should be nearly 1 BTC for them to accept it in their mempool
                            nodeStatus.Relay = false;
                            // This is also considered a violation
                            nodeStatus.AddMediumViolation();
                        }
                        else
                        {
                            // Fee filter can be updated multiple times as the other node's mempool size changes
                            nodeStatus.FeeFilter = feeFilter.FeeRate;
                        }
                    }
                    break;
                case PayloadType.FilterAdd:
                    if (Deser(msg.PayloadData, out FilterAddPayload filterAdd))
                    {

                    }
                    break;
                case PayloadType.FilterClear:
                    // Empty payload
                    // TODO: nodestatus has to clear the set filters here
                    break;
                case PayloadType.FilterLoad:
                    if (Deser(msg.PayloadData, out FilterLoadPayload filterLoad))
                    {

                    }
                    break;
                case PayloadType.GetAddr:
                    // Empty payload
                    if (!nodeStatus.IsAddrSent)
                    {
                        nodeStatus.IsAddrSent = true;
                        NetworkAddressWithTime[] availableAddrs = settings.GetRandomNodeAddrs(10, Constants.MaxAddrCount, true);
                        if (!(availableAddrs is null) && availableAddrs.Length != 0)
                        {
                            result = new Message[] { new Message(new AddrPayload(availableAddrs), settings.Network) };
                        }
                    }
                    break;
                case PayloadType.GetBlocks:
                    if (Deser(msg.PayloadData, out GetBlocksPayload getBlks))
                    {

                    }
                    break;
                case PayloadType.GetBlockTxn:
                    if (Deser(msg.PayloadData, out GetBlockTxnPayload getBlkTxn))
                    {

                    }
                    break;
                case PayloadType.GetCFHeaders:
                    break;
                case PayloadType.GetCFCheckpt:
                    break;
                case PayloadType.GetCFilters:
                    break;
                case PayloadType.GetData:
                    if (settings.Blockchain.State != BlockchainState.Synchronized)
                    {
                        nodeStatus.UpdateTime();
                        // If client is syncing it can't provide data to peers
                        return null;
                    }

                    if (Deser(msg.PayloadData, out GetDataPayload getData))
                    {

                    }
                    break;
                case PayloadType.GetHeaders:
                    if (settings.Blockchain.State == BlockchainState.HeadersSync)
                    {
                        nodeStatus.UpdateTime();
                        // If the client is syncing its headers it can't provide headers
                        return null;
                    }

                    if (Deser(msg.PayloadData, out GetHeadersPayload getHdrs))
                    {
                        BlockHeader[] hds = settings.Blockchain.GetMissingHeaders(getHdrs.Hashes, getHdrs.StopHash);
                        if (!(hds is null))
                        {
                            if (hds.Length > HeadersPayload.MaxCount)
                            {
                                // This should never happen but since IBlockchain is a dependency we have to check it here
                                // to prevent an exception being thrown.
                                BlockHeader[] temp = new BlockHeader[HeadersPayload.MaxCount];
                                Array.Copy(hds, 0, temp, 0, temp.Length);
                                hds = temp;
                            }

                            result = new Message[1] { new Message(new HeadersPayload(hds), settings.Network) };
                        }
                    }
                    break;
                case PayloadType.Headers:
                    if (Deser(msg.PayloadData, out HeadersPayload hdrs))
                    {
                        if (hdrs.Headers.Length == 0)
                        {
                            nodeStatus.UpdateTime();
                            // Header locator will always create a request that will fetch at least one header.
                            // Additionally sending an empty header array is a violation on its own.
                            nodeStatus.AddMediumViolation();
                            return null;
                        }

                        BlockProcessResult processResult = settings.Blockchain.ProcessHeaders(hdrs.Headers);
                        switch (processResult)
                        {
                            case BlockProcessResult.UnknownBlocks:
                                nodeStatus.AddSmallViolation();
                                nodeStatus.ReStartDisconnectTimer();
                                result = new Message[1] { GetLocatorMessage() };
                                break;
                            case BlockProcessResult.InvalidBlocks:
                                nodeStatus.StopDisconnectTimer();
                                if (settings.Blockchain.State != BlockchainState.Synchronized)
                                {
                                    nodeStatus.SignalDisconnect();
                                }
                                else
                                {
                                    nodeStatus.AddMediumViolation();
                                }
                                break;
                            case BlockProcessResult.ForkBlocks:
                                break;
                            case BlockProcessResult.Success:
                                if (hdrs.Headers.Length == HeadersPayload.MaxCount)
                                {
                                    nodeStatus.ReStartDisconnectTimer();
                                    result = new Message[1] { GetLocatorMessage() };
                                }
                                else if (settings.Blockchain.State == BlockchainState.BlocksSync)
                                {
                                    nodeStatus.StopDisconnectTimer();
                                    result = GetMissingBlockMessage();
                                    nodeStatus.StartDisconnectTimer(TimeConstants.OneMin_Milliseconds);
                                }
                                break;
                        }
                    }
                    break;
                case PayloadType.Inv:
                    if (Deser(msg.PayloadData, out InvPayload inv))
                    {
                        // TODO: it may be best if InvPayload sets this while deserializing to avoid using Linq
                        if (!settings.Relay && inv.InventoryList.Any(x => x.InvType == InventoryType.Tx))
                        {
                            nodeStatus.AddBigViolation();
                        }
                    }
                    break;
                case PayloadType.MemPool:
                    // Empty payload
                    break;
                case PayloadType.MerkleBlock:
                    if (Deser(msg.PayloadData, out MerkleBlockPayload mrklBlk))
                    {

                    }
                    break;
                case PayloadType.NotFound:
                    if (Deser(msg.PayloadData, out NotFoundPayload notFound))
                    {

                    }
                    break;
                case PayloadType.Ping:
                    if (Deser(msg.PayloadData, out PingPayload ping))
                    {
                        result = new Message[1] { new Message(new PongPayload(ping.Nonce), settings.Network) };
                    }
                    break;
                case PayloadType.Pong:
                    if (Deser(msg.PayloadData, out PongPayload pong))
                    {
                        nodeStatus.CheckPing(pong.Nonce);
                    }
                    break;
                case PayloadType.Reject:
                    // Reject messages are ignored
                    break;
                case PayloadType.SendAddrV2:
                    break;
                case PayloadType.SendCmpct:
                    if (Deser(msg.PayloadData, out SendCmpctPayload sendCmp))
                    {
                        nodeStatus.SendCompact = sendCmp.Announce;
                        nodeStatus.SendCompactVer = sendCmp.CmpctVersion;
                    }
                    break;
                case PayloadType.SendHeaders:
                    // Empty payload
                    if (nodeStatus.SendHeaders)
                    {
                        // It's a violation if the other node "spams" the same settings more than once.
                        nodeStatus.AddSmallViolation();
                    }
                    else
                    {
                        nodeStatus.SendHeaders = true;
                    }
                    break;
                case PayloadType.Tx:
                    if (!settings.Relay)
                    {
                        nodeStatus.AddBigViolation();
                    }
                    else if (Deser(msg.PayloadData, out TxPayload tx))
                    {
                        if (!settings.AddToMempool(tx.Tx))
                        {
                            nodeStatus.AddMediumViolation();
                        }
                    }
                    break;
                case PayloadType.Verack:
                    result = CheckVerack();
                    break;
                case PayloadType.Version:
                    result = CheckVersion(msg);
                    break;
                case PayloadType.WTxIdRelay:
                    break;
            }

            nodeStatus.UpdateTime();
            return result;
        }


        private Message[] CheckVerack()
        {
            // VerackPayload doesn't have a body and won't deserialize anything
            // If anything were added to it in the future a TryDeserialize() should be written here

            switch (nodeStatus.HandShake)
            {
                case HandShakeState.None:
                case HandShakeState.SentAndConfirmed:
                case HandShakeState.Finished:
                    nodeStatus.AddMediumViolation();
                    break;
                case HandShakeState.ReceivedAndReplied:
                case HandShakeState.SentAndReceived:
                    nodeStatus.HandShake = HandShakeState.Finished;
                    return GetSettingsMessages(null);
                case HandShakeState.Sent:
                    nodeStatus.HandShake = HandShakeState.SentAndConfirmed;
                    break;
                default:
                    break;
            }

            return null;
        }

        private Message[] CheckVersion(Message msg)
        {
            var version = new VersionPayload();
            if (!version.TryDeserialize(new FastStreamReader(msg.PayloadData), out _))
            {
                nodeStatus.AddSmallViolation();
                return null;
            }

            if (version.Version < Constants.P2PMinProtoVer)
            {
                nodeStatus.SignalDisconnect();
                return null;
            }

            nodeStatus.ProtocolVersion = version.Version;
            nodeStatus.Services = version.Services;
            nodeStatus.Nonce = version.Nonce;
            nodeStatus.UserAgent = version.UserAgent;
            nodeStatus.StartHeight = version.StartHeight;
            nodeStatus.Relay = version.Relay;
            settings.UpdateMyIP(version.ReceivingNodeNetworkAddress.NodeIP);
            settings.Time.UpdateTime(version.Timestamp);

            Message[] result = null;

            switch (nodeStatus.HandShake)
            {
                case HandShakeState.None:
                    nodeStatus.HandShake = HandShakeState.ReceivedAndReplied;
                    result = new Message[2]
                    {
                        new Message(new VerackPayload(), settings.Network),
                        GetVersionMsg()
                    };
                    break;
                case HandShakeState.Sent:
                    nodeStatus.HandShake = HandShakeState.SentAndReceived;
                    result = new Message[1]
                    {
                        new Message(new VerackPayload(), settings.Network)
                    };
                    break;
                case HandShakeState.SentAndConfirmed:
                    nodeStatus.HandShake = HandShakeState.Finished;
                    result = GetSettingsMessages(new Message(new VerackPayload(), settings.Network));
                    break;
                case HandShakeState.ReceivedAndReplied:
                case HandShakeState.SentAndReceived:
                case HandShakeState.Finished:
                    nodeStatus.AddMediumViolation();
                    break;
                default:
                    break;
            }

            return result;
        }
    }
}
