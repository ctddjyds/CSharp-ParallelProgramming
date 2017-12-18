using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// Added
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Listing5_5
{
    class Program
    {
        private static CountdownEvent _countdown;

        private static int MIN_PATHS = 
            Environment.ProcessorCount;
        private static int MAX_PATHS =
            Environment.ProcessorCount * 3;
        
        private static void SimulatePaths(int pathCount)
        {
            for (int i =0; i < pathCount; i++)
            {
                Task.Factory.StartNew((num) =>
                {
                    try
                    {
                        var pathNumber = (int)num;
                        // Simulate workload
                        var sw = Stopwatch.StartNew();
                        // Generate a Random number of milliseconds to wait for
                        var rnd = new Random();
                        var waitfor = rnd.Next(2000, 5000);
                        SpinWait.SpinUntil(
                            () => (sw.ElapsedMilliseconds >= waitfor));
                        Console.WriteLine(
                            "Path {0} simulated.",
                            pathNumber);
                    }
                    finally
                    {
                        // Signal the CountdownEvent
                        // to decrement the count
                        _countdown.Signal();
                    }
                }, i);
            }
        }

        static void Main(string[] args)
        {
            _countdown = new CountdownEvent(MIN_PATHS);

            var t1 = Task.Factory.StartNew(() =>
                {
                    for (int i = MIN_PATHS; i <= MAX_PATHS; i++)
                    {
                        Console.WriteLine(
                            ">>>> {0} Concurrent paths start.",
                            i);
                        // Reset the count to i
                        _countdown.Reset(i);
                        SimulatePaths(i);
                        // Join
                        _countdown.Wait();
                        Console.WriteLine(
                            "<<<< {0} Concurrent paths end.",
                            i);
                    }
                });

            try
            {
                t1.Wait();
                Console.WriteLine(
                    "The simulation was executed.");
            }
            finally
            {
                _countdown.Dispose();
            }

            Console.ReadLine();
        }
    }
}
