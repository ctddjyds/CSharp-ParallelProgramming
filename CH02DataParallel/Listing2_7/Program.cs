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
using System.Collections.Concurrent;

namespace Listing2_7
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
            Parallel.ForEach(Partitioner.Create(1, NUM_AES_KEYS + 1), range =>
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
            Parallel.ForEach(Partitioner.Create(1, NUM_MD5_HASHES + 1),
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

        static void Main(string[] args)
        {
            var sw = Stopwatch.StartNew();

            ParallelPartitionGenerateAESKeys();
            ParallelPartitionGenerateMD5Hashes();

            Debug.WriteLine(sw.Elapsed.ToString());

            // Display the results and wait for the user to press a key
            Console.WriteLine("Finished!");
            Console.ReadLine();
        }
    }
}
