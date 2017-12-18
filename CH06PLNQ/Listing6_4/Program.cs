using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// Added for the MD5 class
using System.Security.Cryptography;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Listing6_4
{
    class Program
    {
        static void Main(string[] args)
        {
            int[] inputIntegers = { 0, 3, 4, 8, 15, 22, 34, 57, 68, 32, 21, 30 };
            
            // Comment the previous line and uncomment this one to run the LINQ queries with more data
            // var inputIntegers = ParallelEnumerable.Range(1, 100000000);

            var mean = inputIntegers.AsParallel().Average();

            var standardDeviation = inputIntegers.AsParallel().Aggregate(
             // Seed
             0d,
             // Update accumulator function
             (subTotal, thisNumber) => subTotal + 
                 Math.Pow((thisNumber - mean), 2),
             // Combine accumulators function
             (total, thisTask) => total + thisTask, 
             // Result selector
             (finalSum) => Math.Sqrt((finalSum / 
                 (inputIntegers.Count() - 1))));

            var skewness = inputIntegers.AsParallel().Aggregate(
             // Seed
             0d,
             // Update accumulator function
             (subTotal, thisNumber) => subTotal + 
                 Math.Pow(((thisNumber - mean) / standardDeviation), 3),
             // Combine accumulators function
             (total, thisTask) => total + thisTask,
             // Result selector
             (finalSum) => 
                 ((finalSum * inputIntegers.Count()) / 
                  ((inputIntegers.Count() - 1) * 
                   (inputIntegers.Count() - 2))));

            var kurtosis = inputIntegers.AsParallel().Aggregate(
                // Seed
                0d,
                // Update accumulator function
                (subTotal, thisNumber) => subTotal + 
                    Math.Pow(((thisNumber - mean) / 
                     standardDeviation), 4),
                // Combine accumulators function
                (total, thisTask) => total + thisTask,
                // Result selector
                (finalSum) =>
                    ((finalSum * inputIntegers.Count() * 
                      (inputIntegers.Count() + 1)) /
                     ((inputIntegers.Count() - 1) * 
                      (inputIntegers.Count() - 2) * 
                      (inputIntegers.Count() - 3))) - 
                     ((3 * 
                       Math.Pow((inputIntegers.Count() - 1), 2)) / 
                      ((inputIntegers.Count() - 2) * 
                       (inputIntegers.Count() - 3))));

            Console.WriteLine("Mean: {0}", mean);
            Console.WriteLine("Standard deviation: {0}", standardDeviation);
            Console.WriteLine("Skewness: {0}", skewness);
            Console.WriteLine("Kurtosis: {0}", kurtosis);

            Console.ReadLine();
        }
    }
}
