using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
// Added
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Listing8_5
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // The total number of workers
        private int _workerCount = 0;

        // The number of active workers
        private int _activeWorkers = 0;

        // A CountdownEvent
        // with its initial count set to 1
        private CountdownEvent _countdown = new CountdownEvent(1);

        private void DoSomeWorkAtZone(
            int workerNum,
            string zone)
        {
            // Simulate some work
            // between 1 and 3 seconds
            var milliSecs =
                new Random(
                    Thread.CurrentThread.ManagedThreadId).Next(1000, 3000);
            Debug.WriteLine(
                "Worker #{0} works at the {1} {2} seconds",
                workerNum,
                zone,
                milliSecs);
            var sw = Stopwatch.StartNew();
            // Simulate some CPU-bound work
            while (sw.Elapsed.TotalMilliseconds < milliSecs)
            {
                // Go on simulating CPU-bound work
                double calc = Math.Pow(
                Math.PI *
                sw.Elapsed.TotalMilliseconds, 2);
            }
        }
        
        private void DoSomeWork(int workerNum)
        {
            var taskLeft =
            new Task(() =>
            {
                DoSomeWorkAtZone(
                workerNum,
                "left");
            });
            var taskCenter =
            new Task(() =>
            {
                DoSomeWorkAtZone(
                workerNum,
                "center");
            });
            var taskRight =
            new Task(() =>
            {
                DoSomeWorkAtZone(
                workerNum,
                "right");
            });
            taskLeft.Start();
            taskCenter.Start();
            taskRight.Start();
            Task.WaitAll(
                new[] { taskLeft,
                taskCenter,
                taskRight });
        }
        
        private void butLaunchWorker_Click(
            object sender, RoutedEventArgs e)
        {
            _workerCount++;

            // Create and start a new Task
            // and send _workerCount as a
            // parameter to the action delegate
            var workerTask = Task.Factory.StartNew((num) =>
            {
                // Retrieve the workerNum received
                // as a parameter in object state
                int workerNum = (int)num;
                // Increment the number of active workers
                // with an atomic operation
                Interlocked.Increment(
                    ref _activeWorkers);
                // Increment the current count
                // for the shared CountdownEvent
                _countdown.AddCount(1);
                try
                {
                    Debug.WriteLine(
                        "Worker {0} started ", workerNum);

                    DoSomeWork(workerNum);

                    Debug.WriteLine(
                        "Worker {0} finished ", workerNum);

                    // Register one signal
                    // for the shared CountdownEvent
                    // and decreases the remaining signals
                    // required to unblock the thread
                    // that called the Wait method
                    _countdown.Signal();
                }
                finally
                {
                    // Increment the number of active workers
                    // with an atomic operation
                    Interlocked.Decrement(
                        ref _activeWorkers);
                }
            }, _workerCount);
        }

        private void butCheckActive_Click(
            object sender, RoutedEventArgs e)
        {
            lblActiveWorkers.Content =
                String.Format(
                    "Active workers: {0}",
                    _activeWorkers);
        }

        private void butWaitAll_Click(object sender, RoutedEventArgs e)
        {
            // Signal once to equal the number of workers
            _countdown.Signal();
            // Block the UI thread until
            // the signal count for the
            // CountdownEvent reaches 0
            // It is obviously a very bad practice
            // to block the UI thread like this
            // This is done just for example purposes
            _countdown.Wait();
            // Reset the number of remaining signals
            // to 1
            butWaitAll.Content = "Done!";
        }
    }
}
