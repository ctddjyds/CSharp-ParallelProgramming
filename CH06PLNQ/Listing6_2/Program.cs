using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;

namespace Listing6_2
{
    class Program
    {
        static int NUM_INTS = 50000000;

        static IEnumerable<int> GenerateInputData()
        {
            return Enumerable.Range(1, NUM_INTS);
        }
        
        static void Main(string[] args)
        {
            var inputIntegers = GenerateInputData();

            var seqReductionQuery = (from intNum in inputIntegers
                                     where ((intNum % 5) == 0)
                                     select (intNum / Math.PI)).Average();

            Console.WriteLine("Average {0}", seqReductionQuery);
            Console.ReadLine();
        }
    }
}