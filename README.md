[![Build Status](https://travis-ci.org/Autarkysoft/Denovo.svg?branch=master)](https://travis-ci.org/Autarkysoft/Denovo)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/Autarkysoft/Denovo/blob/master/License)
[![NuGet](https://img.shields.io/nuget/v/Autarkysoft.Bitcoin.svg)](https://www.nuget.org/packages/Autarkysoft.Bitcoin)

<p align="center"><b>The Revolution Will Not Be Centralized</b></p>

# Denovo
Denovo is a stand alone bitcoin client written completely in C# and from scratch and with only one dependency for GUI. Using the latest 
[.net core](https://github.com/dotnet/core) version with [AvaloniaUI](https://github.com/AvaloniaUI/Avalonia) it can run on any 
operating systems.  
- Development is not started yet.
- Current version `0.0.0.0` (check the [versioning convention](https://github.com/Autarkysoft/Conventions/blob/master/Versioning.md)
for more information).

# Bitcoin.Net
The backbone of Denovo, Bitcoin.net is a stand alone bitcoin library written completely in C# and from scratch (no code translating)
with no dependencies. 
It is released as a different project so that it could be used by any other third party projects.  
Current version is `0.1.0.0` (check the [versioning convention](https://github.com/Autarkysoft/Conventions/blob/master/Versioning.md)
for more information). This is first release of the library. The implementation is covering almost the entire bitcoin protocol,
there may be some missing parts or some bugs.  
Please report any problems that you encounter or any feedback that you may have.    

### Bitcoin.Net can be downloaded from Nuget:  
Using Package manager in Visual Studio:  

    Install-Package Autarkysoft.Bitcoin
    
Using .Net CLI:  

    dotnet add package Autarkysoft.Bitcoin

### Current Features
* Full xml documentation of the code explaining what each member does, expections that may be thrown, examples if needed,...
* Neatly categorized namespaces for ease of access: `Blockchain`, `Cryptography`, `P2PNetwork` are the 3 main ones and there are
`Encoders`, `ImprovementProposals` covering the rest.
* Near 100% test coverage (for finished parts only, _for now_).
* Loosely coupled implementation of blocks, transactions and scripts making it easy to test and scale.
* Stand alone cryptography namespace making it possible to optimize functions for bitcoin 
(only some parts are currently optimized: `Hashing` and `KeyDerivationFunctions` namespaces)
  * Asymmetric: ECC (unoptimized and untested)
  * Hashing: SHA-1 (unoptimized), SHA-2 (256/512), HMAC-SHA (256/512), RIPEMD160, RIPEMD160 of SHA256 (aka Hash160) all optimized
  * KeyDerivationFunctions: PBKDF2, Scrypt both optimized
  * RFC-6979: Optimized. Also an extra entropy is added so that signer can grind to find low R values to a fixed length (<32).
* Implementation of improvement proposals, consensus related BIPs are part of the library and optional bips (eg. BIP-32)
are in separate classes. Currently:
  * BIPs: [16](https://github.com/bitcoin/bips/blob/master/bip-0016.mediawiki "Pay to Script Hash"), 
  [34](https://github.com/bitcoin/bips/blob/master/bip-0034.mediawiki "Block v2, Height in Coinbase"), 
  [65](https://github.com/bitcoin/bips/blob/master/bip-0065.mediawiki "OP_CheckLocktimeVerify"), 
  [66](https://github.com/bitcoin/bips/blob/master/bip-0066.mediawiki "Strict DER signatures"), 
  [68](https://github.com/bitcoin/bips/blob/master/bip-0068.mediawiki "Relative lock-time using consensus-enforced sequence numbers"), 
  [112](https://github.com/bitcoin/bips/blob/master/bip-0112.mediawiki "OP_CheckSequenceVerify"), 
  [141](https://github.com/bitcoin/bips/blob/master/bip-0141.mediawiki "Segregated Witness (Consensus layer)"), 
  [143](https://github.com/bitcoin/bips/blob/master/bip-0143.mediawiki "Transaction Signature Verification for Version 0 Witness Program"), 
  [144](https://github.com/bitcoin/bips/blob/master/bip-0144.mediawiki "Segregated Witness (Peer Services)"), 
  [147](https://github.com/bitcoin/bips/blob/master/bip-0147.mediawiki "Dealing with dummy stack element malleability"), 
  [173](https://github.com/bitcoin/bips/blob/master/bip-0173.mediawiki "Base32 address format for native v0-16 witness outputs")
  * [BIP-14](https://github.com/bitcoin/bips/blob/master/bip-0014.mediawiki "Protocol Version and User Agent")
  * [BIP-21](https://github.com/bitcoin/bips/blob/master/bip-0021.mediawiki "URI Scheme")
  * [BIP-32](https://github.com/bitcoin/bips/blob/master/bip-0032.mediawiki "Hierarchical Deterministic Wallets")
  * [BIP-39](https://github.com/bitcoin/bips/blob/master/bip-0039.mediawiki "Mnemonic code for generating deterministic keys")
  * [BIP-178](https://github.com/bitcoin/bips/blob/master/bip-0178.mediawiki "Version Extended WIF")
  * [BIP-340](https://github.com/bitcoin/bips/blob/master/bip-0340.mediawiki "Schnorr Signatures for secp256k1")

### Future plans
* Optimization of the libray
* Complete testing of remaining parts
* Add more relevant and useful BIPs
* Explore more ideas for a better Bitcoin (eg. block compressions and P2P protocol) to add under `Experimental` namespace.

## Contributing
Please check out [conventions](https://github.com/Autarkysoft/Conventions) for information about coding styles, versioning, 
making pull requests, and more.
