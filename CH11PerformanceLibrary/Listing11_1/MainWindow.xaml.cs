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
// Added for tasks
using System.Threading.Tasks;
// Added for MKL
using mkl;

namespace Listing11_1
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

        // The size of the input array of double
        private const int INPUT_SIZE = 30000;

        private double[] Generate1DInput()
        {
            double[] data = new double[INPUT_SIZE];
            for (int i = 0; i < INPUT_SIZE; i++)
            {
                data[i] =
                    Math.Sin(i + 1) * 
                    Math.Sqrt(3d) / 2d;
            }

            return data;
        }

        private double[] DFT1DRealToComplex()
        {
            /* Real-to-complex 1D transform
             * for double precision data 
             * not inplace with pack format
             * Configuration parameters for MKL DFTI
             * DFTI.FORWARD_DOMAIN = DFTI.REAL
             * DFTI.PRECISION      = DFTI.DOUBLE
             * DFTI.DIMENSION      = 1
             * DFTI.LENGTHS        = INPUT_SIZE (n)
             * DFTI.PLACEMENT      = DFTI.NOT_INPLACE
             * DFTI.BACKWARD_SCALE = (1.0 / n) (backwardScale)
             * Default values:
             * DFTI.PACKED_FORMAT  = DFTI.PACK_FORMAT
             * DFTI.FORWARD_SCALE  = 1.0          
             */
            IntPtr descriptor = new IntPtr();
            int precision = DFTI.DOUBLE;
            int forwardDomain = DFTI.REAL;
            int dimension = 1;
            int n = INPUT_SIZE;

            // The input data to be transformed
            double[] input = Generate1DInput();

            // Create a new DFTI descriptor
            DFTI.DftiCreateDescriptor(ref descriptor,
                precision, forwardDomain, dimension, n);

            // Configure DFTI.BACKWARD_SCALE
            double backwardScale = 1.0 / n;
            DFTI.DftiSetValue(descriptor, 
                DFTI.BACKWARD_SCALE, 
                backwardScale);

            // Configure DFTI.PLACEMENT
            DFTI.DftiSetValue(descriptor, 
                DFTI.PLACEMENT, 
                DFTI.NOT_INPLACE);

            // Configure DFTI.PACKET_FORMAT
            DFTI.DftiSetValue(descriptor, 
                DFTI.PACKED_FORMAT, 
                DFTI.PACK_FORMAT);

            // Commit the descriptor with the configuration
            DFTI.DftiCommitDescriptor(descriptor);

            // This is the output array
            double[] output = new double[n];

            // Compute the forward transform
            var err = DFTI.DftiComputeForward(descriptor, 
                input, 
                output);

            // Free the descriptor
            DFTI.DftiFreeDescriptor(ref descriptor);

            if (err == DFTI.NO_ERROR)
            {
                return output;
            }
            else
            {
                throw new MKLException(
                    String.Format("DFTI returned error code {0}",
                    err));
            }
        }

        // The output of the Forward Fast Fourier Transform
        private double[] _output;

        private void butRun_Click(object sender, RoutedEventArgs e)
        {
            butRun.IsEnabled = false;

            // This scheduler will execute tasks 
            // on the current SynchronizationContext
            // That is, it will access UI controls safely
            var uiScheduler = 
                TaskScheduler.FromCurrentSynchronizationContext();

            var fourierTask = Task.Factory.StartNew<double[]>(
                () =>
                {
                    return DFT1DRealToComplex();
                });

            fourierTask.ContinueWith(
                (antecedentTask) =>
                {
                    // This code runs in the UI thread
                    try
                    {
                        _output = antecedentTask.Result;
                        // Show the results 
                        // in the lstOutput ListBox
                        lstOutput.ItemsSource = _output;
                    }
                    catch (AggregateException ex)
                    {
                        lstOutput.Items.Clear();
                        foreach (Exception innerEx in ex.InnerExceptions)
                        {
                            // Show the error message
                            lstOutput.Items.Add(innerEx.Message);
                        }
                    }
                    butRun.IsEnabled = true;
                }, uiScheduler); 
        }
    }
}