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
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Windows.Interop;
using ipp;

namespace Listing11_4
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

        private BitmapData GetBitmapData(
            Bitmap bitmap, ImageLockMode lockMode)
        {
            return bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, 
                    bitmap.Width, bitmap.Height),
                lockMode,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        }

        unsafe private Bitmap ApplyFilterSobelVert()
        {
            // This is the file name to process
            string fileName =
                @"C:\Pictures\Test.jpg";
            var originalImage = new Bitmap(fileName);
            var srcBitmapData = GetBitmapData(
                originalImage, ImageLockMode.ReadOnly);

            var destinationImage = new Bitmap(
                originalImage.Width, originalImage.Height);
            var dstBitmapData = GetBitmapData(
                destinationImage, ImageLockMode.ReadWrite);

            IppiSize roi =
                new IppiSize(
                    originalImage.Width - 3,
                    originalImage.Height - 3);

            const int ksize = 5;
            const int half = ksize / 2;
            byte* pSrc = (byte*)srcBitmapData.Scan0 + 
                (srcBitmapData.Stride + 3) * half;
            byte* pDst = (byte*)dstBitmapData.Scan0 + 
                (dstBitmapData.Stride + 3) * half;

            IppStatus status =
                ipp.ip.ippiFilterSobelVert_8u_C3R(
                    pSrc, srcBitmapData.Stride,
                    pDst, dstBitmapData.Stride,
                    roi);

            // Unlock bits for both source and destination
            originalImage.UnlockBits(srcBitmapData);
            destinationImage.UnlockBits(dstBitmapData);

            return destinationImage;
        }

        private void butProcessImage_Click(
            object sender, RoutedEventArgs e)
        {
            butProcessImage.IsEnabled = false;

            // This scheduler will execute tasks 
            // on the current SynchronizationContext
            // That is, it will access UI controls safely
            var uiScheduler =
                TaskScheduler.FromCurrentSynchronizationContext();

            var filterTask = Task.Factory.StartNew<Bitmap>(
                () =>
                {
                    return ApplyFilterSobelVert();
                });

            filterTask.ContinueWith(
                (antecedentTask) =>
                {
                    // This code runs in the UI thread
                    try
                    {
                        var outBitmap = antecedentTask.Result;
                        // Show the results 
                        // in the Image control
                        imgProcessedImage.Source = 
                            Imaging.CreateBitmapSourceFromHBitmap(
                            outBitmap.GetHbitmap(), 
                            IntPtr.Zero, 
                            System.Windows.Int32Rect.Empty,
                            BitmapSizeOptions.FromWidthAndHeight(
                                outBitmap.Width, outBitmap.Height));
                    }
                    catch (AggregateException ex)
                    {
                        foreach 
                           (Exception innerEx 
                            in ex.InnerExceptions)
                        {
                            // Add code 
                            // to the error messages
                            // in innerEx.Message);
                        }
                    }
                    butProcessImage.IsEnabled = true;
                }, uiScheduler); 
        }
    }
}
