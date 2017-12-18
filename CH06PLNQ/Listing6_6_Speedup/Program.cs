using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// Added for the MD5 class
using System.Security.Cryptography;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Listing6_6_Speedup
{
    class Program
    {
        private const int NUM_MD5_HASHES = 100000;

        private static ParallelQuery<int> GenerateMD5InputData()
        {
            return ParallelEnumerable.Range(1, NUM_MD5_HASHES);
        }

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

        private static string GenerateMD5Hash(int number)
        {
            var md5M = MD5.Create();
            byte[] data = Encoding.Unicode.GetBytes(Environment.UserName + number.ToString());
            byte[] result = md5M.ComputeHash(data);
            string hexString = ConvertToHexString(result);

            return hexString;
        }

        static void Main(string[] args)
        {
            Console.ReadLine();
            Console.WriteLine("Started");

            var sw = Stopwatch.StartNew();

            var inputIntegers = GenerateMD5InputData();
            var hashesBag = new ConcurrentBag<string>();
            inputIntegers.WithDegreeOfParallelism(4).ForAll((i) =>
                hashesBag.Add(GenerateMD5Hash(i)));

            Console.WriteLine("Finished in {0}", sw.Elapsed);
            Console.WriteLine("{0} MD5 hashes generated in {1}", hashesBag.Count(), sw.Elapsed.ToString());
            Console.WriteLine("First MD5 hash: {0}", hashesBag.First());
            Console.WriteLine("Last MD5 hash: {0}", hashesBag.Last());
            Console.ReadLine();
        }
    }
}
