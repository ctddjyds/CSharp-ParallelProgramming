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

namespace Listing4_4
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

        private static ConcurrentQueue<Byte[]> _byteArraysQueue;
        private static ConcurrentQueue<string> _keysQueue;

        private static void ParallelPartitionGenerateAESKeys(int maxDegree)
        {
            var parallelOptions = new ParallelOptions();
            parallelOptions.MaxDegreeOfParallelism = maxDegree;

            var sw = Stopwatch.StartNew();
            Parallel.ForEach(
               Partitioner.Create(1, NUM_AES_KEYS + 1), 
               parallelOptions, range =>
            {
                var aesM = new AesManaged();
                Debug.WriteLine("AES Range ({0}, {1}. Time: {2})",
                                range.Item1, range.Item2, 
                                DateTime.Now.TimeOfDay);
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    aesM.GenerateKey();
                    byte[] result = aesM.Key;
                    _byteArraysQueue.Enqueue(result);
                }
            });
            Debug.WriteLine("AES: " + sw.Elapsed.ToString());
        }

        private static void ConvertAESKeysToHex(Task taskProducer)
        {
            var sw = Stopwatch.StartNew();
            // This condition running in a loop (spinning) is very inefficient
            // This example uses this spinning for educational purposes
            // It isn’t a best practice
            // Subsequent sections and chapters explain an improved version
            while ((taskProducer.Status == TaskStatus.Running) ||
                   (taskProducer.Status == TaskStatus.WaitingToRun) || 
                   (_byteArraysQueue.Count() > 0))
            {
                Byte[] result;
                if (_byteArraysQueue.TryDequeue(out result))
                {
                    string hexString = ConvertToHexString(result);
                    _keysQueue.Enqueue(hexString);
                }
            }
            Debug.WriteLine("HEX: " + sw.Elapsed.ToString());
        }


        static void Main(string[] args)
        {
            var sw = Stopwatch.StartNew();

            _byteArraysQueue = new ConcurrentQueue<byte[]>();
            _keysQueue = new ConcurrentQueue<string>();

            var taskAESKeys = Task.Factory.StartNew(
                () => ParallelPartitionGenerateAESKeys(Environment.ProcessorCount - 1));
            var taskHexStrings = Task.Factory.StartNew(
                () => ConvertAESKeysToHex(taskAESKeys));

            string lastKey;
            // This condition running in a loop (spinning) is very inefficient
            // This example uses this spinning for educational purposes
            // It isn’t a best practice
            // Subsequent sections and chapters explain an improved version
            while ((taskHexStrings.Status == TaskStatus.Running) ||
                   (taskHexStrings.Status == TaskStatus.WaitingToRun))
            {
                // Display partial results
                var countResult = _keysQueue.Count(key => key.Contains("F"));

                Console.WriteLine("So far, the number of keys that contain an F is: {0}", countResult);
                if (_keysQueue.TryPeek(out lastKey))
                {
                    Console.WriteLine("The first key in the queue is: {0}", lastKey);
                }
                else
                {
                    Console.WriteLine("No keys yet.");
                }
                // Sleep the main thread for 0.5 seconds
                System.Threading.Thread.Sleep(500);
            }

            Task.WaitAll(taskAESKeys, taskHexStrings);

            Console.WriteLine("Number of keys in the list: {0}", _keysQueue.Count());
            Debug.WriteLine(sw.Elapsed.ToString());
            //  Display the results and wait for the user to press a key
            Console.WriteLine("Finished!");
            Console.ReadLine();
        }
    }
}