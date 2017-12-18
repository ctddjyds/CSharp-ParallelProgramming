using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
// This code has performance problems
// Don’t consider this code as a best practice
namespace Listing10_5
{
    class Program
    {
        // 100,000,000 ints
        private static int NUM_INTS = 100000000;

        private static IEnumerable<int> GenerateInputData()
        {
            return Enumerable.Range(1, NUM_INTS);
        }

        private static double CalculateX(int intNum)
        {
            return Math.Pow(Math.Sqrt(intNum / Math.PI), 3);
        }

        static void Main(string[] args)
        {
            var sw = Stopwatch.StartNew();
            var inputIntegers = GenerateInputData();
            var numTasks = Environment.ProcessorCount * 3;
            var tasks = new List<Task>(numTasks);
            for (int i = 0; i < numTasks; i++)
            {
                var t = Task.Factory.StartNew(
                () =>
                {
                    var seqReductionQuery =
                    (from intNum
                    in inputIntegers
                     where ((intNum % 5) == 0) ||
                     ((intNum % 7) == 0) ||
                     ((intNum % 9) == 0)
                     select (CalculateX(intNum)))
                    .Average();
                    Console.WriteLine("Average {0}",
                    seqReductionQuery);
                });
                tasks.Add(t);
            }
            Task.WaitAll(tasks.ToArray());
            // Comment the next two lines
            // while profiling
            // Console.WriteLine("Elapsed time {0}",
            // sw.Elapsed.TotalSeconds.ToString());
            // Console.ReadLine();
        }
    }
}