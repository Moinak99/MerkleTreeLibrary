using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace MerkleTreeLib
{
    static class Constants
    {
        public const String tagValue = "Bitcoin_Transaction";
    }
    class MerkleTree
    {
        /// <summary>
        /// Computes a BIP340-compatible tagged hash for a given message using a specified tag(Bitcoin_Transaction).
        /// This method is used to create unique hashes for blockchain data, such as in the Merkle Tree.
        /// </summary>
        /// <param name="msg">The input message to be hashed, such as a user string (e.g., 'aaa')</param>
        /// <param name="tag">The tag value used for hashing, such as 'Bitcoin_Transaction' to ensure uniqueness</param>
        /// <returns>A hex-encoded string representing the computed tagged hash</returns>
        public static String TaggedHash(String msg, String tag)
        {
            using SHA256 sHA256 = SHA256.Create();
            byte[] tagHash = sHA256.ComputeHash(Encoding.UTF8.GetBytes(tag));
            byte[] messageBytes = Encoding.UTF8.GetBytes(msg);
            byte[] preimage = new byte[tagHash.Length * 2 + messageBytes.Length];
            Array.Copy(tagHash, 0, preimage, 0, tagHash.Length);
            Array.Copy(tagHash, 0, preimage, tagHash.Length, tagHash.Length);
            Array.Copy(messageBytes, 0, preimage, tagHash.Length * 2, messageBytes.Length);
            byte[] hash = sHA256.ComputeHash(preimage);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        /// <summary>
        /// Calculates the Merkle Root for an array of input strings using a BIP340-compatible tagged hash.
        /// This method builds a Merkle Tree where all levels (leaves and parents) use the specified tag
        /// </summary>
        /// <param name="inputs">The array of strings to be hashed into the Merkle Tree (e.g., ['aaa', 'bbb'])</param>
        /// <param name="tag">The tag value for the tagged hash, set to 'Bitcoin_Transaction' as required</param>
        /// <returns>The hex-encoded Merkle Root hash summarizing all inputs</returns>
        /// <exception cref="ArgumentException">Thrown if the input array is null or empty</exception>
        public static string CalculateMerkleRoot(string[] inputs, string tag)
        {
            if (inputs == null || inputs.Length == 0)
                throw new ArgumentException("Input array cannot be empty");

            // Compute leaf hashes
            List<string> hashes = new List<string>();
            foreach (var input in inputs)
            {
                hashes.Add(TaggedHash(input, tag));
            }

            // Build Merkle Tree with TaggedHash for all levels
            while (hashes.Count > 1)
            {
                List<string> newHashes = new List<string>();
                for (int i = 0; i < hashes.Count; i += 2)
                {
                    string left = hashes[i];
                    string right = i + 1 < hashes.Count ? hashes[i + 1] : left;
                    string combined = left + right;
                    newHashes.Add(TaggedHash(combined, tag));
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
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public static void RunTests(string[] inputs)
        {
            string merkleRoot = CalculateMerkleRoot(inputs, Constants.tagValue);
            Console.WriteLine($"Merkle Root for {string.Join(", ", inputs)}: {merkleRoot}");
        }
    }
}