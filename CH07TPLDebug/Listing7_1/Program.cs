using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// Added
using System.Threading;
using System.Threading.Tasks;

namespace DeadlockTest
{
    class Program
    {
        private static Object _sharedVariable1 = new Object();
        private static Object _sharedVariable2 = new Object();

        private static int _tasksCounter1;
        private static int _tasksCounter2;


        private static void CountTasks1()
        {
            lock (_sharedVariable1)
            {
                _tasksCounter1++;
                Thread.Sleep(5000);
                lock (_sharedVariable2)
                {
                    _tasksCounter2++;
                }
            }
        }

        private static void CountTasks2()
        {
            lock (_sharedVariable2)
            {
                _tasksCounter2++;
                Thread.Sleep(5000);
                lock (_sharedVariable1)
                {
                    _tasksCounter1++;
                }
            }
        }

        static void Main(string[] args)
        {

            _tasksCounter1 = 0;
            _tasksCounter2 = 0;

            var t1 = new Task(() => CountTasks1());
            var t2 = new Task(() => CountTasks2());

            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);
        }
    }
}
