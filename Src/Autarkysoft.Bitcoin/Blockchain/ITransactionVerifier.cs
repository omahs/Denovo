﻿// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Transactions;

namespace Autarkysoft.Bitcoin.Blockchain
{
    public interface ITransactionVerifier
    {
        int BlockHeight { get; set; }
        int SigOpCount { get; set; }
        ulong TotalFee { get; set; }

        bool Verify(ITransaction tx, out string error);
        bool VerifyCoinbase(ITransaction transaction, out string error);
        bool VerifyCoinbaseOutput(ITransaction transaction);
    }
}
