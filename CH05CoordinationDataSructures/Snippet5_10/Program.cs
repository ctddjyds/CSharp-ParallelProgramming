using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Snippet5_10
{
    class Program
    {
        private static void CreatePlanets(int participantNum)
        {
            Console.WriteLine(
                "Creating planets. Participant # {0}",
                participantNum);
        }

        private static void CreateStars(int participantNum)
        {
            Console.WriteLine(
                "Creating stars. Participant # {0}",
                participantNum);
        }

        private static void CheckCollisionsBetweenPlanets(int participantNum)
        {
            Console.WriteLine(
                "Checking collisions between planets. Participant # {0}",
                participantNum);
        }

        private static void CheckCollisionsBetweenStars(int participantNum)
        {
            Console.WriteLine(
                "Checking collisions between stars. Participant # {0}",
                participantNum);
        }

        private static void RenderCollisions(int participantNum)
        {
            Console.WriteLine(
                "Rendering collisions. Participant # {0}",
                participantNum);
        }

        private static int _participants =
            Environment.ProcessorCount;
        private static Task[] _tasks;
        private static Barrier _barrier;

        static void Main(string[] args)
        {
            _tasks = new Task[_participants];
            _barrier = new Barrier(_participants, (barrier) =>
            {
                Console.WriteLine("Current phase: {0}",
                    barrier.CurrentPhaseNumber);
            });

            // You pass false for enableThreadOwnerTracking
            // because you want the best performance out of SpinLock
            var sl = new SpinLock(false);
            var sb = new StringBuilder();

            for (int i = 0; i < _participants; i++)
            {
                _tasks[i] = Task.Factory.StartNew((num) =>
                {
                    var participantNumber = (int)num;
                    for (int j = 0; j < 10; j++)
                    {
                        CreatePlanets(participantNumber);
                        _barrier.SignalAndWait();
                        CreateStars(participantNumber);
                        _barrier.SignalAndWait();
                        CheckCollisionsBetweenPlanets(participantNumber);
                        _barrier.SignalAndWait();
                        CheckCollisionsBetweenStars(participantNumber);
                        _barrier.SignalAndWait();
                        RenderCollisions(participantNumber);
                        _barrier.SignalAndWait();

                        var logLine =
                            String.Format(
                "Time: {0}, Phase: {1}, Participant: {2}, Phase completed OK\n",
                                DateTime.Now.TimeOfDay,
                                _barrier.CurrentPhaseNumber,
                                participantNumber);

                        bool lockTaken = false;
                        try
                        {
                            sl.Enter(ref lockTaken);
                            // SpinLock acquired a lock on sb
                            // Critical section
                            sb.Append(logLine);
                            // End of critical section
                        }
                        finally
                        {
                            // You need to make sure that 
                            // you release the lock on sb
                            if (lockTaken)
                            {
                                // Gives up the lock if it actually acquired it
                                // SpinLock doesn't
                                // You want performance at the expense of fairness
                                // Therefore, you pass false for useMemoryBarrier
                                sl.Exit(false);
                            }
                        }
                    }
                }, i);
            }

            var finalTask = Task.Factory.ContinueWhenAll(_tasks, (tasks) =>
            {
                // Wait for all the tasks to ensure 
                // the propagation of any exception occurred 
                // in any of the _tasks
                Task.WaitAll(_tasks);
                Console.WriteLine(
                    "All the phases were executed.");
                Console.WriteLine(sb);
                // Dispose the Barrier instance
                _barrier.Dispose();
            });

            // Wait for finalTask to finish
            finalTask.Wait();

            Console.ReadLine();
        }
    }
}
