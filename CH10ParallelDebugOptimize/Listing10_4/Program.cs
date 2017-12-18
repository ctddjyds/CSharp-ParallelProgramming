using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
// This code has performance problems
// Don’t consider this code as a best practice
namespace Listing10_4
{
    class Program
    {
        // 5000 ints
        private static int NUM_INTS = 5000;

        private static ParallelQuery<int> GenerateInputData()
        {
            return ParallelEnumerable.Range(1, NUM_INTS);
        }

        private static Mutex _mut = new Mutex();

        private static double CalculateX(int intNum)
        {
            double result = 0;
            // Wait until it is safe to enter
            // the critical section protected by a Mutex
            _mut.WaitOne();
            try
            {
                for (int i = 0; i < 50000; i++)
                {
                    double newResult =
                    Math.Pow(Math.Sqrt(intNum / Math.PI), 3) * i;
                    if (newResult > result)
                    {
                        result = newResult;
                    }
                }
            }
            finally
            {
                // Release the Mutex
                _mut.ReleaseMutex();
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
            // Console.WriteLine("Elapsed time {0}",
            //    sw.Elapsed.TotalSeconds.ToString());

            // Console.ReadLine();
        }
    }
}