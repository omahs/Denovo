﻿// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts.Operations
{
    public class CheckMultiSigOpTests
    {
        [Fact]
        public void Run_CorrectSigsTest()
        {
            MockOpData data = new MockOpData(FuncCallName.Pop, FuncCallName.PopIndex,
                                             FuncCallName.PopCount, FuncCallName.PopCount,
                                             FuncCallName.Pop, FuncCallName.Push)
            {
                _itemCount = 8,
                _opCountToReturn = 1,
                popData = new byte[][] { OpTestCaseHelper.b7, OpTestCaseHelper.num3 },
                popIndexData = new Dictionary<int, byte[]> { { 3, OpTestCaseHelper.num2 } },
                popCountData = new byte[][][]
                {
                    new byte[][]
                    {
                        KeyHelper.Pub1CompBytes, KeyHelper.Pub2CompBytes, KeyHelper.Pub3CompBytes,
                    },
                    new byte[][]
                    {
                        Helper.ShortSig1Bytes, Helper.ShortSig2Bytes,
                    }
                },
                expectedSigs = new byte[][] { Helper.ShortSig1Bytes, Helper.ShortSig2Bytes },
                expectedPubkeys = new byte[][] { KeyHelper.Pub1CompBytes, KeyHelper.Pub2CompBytes, KeyHelper.Pub3CompBytes },
                expectedMultiSigGarbage = OpTestCaseHelper.b7,
                sigVerificationSuccess = true,
                pushData = new byte[][] { OpTestCaseHelper.TrueBytes }
            };

            OpTestCaseHelper.RunTest<CheckMultiSigOp>(data, OP.CheckMultiSig);
            Assert.Equal(4, data.OpCount);
        }

        [Fact]
        public void Run_WrongSigsTest()
        {
            MockOpData data = new MockOpData(FuncCallName.Pop, FuncCallName.PopIndex,
                                             FuncCallName.PopCount, FuncCallName.PopCount,
                                             FuncCallName.Pop, FuncCallName.Push)
            {
                _itemCount = 8,
                _opCountToReturn = 5,
                popData = new byte[][] { OpTestCaseHelper.b7, OpTestCaseHelper.num3 },
                popIndexData = new Dictionary<int, byte[]> { { 3, OpTestCaseHelper.num2 } },
                popCountData = new byte[][][]
                {
                    new byte[][]
                    {
                        KeyHelper.Pub1CompBytes, KeyHelper.Pub2CompBytes, KeyHelper.Pub3CompBytes,
                    },
                    new byte[][]
                    {
                        Helper.ShortSig1Bytes, Helper.ShortSig2Bytes,
                    }
                },
                expectedSigs = new byte[][] { Helper.ShortSig1Bytes, Helper.ShortSig2Bytes },
                expectedPubkeys = new byte[][] { KeyHelper.Pub1CompBytes, KeyHelper.Pub2CompBytes, KeyHelper.Pub3CompBytes },
                expectedMultiSigGarbage = OpTestCaseHelper.b7,
                sigVerificationSuccess = false,
                pushData = new byte[][] { OpTestCaseHelper.FalseBytes }
            };

            OpTestCaseHelper.RunTest<CheckMultiSigOp>(data, OP.CheckMultiSig);
            Assert.Equal(8, data.OpCount);
        }

        public static IEnumerable<object[]> GetSpecialCase()
        {
            yield return new object[]
            {
                true,
                new IOperation[]
                {
                    new PushDataOp(OP._0), // garbage
                    new PushDataOp(OP._0), // m
                    new PushDataOp(OP._0), // n
                    new CheckMultiSigOp(),
                },
                true,
                null
            };
            yield return new object[]
            {
                false,
                new IOperation[]
                {
                    new PushDataOp(OP._0), // garbage
                    new PushDataOp(OP._0), // m
                    new PushDataOp(OP._0), // n
                    new CheckMultiSigOp(),
                },
                true,
                null
            };
            yield return new object[]
            {
                false,
                new IOperation[]
                {
                    new PushDataOp(OP._2), // garbage
                    new PushDataOp(OP._0), // m
                    new PushDataOp(OP._0), // n
                    new CheckMultiSigOp(),
                },
                true,
                null
            };
            yield return new object[]
            {
                true,
                new IOperation[]
                {
                    new PushDataOp(OP._2), // garbage
                    new PushDataOp(OP._0), // m
                    new PushDataOp(OP._0), // n
                    new CheckMultiSigOp(),
                },
                false,
                "The extra item should be OP_0."
            };
        }
        [Theory]
        [MemberData(nameof(GetSpecialCase))]
        public void Run_SpecialCaseTest(bool isStrict, IOperation[] operations, bool expBool, string expError)
        {
            // 0of0 multisig => OP_0 [] OP_0 [] OP_0
            OpData data = new OpData()
            {
                IsStrictMultiSigGarbage = isStrict
            };

            // Run the first 3 PushOps
            for (int i = 0; i < operations.Length - 1; i++)
            {
                bool b1 = operations[i].Run(data, out string error1);
                Assert.True(b1, error1);
                Assert.Null(error1);
            }

            // Run the OP_CheckMultiSig operation
            bool b2 = operations[^1].Run(data, out string error2);
            Assert.Equal(expBool, b2);
            Assert.Equal(expError, error2);
            Assert.Equal(0, data.OpCount);
        }

        [Fact]
        public void Run_()
        {

        }


        public static IEnumerable<object[]> GetErrorCases()
        {
            yield return new object[]
            {
                new MockOpData()
                {
                    _itemCount = 2
                },
                Err.OpNotEnoughItems
            };
            yield return new object[]
            {
                new MockOpData(FuncCallName.Pop)
                {
                    _itemCount = 3,
                    _opCountToReturn = Constants.MaxScriptOpCount - 2,
                    popData = new byte[1][] { OpTestCaseHelper.num3 }
                },
                "Number of OPs in this script exceeds the allowed number."
            };
            yield return new object[]
            {
                new MockOpData(FuncCallName.Pop)
                {
                    _itemCount = 3,
                    _opCountToReturn = 0,
                    popData = new byte[1][] { new byte[1] { 21 } }
                },
                "Invalid number of public keys in multi-sig."
            };
            yield return new object[]
            {
                new MockOpData(FuncCallName.Pop)
                {
                    _itemCount = 3,
                    _opCountToReturn = 0,
                    popData = new byte[1][] { OpTestCaseHelper.numNeg1 }
                },
                "Invalid number of public keys in multi-sig."
            };
            yield return new object[]
            {
                new MockOpData(FuncCallName.Pop)
                {
                    _itemCount = 3,
                    _opCountToReturn = 0,
                    popData = new byte[1][] { OpTestCaseHelper.num2 }
                },
                Err.OpNotEnoughItems
            };
            yield return new object[]
            {
                new MockOpData(FuncCallName.Pop, FuncCallName.PopIndex)
                {
                    _itemCount = 3,
                    _opCountToReturn = 0,
                    popData = new byte[1][] { OpTestCaseHelper.num1 },
                    popIndexData = new Dictionary<int, byte[]> { { 1, new byte[2] { 1, 2 } } },
                },
                "Invalid number (m) format."
            };
            yield return new object[]
            {
                new MockOpData(FuncCallName.Pop, FuncCallName.PopIndex)
                {
                    _itemCount = 3,
                    _opCountToReturn = 0,
                    popData = new byte[1][] { OpTestCaseHelper.num1 },
                    popIndexData = new Dictionary<int, byte[]> { { 1, new byte[1] { 21 } } },
                },
                "Invalid number of signatures in multi-sig."
            };
            yield return new object[]
            {
                new MockOpData(FuncCallName.Pop, FuncCallName.PopIndex)
                {
                    _itemCount = 3,
                    _opCountToReturn = 0,
                    popData = new byte[1][] { OpTestCaseHelper.num1 },
                    popIndexData = new Dictionary<int, byte[]> { { 1, OpTestCaseHelper.num2 } }, // m > n
                },
                "Invalid number of signatures in multi-sig."
            };
            yield return new object[]
            {
                new MockOpData(FuncCallName.Pop, FuncCallName.PopIndex)
                {
                    _itemCount = 3,
                    _opCountToReturn = 0,
                    popData = new byte[1][] { OpTestCaseHelper.num1 },
                    popIndexData = new Dictionary<int, byte[]> { { 1, OpTestCaseHelper.numNeg1 } },
                },
                "Invalid number of signatures in multi-sig."
            };
            yield return new object[]
            {
                new MockOpData(FuncCallName.Pop, FuncCallName.PopIndex)
                {
                    _itemCount = 4,
                    _opCountToReturn = 0,
                    popData = new byte[1][] { OpTestCaseHelper.num2 },
                    popIndexData = new Dictionary<int, byte[]> { { 2, OpTestCaseHelper.num2 } },
                },
                Err.OpNotEnoughItems
            };
            yield return new object[]
            {
                new MockOpData(FuncCallName.Pop, FuncCallName.PopIndex)
                {
                    _itemCount = 4,
                    _opCountToReturn = 0,
                    popData = new byte[1][] { OpTestCaseHelper.num2 },
                    popIndexData = new Dictionary<int, byte[]> { { 2, OpTestCaseHelper.num2 } },
                },
                Err.OpNotEnoughItems
            };
            yield return new object[]
            {
                new MockOpData(FuncCallName.Pop, FuncCallName.PopIndex,
                               FuncCallName.PopCount, FuncCallName.PopCount,
                               FuncCallName.Pop)
                {
                    _itemCount = 7,
                    _opCountToReturn = 0,
                    popData = new byte[][] { OpTestCaseHelper.b7, OpTestCaseHelper.num2 },
                    popIndexData = new Dictionary<int, byte[]> { { 2, OpTestCaseHelper.num2 } },
                    popCountData = new byte[][][]
                    {
                        new byte[][]
                        {
                            KeyHelper.Pub2CompBytes, KeyHelper.Pub3CompBytes
                        },
                        new byte[][]
                        {
                            Helper.ShortSig1Bytes, Helper.ShortSig2Bytes
                        },
                    },
                    expectedPubkeys = new byte[][] { KeyHelper.Pub2CompBytes, KeyHelper.Pub3CompBytes},
                    expectedSigs = new byte[][] { Helper.ShortSig1Bytes, Helper.ShortSig2Bytes },
                    expectedMultiSigGarbage = OpTestCaseHelper.b7,
                    garbageCheckResult = false
                },
                "The extra item should be OP_0."
            };
        }
        [Theory]
        [MemberData(nameof(GetErrorCases))]
        public void Run_ErrorTest(MockOpData data, string expError)
        {
            OpTestCaseHelper.RunFailTest<CheckMultiSigOp>(data, expError);
        }
    }
}
