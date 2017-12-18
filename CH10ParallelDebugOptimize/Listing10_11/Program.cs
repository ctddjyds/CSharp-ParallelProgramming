using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// Added
using System.Threading.Tasks;
using System.Diagnostics;

namespace Listing10_11
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

        private static void GenerateNumbersExcLine()
        {
            int[] numbers1;
            int[] numbers2;

            Parallel.Invoke(
                () =>
                {
                    NumberGenerator ng1 = new NumberGenerator();
                    numbers1 = new int[NUM_INTS];
                    for (int i = 0; i < NUM_INTS; i++)
                    {
                        numbers1[i] = ng1.GenerateNext();
                    }
                    Console.WriteLine("numbers1: {0}",
                        numbers1.Max());
                },
                () =>
                {
                    NumberGenerator ng2 = new NumberGenerator();
                    numbers2 = new int[NUM_INTS];
                    for (int i = 0; i < NUM_INTS; i++)
                    {
                        numbers2[i] = ng2.GenerateNext();
                    }
                    Console.WriteLine("numbers2: {0}",
                        numbers2.Max());
                });
        }

        static void Main(string[] args)
        {
            var sw = Stopwatch.StartNew();
            GenerateNumbersExcLine();
            Console.WriteLine(sw.Elapsed.TotalSeconds);
            Console.ReadLine();
        }
    }
}