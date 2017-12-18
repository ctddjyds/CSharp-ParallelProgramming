using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;

namespace Listing6_3
{
    class Program
    {
        static int NUM_INTS = 50000000;

        static ParallelQuery<int> GenerateInputData()
        {
            return ParallelEnumerable.Range(1, NUM_INTS);
        }

        static void Main(string[] args)
        {
            var inputIntegers = GenerateInputData();

            var parReductionQuery = (from intNum in inputIntegers.AsParallel()
                                     where ((intNum % 5) == 0)
                                     select (intNum / Math.PI)).Average();

            Console.WriteLine("Average {0}", parReductionQuery);
            Console.ReadLine();
        }
    }
}