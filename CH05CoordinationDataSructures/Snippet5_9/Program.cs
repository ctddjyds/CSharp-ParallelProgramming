using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Snippet5_9
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
        private static Barrier _barrier;

        // Each participant must return its log string
        private static Task<string>[] _tasks;

        static void Main(string[] args)
        {
            _tasks = new Task<string>[_participants]; 
            _barrier = new Barrier(_participants, (barrier) =>
            {
                Console.WriteLine("Current phase: {0}",
                    barrier.CurrentPhaseNumber);
            });

            for (int i = 0; i < _participants; i++)
            {
                _tasks[i] = Task<string>.Factory.StartNew((num) =>
                {
                    var localsb = new StringBuilder();
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

                        localsb.AppendFormat(
"Time: {0}, Phase: {1}, Participant: {2}, Phase completed OK\n",
                            DateTime.Now.TimeOfDay,
                            _barrier.CurrentPhaseNumber,
                            participantNumber);
                    }
                    return localsb.ToString();
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

                // Collect the results
                var finalsb = new StringBuilder();
                for (int t = 0; t < _participants; t++)
                {
                    if ((!_tasks[t].IsFaulted) && (!_tasks[t].IsCanceled))
                    {
                        finalsb.Append(_tasks[t].Result);
                    }
                }
                // Display the final string
                Console.WriteLine(finalsb);

                // Dispose the Barrier instance
                _barrier.Dispose();
            });

            // Wait for finalTask to finish
            finalTask.Wait();

            Console.ReadLine();
        }
    }
}