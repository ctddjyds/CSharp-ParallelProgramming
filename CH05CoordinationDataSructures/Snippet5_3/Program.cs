using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Snippet5_3
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
                SpinWait.SpinUntil(() => (_barrier.ParticipantsRemaining == 0), TIMEOUT * 3);
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


        // 2,000 milliseconds = 2 seconds timeout
        private const int TIMEOUT = 2000;

        static void Main(string[] args)
        {
            var cts = 
                new System.Threading.CancellationTokenSource();
            var ct = cts.Token;

            _tasks = new Task[_participants];
            _barrier = new Barrier(_participants, (barrier) =>
            {
                Console.WriteLine("Current phase: {0}", 
                    barrier.CurrentPhaseNumber);
            });

            for (int i = 0; i < _participants; i++)
            {
                _tasks[i] = Task.Factory.StartNew((num) =>
                {
                    var participantNumber = (int)num;
                    for (int j = 0; j < 10; j++)
                    {
                        CreatePlanets(participantNumber);
                        if (!_barrier.SignalAndWait(TIMEOUT))
                        {
                            Console.WriteLine(
        "Participants are requiring more than {0} seconds to reach the barrier.", 
                                TIMEOUT);
                            throw new OperationCanceledException(
                                string.Format(
        "Participants are requiring more than {0} seconds to reach the barrier at the Phase # {1}.",
                                TIMEOUT,
                                _barrier.CurrentPhaseNumber), ct);
                        }
                        CreateStars(participantNumber);
                        _barrier.SignalAndWait();
                        CheckCollisionsBetweenPlanets(participantNumber);
                        _barrier.SignalAndWait();
                        CheckCollisionsBetweenStars(participantNumber);
                        _barrier.SignalAndWait();
                        RenderCollisions(participantNumber);
                        _barrier.SignalAndWait();
                    }
                }, i, ct);
            }

            var finalTask = Task.Factory.ContinueWhenAll(_tasks, (tasks) =>
            {
                // Wait for all the tasks to ensure 
                // the propagation of any exception occurred 
                // in any of the _tasks
                Task.WaitAll(_tasks);
                Console.WriteLine(
                    "All the phases were executed.");
            }, ct);

            // Wait for finalTask to finish
            try
            {
                //// Use a timeout
                if (!finalTask.Wait(TIMEOUT * 2))
                {
                    bool faulted = false;
                    for (int t = 0; t < _participants; t++)
                    {
                        if (_tasks[t].Status != TaskStatus.RanToCompletion)
                        {
                            faulted = true;
                            if (_tasks[t].Status == TaskStatus.Faulted)
                            {
                                foreach (var innerEx 
                                         in _tasks[t].Exception.InnerExceptions)
                                    Console.WriteLine(innerEx.Message);
                            }
                        }
                    }
                    if (faulted)
                    {
                        Console.WriteLine(
                            "The phases failed their execution.");
                    }
                    else
                    {
                        Console.WriteLine(
                            "All the phases were executed.");
                    }
                }
            }
            catch (AggregateException ex)
            {
                foreach (var innerEx in ex.InnerExceptions)
                    Console.WriteLine(innerEx.Message);
                Console.WriteLine(
                    "The phases failed their execution.");
            }
            finally
            {
                // Dispose the Barrier instance
                _barrier.Dispose();
            }

            Console.ReadLine();
        }
    }
}