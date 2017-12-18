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
using System.Threading.Tasks;
using System.IO;

// This code has performance problems
// Don’t consider this code as a best practice
namespace Listing10_9
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private TaskFactory _uiTF;
        
        private static int NUM_INTS = 50000;
        
        private static ParallelQuery<int> GenerateInputData()
        {
            return ParallelEnumerable.Range(1, NUM_INTS);
        }
        
        private void AdvanceProgressBar()
        {
            // This code runs in the UI thread
            // there is no possible concurrency
            pgbProgress.Value++;
        }
        
        private double CalculateX(int intNum)
        {
            // Horrible code that updates
            // the UI thousands of times
            var uiTask = _uiTF.StartNew(
            () =>
            {
                AdvanceProgressBar();
            });
            return Math.Pow(Math.Sqrt(intNum / Math.PI), 3);
        }
        private void butRunQuery_Click(
        object sender, RoutedEventArgs e)
        {
            var uiScheduler =
                TaskScheduler.FromCurrentSynchronizationContext();
            
            _uiTF = new TaskFactory(uiScheduler);
            
            var inputIntegers = GenerateInputData();
            
            // Reset the progress bar
            pgbProgress.Value = 0;
            
            // Set the maximum value for the progress bar
            pgbProgress.Maximum = inputIntegers.Count();
            
            var taskQ = Task<double>.Factory.StartNew(
            () =>
            {
                var parReductionQuery =
                (from intNum in inputIntegers.AsParallel()
                 select (CalculateX(intNum))).Average();
                return parReductionQuery;
            });
            
            taskQ.ContinueWith(
            (t) =>
            {
                // This code runs
                // in the UI thread
                txtResult.Text = String.Format("Average: {0}",
                t.Result);
            }, uiScheduler);
        }
    }
}