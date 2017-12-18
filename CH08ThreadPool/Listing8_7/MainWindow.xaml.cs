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
using System.Threading.Tasks.Schedulers;

namespace Listing8_7
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // The total number of workers
        private int _workerCount = 0;

        // The number of active workers
        private int _activeWorkers = 0;

        // The list of worker tasks
        private List<Task> _workerTasks = new List<Task>();

        // The TaskFactory instance
        // that uses the ThreadPerTaskScheduler
        private TaskFactory _threadFactory;

        // The thread per task scheduler
        private ThreadPerTaskScheduler _taskScheduler;

        public MainWindow()
        {
            InitializeComponent();

            _taskScheduler = new ThreadPerTaskScheduler();

            _threadFactory = new TaskFactory(_taskScheduler);
        }

        private void DoSomeWork(int workerNum)
        {
            // Simulate some work
            // between 7 and 12 seconds
            var milliSecs =
                new Random().Next(7000, 12000);
            Debug.WriteLine(
                "Worker {0} will simulate work for {1} milliseconds",
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
            // Create and start a new Task
            // and send _workerCount as a
            // parameter to the action delegate
            // Uses _threadFactory instead of Task.Factory
            var workerTask = _threadFactory.StartNew((num) =>
            {
                // Retrieve the workerNum received
                // as a parameter in object state
                int workerNum = (int)num;
                // Increment the number of active workers
                // with an atomic operation
                Interlocked.Increment(
                    ref _activeWorkers);
                try
                {
                    Debug.WriteLine(
                        "Worker {0} started ", workerNum);

                    DoSomeWork(workerNum);

                    Debug.WriteLine(
                        "Worker {0} finished ", workerNum);
                }
                finally
                {
                    // Increment the number of active workers
                    // with an atomic operation
                    Interlocked.Decrement(
                        ref _activeWorkers);
                }
            }, _workerCount);

            // Add the new Task instance
            // to the _workerTasks list
            _workerTasks.Add(workerTask);
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
            // Wait for all the tasks in _workerTasks
            // to finish their work
            // It is obviously a very bad practice
            // to block the UI thread like this
            // This is done just for example purposes
            Task.WaitAll(_workerTasks.ToArray());
            butWaitAll.Content = "Done!";
        }
    }
}
