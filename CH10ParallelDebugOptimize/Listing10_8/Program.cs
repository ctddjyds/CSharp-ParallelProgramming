using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
// This code has performance problems
// Don’t consider this code as a best practice
namespace Listing10_8
{
    class Program
    {
        // 100,000,000 ints
        private static int NUM_INTS = 100000000;

        private static string ConvertToHexString(
            char[] charArray)
        {
            // Convert the byte array to hexadecimal string
            var sb = new StringBuilder(charArray.Length);
            
            for (int i = 0; i < charArray.Length; i++)
            {
                sb.Append(((int)charArray[i]).ToString("X2"));
            }
            // This is horrible code that
            // creates too many temporary strings
            var tempString = sb.ToString();
            var tempString2 = tempString.ToLower();
            var tempString3 =
                tempString2.Substring(
                    0, tempString2.Length - 1);
            return tempString3.ToUpper();
        }

        private static ParallelQuery<int> GenerateInputData()
        {
            return ParallelEnumerable.Range(1, NUM_INTS);
        }

        private static string CreateString(int intNum)
        {
            return ConvertToHexString(
                (Math.Pow(Math.Sqrt(intNum / Math.PI), 3)).
                ToString().ToCharArray());
        }

        static void Main(string[] args)
        {
            var sw = Stopwatch.StartNew();
            
            var inputIntegers = GenerateInputData();
         
            var parReductionQuery =
                (from intNum
                 in inputIntegers
                 .AsParallel()
                 where ((intNum % 5) == 0) ||
                       ((intNum % 7) == 0) ||
                       ((intNum % 9) == 0)
                 select (CreateString(intNum))).Max();
            
            Console.WriteLine("Max: {0}", parReductionQuery);
            
            // Comment the next two lines
            // while profiling
            // Console.WriteLine("Elapsed time {0}",
            //     sw.Elapsed.TotalSeconds.ToString());
            // Console.ReadLine();
        }
    }
}