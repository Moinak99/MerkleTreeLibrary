using MerkleTreeLib;

namespace MerkleTreeLibrary
{
    class Program
    {
        static void Main(string[] args)
        {   // driver code
            string[] inputs = { "aaa", "bbb", "ccc", "ddd", "eee" };
            MerkleTree.RunTests(inputs);
        }
    }
}