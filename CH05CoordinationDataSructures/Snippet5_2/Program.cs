using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Snippet5_2
{
    class Program
    {
        private static void CreatePlanets(int participantNum)
        {
            Console.WriteLine(
                "Creating planets. Participant # {0}",
                participantNum);
            if (participantNum == 0)
            {
                // Spin until there aren't remaining participants
                // for the current phase
                // This condition running in a loop (spinning) is very inefficient
                // This example uses this spinning for educational purposes
                // It isn’t a best practice
                SpinWait.SpinUntil(() => (_barrier.ParticipantsRemaining == 0));
            }
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

        private static void CheckCollissionsBetweenStars(int participantNum)
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
                if (barrier.CurrentPhaseNumber == 10)
                {
                    throw new InvalidOperationException(
                        "No more phases allowed.");
                }
            });

            for (int i = 0; i < _participants; i++)
            {
                _tasks[i] = Task.Factory.StartNew((num) =>
                {
                    var participantNumber = (int)num;
                    for (int j = 0; j < 10; j++)
                    {
                        CreatePlanets(participantNumber);
                        try
                        {
                            _barrier.SignalAndWait();
                        }
                        catch (BarrierPostPhaseException bppex)
                        {
                            Console.WriteLine(
                                "{0}", bppex.InnerException.Message);
                            // Exit the for loop
                            break;
                        }
                        CreateStars(participantNumber);
                        _barrier.SignalAndWait();
                        CheckCollisionsBetweenPlanets(participantNumber);
                        _barrier.SignalAndWait();
                        CheckCollissionsBetweenStars(participantNumber);
                        _barrier.SignalAndWait();
                        RenderCollisions(participantNumber);
                        _barrier.SignalAndWait();
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
                // Dispose the Barrier instance
                _barrier.Dispose();
            });

            // Wait for finalTask to finish
            finalTask.Wait();

            Console.ReadLine();
        }
    }
}