using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace MerkleTreeLib
{
    static class Constants
    {
        public const string tagValue = "Bitcoin_Transaction";
    }

    public class MerkleTree
    {
        /// <summary>
        /// Crafts a BIP340-style tagged hash for any message, using a tag like 'Bitcoin_Transaction'.
        /// I use this to make sure each hash is unique for the Merkle Tree.
        /// </summary>
        /// <param name="msg">The message to hash, like a user string (e.g., 'aaa')</param>
        /// <param name="tag">The tag to make it unique, like 'Bitcoin_Transaction'</param>
        /// <returns>A hex string of the hash, no dashes, all lowercase</returns>
        public static string TaggedHash(string msg, string tag)
        {
            using var sha256 = SHA256.Create();
            byte[] tagHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(tag));
            byte[] messageBytes = Encoding.UTF8.GetBytes(msg);
            byte[] preimage = new byte[tagHash.Length * 2 + messageBytes.Length];
            Array.Copy(tagHash, 0, preimage, 0, tagHash.Length);
            Array.Copy(tagHash, 0, preimage, tagHash.Length, tagHash.Length);
            Array.Copy(messageBytes, 0, preimage, tagHash.Length * 2, messageBytes.Length);
            byte[] hash = sha256.ComputeHash(preimage);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        /// <summary>
        /// Builds a Merkle Root from an array of strings, using a single tag for all levels.
        /// This is my old-school way from Task 1—simple but effective!
        /// </summary>
        /// <param name="inputs">Array of strings to hash, like ['aaa', 'bbb']</param>
        /// <param name="tag">The tag, set to 'Bitcoin_Transaction' per spec</param>
        /// <returns>The final Merkle Root as a hex string</returns>
        /// <exception cref="ArgumentException">Yells if the input list is empty or null</exception>
        public static string CalculateMerkleRoot(string[] inputs, string tag)
        {
            if (inputs == null || inputs.Length == 0)
                throw new ArgumentException("Input array cannot be empty");

            List<string> hashes = new List<string>(); // Holds the hashes as we build the tree
            foreach (var input in inputs)
            {
                hashes.Add(TaggedHash(input, tag)); // Hash each input to start the tree
            }

            while (hashes.Count > 1)
            {
                List<string> newHashes = new List<string>(); 
                for (int i = 0; i < hashes.Count; i += 2)
                {
                    string left = hashes[i];
                    string right = i + 1 < hashes.Count ? hashes[i + 1] : left; // Duplicate if odd count
                    string combined = left + right;
                    newHashes.Add(TaggedHash(combined, tag)); // Hash the pair for the parent
                }
                hashes = newHashes; 
            }

            return hashes[0];
        }

        private static byte[] StringToByteArray(string hex)
        {
            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16); // Convert hex pairs to bytes
            return bytes;
        }

        /// <summary>
        /// Computes the Merkle Root and tracks proof paths for each input.
        /// I added this for Task 2—uses different tags for leaves and branches.
        /// </summary>
        /// <param name="inputs">Input strings, like ["(1,1111)", "(2,2222)"]</param>
        /// <param name="leafTag">Tag for leaf hashes, "ProofOfReserve_Leaf"</param>
        /// <param name="branchTag">Tag for branch hashes, "ProofOfReserve_Branch"</param>
        /// <returns>A tuple with the root and a proof dictionary</returns>
        /// <exception cref="ArgumentException">Throws if inputs are null or empty</exception>
        public static (string MerkleRoot, Dictionary<string, List<(string Hash, int Side)>> Proofs) CalculateMerkleRootWithProofs(string[] inputs, string leafTag, string branchTag)
        {
            if (inputs == null || inputs.Length == 0)
                throw new ArgumentException("Input array cannot be empty");

            List<string> hashes = new List<string>(); // Current level of hashes
            Dictionary<string, List<(string Hash, int Side)>> proofs = new Dictionary<string, List<(string Hash, int Side)>>(); // Proofs for each leaf

            foreach (var input in inputs)
            {
                string leafHash = TaggedHash(input, leafTag); // Hash the leaf with its tag
                hashes.Add(leafHash);
                proofs[leafHash] = new List<(string Hash, int Side)>(); // Start a proof list
            }

            while (hashes.Count > 1)
            {
                List<string> newHashes = new List<string>(); // Next level of hashes
                for (int i = 0; i < hashes.Count; i += 2)
                {
                    string left = hashes[i];
                    string right = i + 1 < hashes.Count ? hashes[i + 1] : left; // Mirror if odd
                    string combined = left + right;
                    string parentHash = TaggedHash(combined, branchTag); // Hash branch with its tag
                    newHashes.Add(parentHash);

                    if (proofs.ContainsKey(left))
                        proofs[left].Add((right, 1)); // Right sibling for left's proof
                    if (i + 1 < hashes.Count && proofs.ContainsKey(right))
                        proofs[right].Add((left, 0)); // Left sibling for right's proof
                }
                hashes = newHashes; 
            }

            return (hashes[0], proofs); // Root and all proofs
        }

        /// <summary>
        /// Fetches the proof path for a specific input from the precomputed proofs.
        /// Handy for verifying a user's place in the tree!
        /// </summary>
        /// <param name="input">The input string to find, e.g., "(1,1111)"</param>
        /// <param name="leafTag">Tag for leaf hashing, "ProofOfReserve_Leaf"</param>
        /// <param name="proofs">The proof dictionary from CalculateMerkleRootWithProofs</param>
        /// <returns>A tuple with the leaf hash and its proof path</returns>
        public static (string LeafHash, List<(string Hash, int Side)> Path) GetMerkleProof(
            string input, string leafTag, Dictionary<string, List<(string Hash, int Side)>> proofs)
        {
            string leafHash = TaggedHash(input, leafTag);
            if (!proofs.ContainsKey(leafHash))
                return (leafHash, new List<(string Hash, int Side)>()); // No proof if unknown

            List<(string Hash, int Side)> path = new List<(string Hash, int Side)>(proofs[leafHash]);
            return (leafHash, path); // Return the proof path
        }

        /// <summary>
        /// Runs a quick test to print the Merkle Root for given inputs.
        /// This was my debug tool for Task 1—kept it for nostalgia!
        /// </summary>
        /// <param name="inputs">Array of strings to test</param>
        public static void RunTests(string[] inputs)
        {
            string merkleRoot = CalculateMerkleRoot(inputs, Constants.tagValue);
            Console.WriteLine($"Merkle Root for {string.Join(", ", inputs)}: {merkleRoot}");
        }
    }
}