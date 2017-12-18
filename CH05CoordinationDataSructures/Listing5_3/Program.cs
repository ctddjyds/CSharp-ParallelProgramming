using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// Added
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Listing5_3
{
    class Program
    {
        private static SemaphoreSlim _semaphore;

        private static Task[] _tasks;

        private const int MAX_MACHINES = 3;
        private static int _attackers =
            Environment.ProcessorCount;

        private static void SimulateAttacks(int attackerNumber)
        {
            // Simulate workload
            var sw = Stopwatch.StartNew();
            // Generate a Random number of milliseconds to wait for
            var rnd = new Random();
            var waitfor = rnd.Next(2000, 5000);
            SpinWait.SpinUntil(
                () => (sw.ElapsedMilliseconds >= waitfor));
            // At this point, the method has the necessary data
            // to simulate the attack
            // Needs to enter the semaphore
            // Wait for _semaphore to grant access

            Console.WriteLine(
                "WAIT #### Attacker {0} requested to enter the semaphore.",
                attackerNumber);
            _semaphore.Wait();

            // This code runs when it entered the semaphore
            try
            {
                Console.WriteLine(
                    "ENTER ---> Attacker {0} entered the semaphore.",
                    attackerNumber);

                // Simulate the attack
                // Simulate workload
                sw.Restart();
                waitfor = rnd.Next(2000, 5000);
                SpinWait.SpinUntil(
                    () => (sw.ElapsedMilliseconds >= waitfor));
            }
            finally
            {
                // Exit the semaphore
                _semaphore.Release();
                Console.WriteLine("RELEASE <--- Attacker {0} released the semaphore.",
                    attackerNumber);
            }
        }

        static void Main(string[] args)
        {
            _tasks = new Task[_attackers];            
            // Create the SemaphoreSlim instance
            // with its initial count set to MAX_MACHINES
            // Its maximum concurrent accesses
            // is also going to be set to MAX_MACHINES
            _semaphore = new SemaphoreSlim(MAX_MACHINES);
            Console.WriteLine("{0} attackers are going to be able to enter the semaphore.",
                _semaphore.CurrentCount);

            for (int i = 0; i < _attackers; i++)
            {
                _tasks[i] = new Task((num) =>
                {
                    for (int j = 0; j < 10; j++)
                    {
                        var attackerNumber = (int)num;
                        SimulateAttacks(attackerNumber);
                    }
                }, i);
                _tasks[i].Start();
            }

            var finalTask = Task.Factory.ContinueWhenAll(_tasks, (tasks) =>
            {
                // Wait for all the tasks to ensure 
                // the propagation of any exception occurred 
                // in any of the _tasks
                Task.WaitAll(_tasks);
                Console.WriteLine(
                    "The simulation was executed.");
                // Dispose the SemaphoreSlim instance
                _semaphore.Dispose();
            });

            // Wait for finalTask to finish
            finalTask.Wait();
            
            Console.ReadLine();
        }
    }
}
