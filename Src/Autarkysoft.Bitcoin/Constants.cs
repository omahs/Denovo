﻿// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin
{
    /// <summary>
    /// Constant values used globally in different places.
    /// </summary>
    public struct Constants
    {
        /// <summary>
        /// Smallest bitcoin unit (10^-8)
        /// </summary>
        public const decimal Satoshi = 0.0000_0001m;
        /// <summary>
        /// The bitcoin symbol
        /// </summary>
        public const string Symbol = "฿";
        /// <summary>
        /// Total bitcoin supply in satoshi, 21 million with 8 decimal places.
        /// </summary>
        public const ulong TotalSupply = 21_000_000_0000_0000;
        /// <summary>
        /// Number of blocks between each difficulty adjustment
        /// </summary>
        public const int DifficultyAdjustmentInterval = 2016;
        /// <summary>
        /// 2 weeks in seconds
        /// </summary>
        public const uint PowTargetTimespan = TimeConstants.TwoWeeks_Seconds;
        /// <summary>
        /// Number of times to retry setting random bytes using <see cref="Cryptography.IRandomNumberGenerator"/>.
        /// </summary>
        /// <remarks>
        /// 2 should be enough, we set it higher to be safe!
        /// </remarks>
        public const int RngRetryCount = 5;
        /// <summary>
        /// Byte length of a compressed public key
        /// </summary>
        public const int CompressedPubkeyLen = 33;
        /// <summary>
        /// Byte length of an uncompressed public key
        /// </summary>
        public const int UncompressedPubkeyLen = 65;
        /// <summary>
        /// Number of bytes in a checksum (first 4 bytes of double SHA256 of data)
        /// </summary>
        public const int ChecksumSize = 4;
        /// <summary>
        /// Byte length of <see cref="Cryptography.Hashing.Ripemd160Sha256"/> hash result
        /// </summary>
        public const int Hash160Length = 20;
        /// <summary>
        /// Byte length of <see cref="Cryptography.Hashing.Sha256"/> hash result
        /// </summary>
        public const int Sha256Length = 32;
        /// <summary>
        /// Byte length of the block headers
        /// </summary>
        public const int BlockHeaderSize = 80;
        /// <summary>
        /// Maximum allowed block weight (BIP-141)
        /// </summary>
        public const int MaxBlockWeight = 4_000_000;
        /// <summary>
        /// Maximum amount of time that a block timestamp is allowed to exceed the
        /// current network-adjusted time before the block will be accepted.
        /// </summary>
        public const long MaxFutureBlockTime = 2 * 60 * 60;
        /// <summary>
        /// Maximum allowed length of the scripts in bytes
        /// </summary>
        public const int MaxScriptLength = 10_000;
        /// <summary>
        /// Maximum allowed number of non-push OPs in a script
        /// </summary>
        public const int MaxScriptOpCount = 201;
        /// <summary>
        /// Maximum allowed length of items to be pushed to the stack 
        /// (the redeem scripts used in P2SH are also limited by this length)
        /// </summary>
        public const int MaxScriptItemLength = 520;
        /// <summary>
        /// Maximum allowed number of items on the stack and alt-stack combined
        /// </summary>
        public const int MaxScriptStackItemCount = 1000;
        /// <summary>
        /// Minimum allowed length of the coinbase signature script
        /// </summary>
        public const int MinCoinbaseScriptLength = 2;
        /// <summary>
        /// Maximum allowed length of the coinbase signature script
        /// </summary>
        public const int MaxCoinbaseScriptLength = 100;
        /// <summary>
        /// Maximum allowed number of signature check operations per block
        /// </summary>
        public const int MaxSigOpCount = 80_000;
        /// <summary>
        /// Maximum allowed number of public keys per script
        /// </summary>
        public const int MaxMultisigPubkeyCount = 20;
        /// <summary>
        /// The factor by which values (eg. size, SigOpCount, ...) are multiplied 
        /// </summary>
        public const int WitnessScaleFactor = 4;
        /// <summary>
        /// Minimum byte length of the witness commitment in the coinbase output as defined by BIP-141.
        /// </summary>
        public const int MinWitnessCommitmentLen = 38;
        /// <summary>
        /// Validation weight per passing signature
        /// </summary>
        public const int ValidationWeightPerSigOp = 50;
        /// <summary>
        /// How much weight budget is added to the witness size 
        /// </summary>
        public const int ValidationWeightOffset = 50;
        /// <summary>
        /// The constant string that is attached to the beginning of a message before it is signed
        /// </summary>
        public const string MsgSignConst = "Bitcoin Signed Message:\n";
        /// <summary>
        /// Default main network port
        /// </summary>
        public const ushort MainNetPort = 8333;
        /// <summary>
        /// Default test network port
        /// </summary>
        public const ushort TestNetPort = 18333;
        /// <summary>
        /// Default regtest network port
        /// </summary>
        public const ushort RegTestPort = 18444;
        /// <summary>
        /// 4 byte "magic" value used in P2P message headers for main-net
        /// </summary>
        public const string MainNetMagic = "f9beb4d9";
        /// <summary>
        /// 4 byte "magic" value used in P2P message headers for test-net
        /// </summary>
        public const string TestNetMagic = "0b110907";
        /// <summary>
        /// 4 byte "magic" value used in P2P message headers for reg-test
        /// </summary>
        public const string RegTestMagic = "fabfb5da";
        /// <summary>
        /// The latest P2P protocol version supported by this library
        /// </summary>
        public const int P2PProtocolVersion = 70015;
        /// <summary>
        /// Minimum protocol version that we connect to.
        /// <para/>These are bitcoin core clients with versions &#60; 0.3.18
        /// which don't support the Headers and GetHeaders messages
        /// </summary>
        public const int P2PMinProtoVer = 31800;
        /// <summary>
        /// Protocol version where BIP-31 (Pong message) was enabled
        /// </summary>
        public const int P2PBip31ProtVer = 60000;
        /// <summary>
        /// Protocol version where BIP-130 (SendHeaders message) was enabled
        /// </summary>
        public const int P2PBip130ProtVer = 70012;
        /// <summary>
        /// Protocol version where BIP-133 (FeeFilter message) was enabled
        /// </summary>
        public const int P2PBip133ProtVer = 70013;
        /// <summary>
        /// Length of P2P message headers (4 magic + 12 command + 4 payloadSize + 4 checksum)
        /// </summary>
        public const int MessageHeaderSize = 24;
        /// <summary>
        /// Maximum allowed P2P message payload size (4 MB)
        /// </summary>
        /// <remarks>
        /// https://github.com/bitcoin/bitcoin/blob/5879bfa9a541576100d939d329a2639b79d9e4f9/src/net.h#L55-L56
        /// </remarks>
        public const int MaxPayloadSize = 4 * 1000 * 1000;
        /// <summary>
        /// Maximum allowed number of items in the <see cref="P2PNetwork.Messages.MessagePayloads.AddrPayload.Addresses"/> list
        /// </summary>
        public const int MaxAddrCount = 1000;
        /// <summary>
        /// Maximum number of hashes allowed in located used in <see cref="P2PNetwork.Messages.MessagePayloads.GetHeadersPayload"/>
        /// and <see cref="P2PNetwork.Messages.MessagePayloads.GetBlocksPayload"/>
        /// </summary>
        /// <remarks>
        /// https://github.com/bitcoin/bitcoin/blob/81d5af42f4dba5b68a597536cad7f61894dc22a3/src/net_processing.cpp#L71-L72
        /// </remarks>
        public const int MaxLocatorCount = 101;
    }



    /// <summary>
    /// Globally used constant time values.
    /// </summary>
    public struct TimeConstants
    {
        /// <summary>
        /// 1 second in milliseconds
        /// </summary>
        public const int OneSecond_Milliseconds = 1_000;

        /// <summary>
        /// 5 seconds in milliseconds
        /// </summary>
        public const int FiveSeconds_Milliseconds = 5_000;
        /// <summary>
        /// 10 seconds in milliseconds
        /// </summary>
        public const int TenSeconds_Milliseconds = 10_000;

        /// <summary>
        /// 1 minute in milliseconds
        /// </summary>
        public const double OneMin_Milliseconds = 60_000;
        /// <summary>
        /// 2 minutes in milliseconds
        /// </summary>
        public const double TwoMin_Milliseconds = 120_000;

        /// <summary>
        /// 1 day in seconds
        /// </summary>
        public const int OneDay_Seconds = 1 * 24 * 60 * 60;

        /// <summary>
        /// 2 weeks in seconds
        /// </summary>
        public const int TwoWeeks_Seconds = 2 * 7 * 24 * 60 * 60;
    }



    internal static class ZeroBytes
    {
        /// <summary>
        /// 256-bit zero
        /// </summary>
        internal static readonly byte[] B32 = new byte[32];
        /// <summary>
        /// 160 bit zero
        /// </summary>
        internal static readonly byte[] B20 = new byte[20];
        /// <summary>
        /// Negative 256-bit zero
        /// </summary>
        internal static readonly byte[] B32N = new byte[32]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x80
        };
        /// <summary>
        /// Negative 160-bit zero
        /// </summary>
        internal static readonly byte[] B20N = new byte[20]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x80
        };
    }
}
