using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// Added for the MD5 class
using System.Security.Cryptography;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Listing6_7
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
            if ((number % 10000) == 0)
            {
                throw new InvalidOperationException(
                    String.Format(
                    "The MD5 hash generator doesn't work with input numbers divisible by {0}. Number received {1}", 
                    10000, number));
            }
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

            try
            {
                inputIntegers.ForAll((i) => hashesBag.Add(GenerateMD5Hash(i)));

                Console.WriteLine("First MD5 hash: {0}", hashesBag.First());
                Console.WriteLine("Started to show results in {0}", sw.Elapsed);
                Console.WriteLine("Last MD5 hash: {0}", hashesBag.Last());
                Console.WriteLine("{0} MD5 hashes generated in {1}", hashesBag.Count(), sw.Elapsed.ToString());
            }
            catch (AggregateException ex)
            {
                foreach (Exception innerEx in ex.InnerExceptions)
                {
                    Debug.WriteLine(innerEx.ToString());
                    // Do something considering the innerEx Exception
                    if (innerEx is InvalidOperationException)
                    {
                        Console.WriteLine(String.Format("The MD5 generator failed. Exception details: {0}", innerEx.Message));
                    }
                }
                // Something went wrong
                // Create a new empty ConcurrentBag with no results
                hashesBag = new ConcurrentBag<string>();
            }

            Console.ReadLine();
        }
    }
}
