using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
// This code has performance problems
// Don’t consider this code as a best practice
namespace Listing10_1
{
    class Program
    {
        // 500,000,000 ints
        private static int NUM_INTS = 500000000;
        
        private static ParallelQuery<int> GenerateInputData()
        {
            return ParallelEnumerable.Range(1, NUM_INTS);
        }
        
        private static object _syncCalc = new Object();
        
        private static double CalculateX(int intNum)
        {
            // This code has an unnecessary lock
            double result;
            lock (_syncCalc)
            {
                result =
                Math.Pow(Math.Sqrt(intNum / Math.PI), 3);
            }
            return result;
        }
        
        static void Main(string[] args)
        {
            var sw = Stopwatch.StartNew();
            
            var inputIntegers = GenerateInputData();
            
            var parReductionQuery =
                (from intNum in inputIntegers.AsParallel()
                 where ((intNum % 5) == 0) ||
                       ((intNum % 7) == 0) ||
                       ((intNum % 9) == 0)
                 select (CalculateX(intNum))).Average();
           
            Console.WriteLine("Average {0}", parReductionQuery);
            
            // Comment the next two lines
            // while profiling
            Console.WriteLine("Elapsed time {0}",
                sw.Elapsed.TotalSeconds.ToString());
            
            Console.ReadLine();
        }
    }
}