using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// Added
using System.Threading.Tasks;
using System.Diagnostics;

namespace Listing10_10
{
    class NumberGenerator
    {
        private int _lastNumber = 0;

        public int GenerateNext()
        {
            _lastNumber++;
            return _lastNumber;
        }
    }

    class Program
    {
        private static int NUM_INTS = 50000000;

        private static void GenerateNumbersSharedLine()
        {
            NumberGenerator ng1 = new NumberGenerator();
            NumberGenerator ng2 = new NumberGenerator();
            int[] numbers1 = new int[NUM_INTS];
            int[] numbers2 = new int[NUM_INTS];

            // Generate numbers in parallel
            // with a high probability
            // of sharing the cache line
            Parallel.Invoke(
                () =>
                {
                    for (int i = 0; i < NUM_INTS; i++)
                    {
                        numbers1[i] = ng1.GenerateNext();
                    }
                },
                () =>
                {
                    for (int i = 0; i < NUM_INTS; i++)
                    {
                        numbers2[i] = ng2.GenerateNext();
                    }
                });

            Console.WriteLine("numbers1: {0}",
                numbers1.Max());
            Console.WriteLine("numbers2: {0}",
                numbers2.Max());
        }

        static void Main(string[] args)
        {
            var sw = Stopwatch.StartNew();
            GenerateNumbersSharedLine();
            Console.WriteLine(sw.Elapsed.TotalSeconds);
            Console.ReadLine();
        }
    }
}
