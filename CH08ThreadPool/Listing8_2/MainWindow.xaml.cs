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
using System.Collections.Concurrent;

namespace Listing8_2
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

        private ConcurrentQueue<ManualResetEvent> _handlesQueue =
            new ConcurrentQueue<ManualResetEvent>();

        private void DoSomeWork(int workerNum)
        {
            // Simulate some work
            // between 7 and 12 seconds
            var milliSecs =
                new Random().Next(7000, 12000);
            Debug.WriteLine(
                "Worker #{0} goes to sleep {1} milliseconds",
                workerNum,
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

        private void butLaunchWorker_Click(
            object sender, RoutedEventArgs e)
        {
            // Increment the worker count
            // There is no need for an atomic operation here
            // because this code runs always on the UI thread
            _workerCount++;
            // Request ThreadPool to queue
            // a work item (a WaitCallback)
            // and send _workerCount as a
            // parameter to the WaitCallback
            ThreadPool.QueueUserWorkItem(
                (state) =>
                {
                    int workerNum = (int)state;
                    Interlocked.Increment(
                        ref _activeWorkers);
                    // Create a new unsignaled ManualResetEvent
                    // and add it to
                    // the _handlesQueue ConcurrentQueue
                    var handle = new ManualResetEvent(false);
                    _handlesQueue.Enqueue(handle);
                    try
                    {
                        Debug.WriteLine(
                            "Worker #{0} started ", workerNum);

                        DoSomeWork(workerNum);

                        Debug.WriteLine(
                            "Worker #{0} finished ", workerNum);

                        // Signal (set) the ManualResetEvent
                        handle.Set();
                    }
                    finally
                    {
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
            // Create and start a new task that waits
            // for all the events to be set
            var t1 = Task.Factory.StartNew(
                () =>
                {
                    if (_handlesQueue.Count > 0)
                    {
                        // Wait for all the events to be set
                        WaitHandle.WaitAll(
                        _handlesQueue.ToArray());
                    }
                });

                // Block this thread - the UI thread - until
                // the t1 task finishes
                // It is obviously a very bad practice
                // to block the UI thread like this
                // This is done just for example purposes
                t1.Wait();
                // Update the Button’s title
                butWaitAll.Content = "Done!";
        }
    }
}
