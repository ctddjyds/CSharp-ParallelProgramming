using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// Added for the MD5 class
using System.Security.Cryptography;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Listing6_5
{
    class Program
    {
        private static ParallelQuery<int> inputIntegers = 
            ParallelEnumerable.Range(1, 100000000);

        private static double CalculateMean(
            System.Threading.CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return inputIntegers.AsParallel().WithCancellation(ct).Average();
        }

        private static double CalculateStandardDeviation(
            System.Threading.CancellationToken ct, double mean)
        {
            ct.ThrowIfCancellationRequested();
            return inputIntegers.AsParallel().WithCancellation(ct).Aggregate(
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
        }

        private static double CalculateSkewness(
            System.Threading.CancellationToken ct, 
            double mean, double standardDeviation)
        {
            ct.ThrowIfCancellationRequested();
            return inputIntegers.AsParallel().WithCancellation(ct).Aggregate(
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
        }

        private static double CalculateKurtosis(
            System.Threading.CancellationToken ct, 
            double mean, double standardDeviation)
        {
            ct.ThrowIfCancellationRequested();
            return inputIntegers.AsParallel().WithCancellation(ct).Aggregate(
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
        }

        static void Main(string[] args)
        {
            Console.ReadLine();

            Console.WriteLine("Started");

            var cts = new System.Threading.CancellationTokenSource();
            var ct = cts.Token;

            var sw = Stopwatch.StartNew();

            var taskMean = new Task<double>(() => CalculateMean(ct), ct);

            var taskSTDev = taskMean.ContinueWith<double>((t) =>
                {
                    // At this point
                    // t.Result = mean
                    return CalculateStandardDeviation(ct, t.Result);
                }, TaskContinuationOptions.OnlyOnRanToCompletion);

            var taskSkewness = taskSTDev.ContinueWith<double>((t) =>
                {
                    // At this point
                    // t.Result = standard deviation
                    return CalculateSkewness(ct, taskMean.Result, t.Result);
                }, TaskContinuationOptions.OnlyOnRanToCompletion);

            var taskKurtosis = taskSTDev.ContinueWith<double>((t) =>
                {
                    // At this point
                    // t.Result = standard deviation
                    return CalculateKurtosis(ct, taskMean.Result, t.Result);
                }, TaskContinuationOptions.OnlyOnRanToCompletion);

            var deferredCancelTask = Task.Factory.StartNew(() =>
            {
                // Sleep the thread that runs this task for 5 seconds
                System.Threading.Thread.Sleep(5000);
                // Send the signal to cancel
                cts.Cancel();
            });

            try
            {
                // Start taskMean and then all the chained tasks
                taskMean.Start();

                // Wait for both taskSkewness and taskKurtosis to finish
                Task.WaitAll(taskSkewness, taskKurtosis);
                
                Console.WriteLine("Mean: {0}", taskMean.Result);
                Console.WriteLine("Standard deviation: {0}", taskSTDev.Result);
                Console.WriteLine("Skewness: {0}", taskSkewness.Result);
                Console.WriteLine("Kurtosis: {0}", taskKurtosis.Result);

                Console.ReadLine();
            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("The query was cancelled.");
                Console.ReadLine();
            }
            catch (AggregateException ex)
            {
                foreach (Exception innerEx in ex.InnerExceptions)
                {
                    Debug.WriteLine(innerEx.ToString());
                    // Do something considering the innerEx Exception
                    if (ex.InnerException is OperationCanceledException)
                    {
                        // A task was cancelled
                        // Write each task status to the console
                        Console.WriteLine("Mean task: {0}", taskMean.Status);
                        Console.WriteLine("Standard deviation task: {0}", taskSTDev.Status);
                        Console.WriteLine("Skewness task: {0}", taskSkewness.Status);
                        Console.WriteLine("Kurtosis task: {0}", taskKurtosis.Status);
                        Console.ReadLine();
                    }
                }
            }
        }
    }
}
