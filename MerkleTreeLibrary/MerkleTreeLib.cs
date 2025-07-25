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

        // Calculate Merkle Root for an array of strings
        public static string CalculateMerkleRoot(string[] inputs, string tag)
        {
            if (inputs == null || inputs.Length == 0)
                throw new ArgumentException("Input array cannot be empty");

            // Compute leaf hashes
            List<string> hashes = new List<string>();
            foreach (var input in inputs)
            {
                hashes.Add(TaggedHash(tag, input));
            }

            // Build Merkle Tree
            while (hashes.Count > 1)
            {
                List<string> newHashes = new List<string>();
                for (int i = 0; i < hashes.Count; i += 2)
                {
                    string left = hashes[i];
                    string right = i + 1 < hashes.Count ? hashes[i + 1] : left; // Duplicate last hash if odd
                    byte[] combined = StringToByteArray(left + right);
                    using SHA256 sha256 = SHA256.Create();
                    byte[] parentHash = sha256.ComputeHash(combined);
                    newHashes.Add(BitConverter.ToString(parentHash).Replace("-", "").ToLower());
                }
                hashes = newHashes;
            }

            return hashes[0];
        }

        // Helper to convert hex string to byte array
        private static byte[] StringToByteArray(string hex)
        {
            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        // Test method
        public static void RunTests(string[] inputs)
        {
            string merkleRoot = CalculateMerkleRoot(inputs, Constants.tagValue);
            Console.WriteLine($"Merkle Root for {string.Join(", ", inputs)}: {merkleRoot}");
        }
    }

}