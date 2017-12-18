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

namespace Listing4_5
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
        private static ConcurrentQueue<string> _validKeys;

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

        private static string[] _invalidHexValues = { "AF", "BD", "BF", "CF", "DA", "FA", "FE", "FF" };
        private static int MAX_INVALID_HEX_VALUES = 3;
        private static int tasksHexStringsRunning = 0;

        private static bool IsKeyValid(string key)
        {
            var sw = Stopwatch.StartNew();
            int count = 0;
            for (int i = 0; i < _invalidHexValues.Length; i++)
            {
                if (key.Contains(_invalidHexValues[i]))
                {
                    count++;
                    if (count == MAX_INVALID_HEX_VALUES)
                    {
                        return true;
                    }
                    if (((_invalidHexValues.Length - i) + count) < MAX_INVALID_HEX_VALUES)
                    {
                        // There are no chances of being an invalid key
                        return false;
                    }
                }
            }
            return false;
        }

        private static void ValidateKeys()
        {
            var sw = Stopwatch.StartNew();
            // This condition running in a loop (spinning) is very inefficient
            // This example uses this spinning for educational purposes
            // It isn’t a best practice
            // Subsequent sections and chapters explain an improved version
            while ((tasksHexStringsRunning > 0) ||
                   (_keysQueue.Count() > 0))
            {
                string hexString;
                if (_keysQueue.TryDequeue(out hexString))
                {
                    if (IsKeyValid(hexString))
                    {
                        _validKeys.Enqueue(hexString);
                    }
                }
            }
            Debug.WriteLine("VALIDATE: " + sw.Elapsed.ToString());
        }

        static void Main(string[] args)
        {
            var sw = Stopwatch.StartNew();

            _byteArraysQueue = new ConcurrentQueue<byte[]>();
            _keysQueue = new ConcurrentQueue<string>();
            _validKeys = new ConcurrentQueue<string>();

            // This code requires at least 3 logical cores
            int taskAESKeysMax = Environment.ProcessorCount / 2;
            // Use the remaining logical cores - 1
            // to create parallelized tasks to run many consumers
            int taskHexStringsMax = Environment.ProcessorCount - taskAESKeysMax - 1;
            var taskAESKeys = Task.Factory.StartNew(() => ParallelPartitionGenerateAESKeys(taskAESKeysMax));
            Task[] tasksHexStrings = new Task[taskHexStringsMax];
            for (int i = 0; i < taskHexStringsMax; i++)
            {
                // Increment tasksHexStringsRunning as an atomic operation
                System.Threading.Interlocked.Increment(ref tasksHexStringsRunning);

                tasksHexStrings[i] = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        ConvertAESKeysToHex(taskAESKeys);
                    }
                    finally
                    {
                        // Decrement tasksHexStringsRunning as an atomic operation
                        System.Threading.Interlocked.Decrement(ref tasksHexStringsRunning);
                    }
                });
            }
            var taskValidateKeys = Task.Factory.StartNew(() => ValidateKeys());

            taskValidateKeys.Wait();

            Console.WriteLine("Number of keys in the list: {0}", _keysQueue.Count());
            Console.WriteLine("Number of valid keys: {0}", _validKeys.Count());

            Debug.WriteLine(sw.Elapsed.ToString());
            //  Display the results and wait for the user to press a key
            Console.WriteLine("Finished!");
            Console.ReadLine();
        }
    }
}