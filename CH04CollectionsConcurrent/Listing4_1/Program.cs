using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// Added for the Stopwatch
using System.Diagnostics;
// Added for the cryptography classes
using System.Security.Cryptography;
// This namespace will be used later to run code in parallel
using System.Threading.Tasks;
// Added to access the custom partitioner
using System.Collections.Concurrent;

namespace Listing4_1
{
    class Program
    {
        private const int NUM_AES_KEYS = 800000;

        private static string ConvertToHexString(Byte[] byteArray)
        {
            // Convert the byte array to hexadecimal string
            var sb = new StringBuilder(byteArray.Length);

            for (int i = 0; i < byteArray.Length; i++)
            {
                sb.Append(byteArray[i].ToString("X2"));
            }

            return sb.ToString();
        }

        private static List<string> _keysList;

        private static void ParallelPartitionGenerateAESKeys()
        {
            var sw = Stopwatch.StartNew();
            Parallel.ForEach(Partitioner.Create(1, NUM_AES_KEYS + 1), range =>
            {
                var aesM = new AesManaged();
                Debug.WriteLine("AES Range ({0}, {1}. Time: {2})",
                                range.Item1, range.Item2, 
                                DateTime.Now.TimeOfDay);
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    aesM.GenerateKey();
                    byte[] result = aesM.Key;
                    string hexString = ConvertToHexString(result);

                    lock (_keysList)
                    {
                        // Critical code section
                        // It is safe to add elements to the List
                        _keysList.Add(hexString);
                    }
                    // Console.WriteLine("AES KEY: {0} ", hexString);
                }
            });
            Debug.WriteLine("AES: " + sw.Elapsed.ToString());
        }

        static void Main(string[] args)
        {
            var sw = Stopwatch.StartNew();

            _keysList = new List<string>(NUM_AES_KEYS);

            ParallelPartitionGenerateAESKeys();

            Console.WriteLine("Number of keys in the list: {0}", 
                _keysList.Count());
            Debug.WriteLine(sw.Elapsed.ToString());
            //  Display the results and wait for the user to press a key
            Console.WriteLine("Finished!");
            Console.ReadLine();
        }
    }
}