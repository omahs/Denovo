﻿// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;

namespace Autarkysoft.Bitcoin.Blockchain
{
    /// <summary>
    /// Implements <see cref="IUtxo"/> and <see cref="IDeserializable"/>.
    /// </summary>
    public class Utxo : IUtxo, IDeserializable
    {
        /// <inheritdoc/>
        public bool IsMempoolSpent { get; set; }
        /// <inheritdoc/>
        public bool IsBlockSpent { get; set; }

        /// <inheritdoc/>
        public uint Index { get; set; }
        /// <inheritdoc/>
        public ulong Amount { get; set; }

        private IPubkeyScript _pubScr = new PubkeyScript();
        /// <inheritdoc/>
        public IPubkeyScript PubScript
        {
            get => _pubScr;
            set => _pubScr = value ?? new PubkeyScript();
        }

        /// <inheritdoc/>
        public void Serialize(FastStream stream)
        {
            stream.Write(Index);
            stream.Write(Amount);
            PubScript.Serialize(stream);
        }

        /// <inheritdoc/>
        public bool TryDeserialize(FastStreamReader stream, out string error)
        {
            if (stream is null)
            {
                error = "Stream can not be null.";
                return false;
            }

            if (!stream.CheckRemaining(sizeof(uint) + sizeof(ulong)))
            {
                error = Err.EndOfStream;
                return false;
            }

            Index = stream.ReadUInt32Checked();
            Amount = stream.ReadUInt64Checked();

            return PubScript.TryDeserialize(stream, out error);
        }
    }
}
