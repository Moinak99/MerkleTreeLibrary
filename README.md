# MerkleTreeLibrary

This is my C# library to calculate the Merkle Root for an array of strings. It uses a BIP340-compatible tagged hash (SHA-256) to ensure unique roots.

## What It Does
The library takes an array of strings (like ["aaa", "bbb"]) and computes a single Merkle Root hash. I added a simple test method to print the result.

## How to Set It Up
1. Clone this repo: `git clone <reponame>`
2. Open it in your  IDE (I use VS Code with .NET SDK 9.0).
3. Build it with `dotnet build` - should be smooth if you’ve got the SDK!
4. Reference it in your project by adding `<ProjectReference Include="..\MerkleTreeLibrary\MerkleTreeLibrary.csproj" />` to your .csproj.

## Usage Example
Here’s how I tested it -

```csharp
using MerkleTreeLib;

string[] inputs = { "aaa", "bbb", "ccc" };
MerkleTree.RunTests(inputs);
// Output: Merkle Root for aaa, bbb, ccc, ddd, eee: 33ce01bab47b07c208ccc2e2adfa62949b0209c547fd01087e52c12c259ec30c
