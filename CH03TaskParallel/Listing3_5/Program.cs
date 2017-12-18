using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// Added for the Stopwatch
using System.Diagnostics;
// Added for the cryptography classes
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Listing3_5
{
    class Program
    {
        private const int NUM_AES_KEYS = 800000;
        private const int NUM_MD5_HASHES = 100000;

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

        private static void GenerateAESKeys()
        {
            var sw = Stopwatch.StartNew();
            var aesM = new AesManaged();
            for (int i = 1; i <= NUM_AES_KEYS; i++)
            {
                aesM.GenerateKey();
                byte[] result = aesM.Key;
                string hexString = ConvertToHexString(result);
                // Console.WriteLine("AES KEY: {0} ", hexString);
            }
            Debug.WriteLine("AES: " + sw.Elapsed.ToString());
        }

        private static void ParallelGenerateAESKeys()
        {
            var sw = Stopwatch.StartNew();
            Parallel.For(1, NUM_AES_KEYS + 1, (int i) =>
            {
                var aesM = new AesManaged();
                byte[] result = aesM.Key;
                string hexString = ConvertToHexString(result);
                // Console.WriteLine(“AES KEY: {0} “, hexString);
            });
            Debug.WriteLine("AES: " + sw.Elapsed.ToString());
        }

        private static void GenerateMD5Hashes()
        {
            var sw = Stopwatch.StartNew();
            var md5M = MD5.Create();
            for (int i = 1; i <= NUM_MD5_HASHES; i++)
            {
                byte[] data =
                    Encoding.Unicode.GetBytes(
                        Environment.UserName + i.ToString());
                byte[] result = md5M.ComputeHash(data);
                string hexString = ConvertToHexString(result);
                // Console.WriteLine("MD5 HASH: {0}", hexString);
            }
            Debug.WriteLine("MD5: " + sw.Elapsed.ToString());
        }

        private static void ParallelGenerateMD5Hashes()
        {
            var sw = Stopwatch.StartNew();
            Parallel.For(1, NUM_MD5_HASHES + 1, (int i) =>
            {
                var md5M = MD5.Create();
                byte[] data =
                    Encoding.Unicode.GetBytes(
                        Environment.UserName + i.ToString());
                byte[] result = md5M.ComputeHash(data);
                string hexString = ConvertToHexString(result);
                // Console.WriteLine(“MD5 HASH: {0}”, hexString);
            });
            Debug.WriteLine("MD5: " + sw.Elapsed.ToString());
        }


        private static void ParallelPartitionGenerateAESKeys()
        {
            var sw = Stopwatch.StartNew();
            Parallel.ForEach(
                Partitioner.Create(
                    1,
                    NUM_AES_KEYS,
                    ((int)(NUM_AES_KEYS / Environment.ProcessorCount) + 1)),
            range =>
            {
                var aesM = new AesManaged();
                Debug.WriteLine(
                    "AES Range ({0}, {1}. TimeOfDay before inner loop starts: {2})",
                    range.Item1, range.Item2,
                    DateTime.Now.TimeOfDay);
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    aesM.GenerateKey();
                    byte[] result = aesM.Key;
                    string hexString = ConvertToHexString(result);
                    // Console.WriteLine(“AES KEY: {0} “, hexString);
                }
            });
            Debug.WriteLine("AES: " + sw.Elapsed.ToString());
        }

        private static void ParallelPartitionGenerateMD5Hashes()
        {
            var sw = Stopwatch.StartNew();
            Parallel.ForEach(
                Partitioner.Create(
                    1,
                    NUM_MD5_HASHES,
                    ((int)(NUM_MD5_HASHES / Environment.ProcessorCount) + 1)),
            range =>
            {
                var md5M = MD5.Create();
                Debug.WriteLine(
                    "MD5 Range ({0}, {1}. TimeOfDay before inner loop starts: {2})",
                    range.Item1, range.Item2,
                    DateTime.Now.TimeOfDay);
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    byte[] data =
                        Encoding.Unicode.GetBytes(
                            Environment.UserName + i.ToString());
                    byte[] result = md5M.ComputeHash(data);
                    string hexString = ConvertToHexString(result);
                    // Console.WriteLine(“MD5 HASH: {0}”, hexString);
                }
            });
            Debug.WriteLine("MD5: " + sw.Elapsed.ToString());
        }

        private static void DisplayParallelLoopResult(ParallelLoopResult loopResult)
        {
            string text;
            if (loopResult.IsCompleted)
            {
                text = "The loop ran to completion.";
            }
            else
            {
                if (loopResult.LowestBreakIteration.HasValue)
                {
                    text = "The loop ended by calling the Break statement.";
                }
                else
                {
                    text = "The loop ended prematurely with a Stop statement.";
                }
            }
            Console.WriteLine(text);
        }

        private static IEnumerable<int> GenerateMD5InputData()
        {
            // Generate a sequence of NUM_MD5_HASHES integral numbers
            // The value of the first integer in the sequence (start) is 1
            // The number of sequential integers to generate (count)
            // is NUM_MD5_HASHES
            return Enumerable.Range(1, NUM_MD5_HASHES);
        }

        private static void ParallelForEachGenerateMD5Hashes()
        {
            var sw = Stopwatch.StartNew();
            var inputData = GenerateMD5InputData();
            Parallel.ForEach(inputData, (int number) =>
            {
                var md5M = MD5.Create();
                byte[] data =
                    Encoding.Unicode.GetBytes(
                        Environment.UserName + number.ToString());
                byte[] result = md5M.ComputeHash(data);
                string hexString = ConvertToHexString(result);
                // Console.WriteLine("MD5 HASH: {0}", hexString);
            });
            Debug.WriteLine("MD5: " + sw.Elapsed.ToString());
        }

        private static void ParallelForEachGenerateMD5HashesBreak()
        {
            var sw = Stopwatch.StartNew();
            var inputData = GenerateMD5InputData();
            var loopResult = Parallel.ForEach(inputData,
            (int number, ParallelLoopState loopState) =>
            {
                // There is very little value in doing this first thing
                // in the loop, since the loop itself is checking
                // the same condition prior to invoking the body delegate
                // Therefore, the following if statement appears commented
                //if (loopState.ShouldExitCurrentIteration)
                //{
                // return;
                //}
                var md5M = MD5.Create();
                byte[] data =
                    Encoding.Unicode.GetBytes(
                        Environment.UserName + number.ToString());
                byte[] result = md5M.ComputeHash(data);
                string hexString = ConvertToHexString(result);
                // Console.WriteLine("MD5 HASH: {0}", hexString);
                if (sw.Elapsed.Seconds > 3)
                {
                    loopState.Break();
                    return;
                }
            });
            DisplayParallelLoopResult(loopResult);
            Debug.WriteLine("MD5: " + sw.Elapsed.ToString());
        }

        private static void ParallelForEachGenerateMD5HashesException()
        {
            var sw = Stopwatch.StartNew();
            var inputData = GenerateMD5InputData();
            var loopResult = new ParallelLoopResult();
            try
            {
                loopResult = Parallel.ForEach(inputData,
                (int number, ParallelLoopState loopState) =>
                {
                    // There is very little value in doing this first thing
                    // in the loop, since the loop itself is checking
                    // the same condition prior to invoking the body delegate
                    // Therefore, the following if statement appears commented
                    //if (loopState.ShouldExitCurrentIteration)
                    //{
                    // return;
                    //}
                    var md5M = MD5.Create();
                    byte[] data =
                        Encoding.Unicode.GetBytes(
                            Environment.UserName + number.ToString());
                    byte[] result = md5M.ComputeHash(data);
                    string hexString = ConvertToHexString(result);
                    // Console.WriteLine("MD5 HASH: {0}", hexString);
                    if (sw.Elapsed.Seconds > 3)
                    {
                        throw new TimeoutException(
                            "Parallel.ForEach is taking more than 3 seconds to complete.");
                    }
                });
            }
            catch (AggregateException ex)
            {
                foreach (Exception innerEx in ex.InnerExceptions)
                {
                    Debug.WriteLine(innerEx.ToString());
                    // Do something considering the innerEx Exception
                }
            }
            DisplayParallelLoopResult(loopResult);
            Debug.WriteLine("MD5: " + sw.Elapsed.ToString());
        }

        private static void ParallelGenerateAESKeysMaxDegree(int maxDegree)
        {
            var parallelOptions = new ParallelOptions();
            parallelOptions.MaxDegreeOfParallelism = maxDegree;
            var sw = Stopwatch.StartNew();
            Parallel.For(1, NUM_AES_KEYS + 1, parallelOptions, (int i) =>
            {
                var aesM = new AesManaged();
                byte[] result = aesM.Key;
                string hexString = ConvertToHexString(result);
                // Console.WriteLine(“AES KEY: {0} “, hexString);
            });
            Debug.WriteLine("AES: " + sw.Elapsed.ToString());
        }
        private static void ParallelGenerateMD5HashesMaxDegree(int maxDegree)
        {
            var parallelOptions = new ParallelOptions();
            parallelOptions.MaxDegreeOfParallelism = maxDegree;
            var sw = Stopwatch.StartNew();
            Parallel.For(1, NUM_MD5_HASHES + 1, parallelOptions, (int i) =>
            {
                var md5M = MD5.Create();
                byte[] data =
                    Encoding.Unicode.GetBytes(
                        Environment.UserName + i.ToString());
                byte[] result = md5M.ComputeHash(data);
                string hexString = ConvertToHexString(result);
                // Console.WriteLine(“MD5 HASH: {0}”, hexString);
            });
            Debug.WriteLine("MD5: " + sw.Elapsed.ToString());
        }

        private static void GenerateAESKeysCancel(System.Threading.CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var sw = Stopwatch.StartNew();
            var aesM = new AesManaged();
            for (int i = 1; i <= NUM_AES_KEYS; i++)
            {
                aesM.GenerateKey();
                byte[] result = aesM.Key;
                string hexString = ConvertToHexString(result);
                // Console.WriteLine("AES KEY: {0} ", hexString);
                if (sw.Elapsed.TotalSeconds > 0.5)
                {
                    throw new TimeoutException(
                        "GenerateAESKeysCancel is taking more than 0.5 seconds to complete.");
                }
                ct.ThrowIfCancellationRequested();
            }
            Debug.WriteLine("AES: " + sw.Elapsed.ToString());
        }

        private static void GenerateMD5HashesCancel(System.Threading.CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var sw = Stopwatch.StartNew();
            var md5M = MD5.Create();
            for (int i = 1; i <= NUM_MD5_HASHES; i++)
            {
                byte[] data =
                    Encoding.Unicode.GetBytes(
                        Environment.UserName + i.ToString());
                byte[] result = md5M.ComputeHash(data);
                string hexString = ConvertToHexString(result);
                // Console.WriteLine("MD5 HASH: {0}", hexString);
                ct.ThrowIfCancellationRequested();
            }
            Debug.WriteLine("MD5: " + sw.Elapsed.ToString());
        }

        private static List<String> GenerateAESKeysWithCharPrefix(
            System.Threading.CancellationToken ct, char prefix)
        {
            var sw = Stopwatch.StartNew();
            var aesM = new AesManaged();
            var keysList = new List<String>();
            for (int i = 1; i <= NUM_AES_KEYS; i++)
            {
                aesM.GenerateKey();
                byte[] result = aesM.Key;
                string hexString = ConvertToHexString(result);
                if (hexString[0] == prefix)
                {
                    keysList.Add(hexString);
                }
                // Console.WriteLine("AES KEY: {0} ", hexString);
                if (ct.IsCancellationRequested)
                {
                    ct.ThrowIfCancellationRequested();
                }
            }
            Debug.WriteLine("AES: " + sw.Elapsed.ToString());
            return keysList;
        }

        static void Main(string[] args)
        {
            Console.ReadLine();
            Console.WriteLine("Started");
            var cts = new System.Threading.CancellationTokenSource();
            var ct = cts.Token;
            var sw = Stopwatch.StartNew();
            // The compiler will infer t1 as Task<List<String>>
            // from the delegate's body
            var t1 = Task.Factory.StartNew(
                () => GenerateAESKeysWithCharPrefix(ct, 'A'), ct);
            try
            {
                // Wait for t1 to finish
                t1.Wait();
            }
            catch (AggregateException ex)
            {
                foreach (Exception innerEx in ex.InnerExceptions)
                {
                    Debug.WriteLine(innerEx.ToString());
                    // Do something based on the innerEx Exception
                }
            }
            if (t1.IsCanceled)
            {
                Console.WriteLine(
                "The task running GenerateAESKeysWithCharPrefix was cancelled.");
            }
            if (t1.IsFaulted)
            {
                foreach (Exception innerEx in t1.Exception.InnerExceptions)
                {
                    Debug.WriteLine(innerEx.ToString());
                    // Do something based on the innerEx Exception
                }
            }
            var t2 = Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < t1.Result.Count; i++)
                {
                    // Do something with the result
                    // returned by the task’s delegate
                    Console.WriteLine(t1.Result[i]);
                }
            }, TaskCreationOptions.LongRunning);
            Debug.WriteLine(sw.Elapsed.ToString());
            // Display the results and wait for the user to press a key
            Console.WriteLine("Finished!");
            Console.ReadLine();
        }
    }
}