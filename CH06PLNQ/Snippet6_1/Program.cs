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

namespace Snippet6_1
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

        private static ConcurrentQueue<String> Keys;

        private static void ParallelPartitionGenerateAESKeysWCP(System.Threading.CancellationToken ct, char prefix)
        {
            var sw = Stopwatch.StartNew();
            var parallelOptions = new ParallelOptions();
            // Set the CancellationToken for the ParallelOptions instance
            parallelOptions.CancellationToken = ct;

            Parallel.ForEach(Partitioner.Create(1, NUM_AES_KEYS + 1), parallelOptions, range =>
            {
                var aesM = new AesManaged();
                Debug.WriteLine("AES Range ({0}, {1}. TimeOfDay before inner loop starts: {2})",
                                range.Item1, range.Item2, DateTime.Now.TimeOfDay);
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    aesM.GenerateKey();
                    byte[] result = aesM.Key;
                    string hexString = ConvertToHexString(result);
                    // Console.WriteLine("AES KEY: {0} ", hexString);
                    if (hexString[0] == prefix)
                    {
                        Keys.Enqueue(hexString);
                    }
                    parallelOptions.CancellationToken.ThrowIfCancellationRequested();
                }
            });
            Debug.WriteLine("AES: " + sw.Elapsed.ToString());
        }

        static int CountLetters(String key)
        {
            int letters = 0;
            for (int i = 0; i < key.Length; i++)
            {
                if (Char.IsLetter(key, i))
                {
                    letters++;
                }
            }
            return letters;
        }

        static void Main(string[] args)
        {
            Console.ReadLine();
            Console.WriteLine("Started");

            var cts = new System.Threading.CancellationTokenSource();
            var ct = cts.Token;

            var sw = Stopwatch.StartNew();

            Keys = new ConcurrentQueue<String>();

            var tAsync = new Task(() => ParallelPartitionGenerateAESKeysWCP(ct, 'A'));
            tAsync.Start();

            // Do something else
            // Wait for tAsync to finish
            tAsync.Wait();

            // Define the query indicating that it should run in parallel
            var keysWith10Letters = from key in Keys.AsParallel()
                        where (CountLetters(key) >= 10) 
                            && (key.Contains('A')) 
                            && (key.Contains('F')) 
                            && (key.Contains('9')) 
                            && (!key.Contains('B'))
                        select key;

            // Write some of the results of executing the query
            // Remember that the PLINQ query is going to be executed at this point
            // when the code requires results
            var keysList = keysWith10Letters.ToList();
            Console.WriteLine("The code generated {0} keys with at least ten letters, A, F and 9 but no B in the hexadecimal code.",
                keysList.Count());
            Console.WriteLine("First key: {0} ",
                keysList.ElementAt(0));
            Console.WriteLine("Last key: {0} ",
                keysList.ElementAt(keysWith10Letters.Count() - 1));

            Console.WriteLine("Finished in {0}", sw.Elapsed.ToString());

            Console.ReadLine();
        }
    }
}
