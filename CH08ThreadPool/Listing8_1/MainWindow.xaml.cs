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

namespace Listing8_1
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
                    // Retrieve the workerNum received
                    // as a parameter in object state
                    int workerNum = (int)state;
                    // Increment the number of active workers
                    // with an atomic operation
                    Interlocked.Increment(
                        ref _activeWorkers);
                    try
                    {
                        Debug.WriteLine(
                            "Worker #{0} started ", workerNum);

                        DoSomeWork(workerNum);

                        Debug.WriteLine(
                            "Worker #{0} finished ", workerNum);
                    }
                    finally
                    {
                        // Decrement the number of active workers
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
    }
}
