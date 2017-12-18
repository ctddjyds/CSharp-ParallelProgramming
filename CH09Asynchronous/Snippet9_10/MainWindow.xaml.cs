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
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using System.Net;

namespace Snippet9_10
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

        // The addresses for the 
        // files to download
        private string[] fileAddresses = {
 @"http://media.wiley.com/product_data/excerpt/4X/04705022/047050224X.pdf", 
 @"http://media.wiley.com/product_data/excerpt/4X/04705022/047050224X-1.pdf", 
 @"http://media.wiley.com/product_data/excerpt/4X/04705022/047050224X-2.pdf"
        };

        // The list of tasks
        // Each Task instance returns a string
        // with the file name downloaded
        // and downloads a file with an EAP operation
        private List<Task<string>> _downloadTasks;

        private CancellationTokenSource _cts;
        private CancellationToken _ct;

        private void CreateToken()
        {
            _cts = new System.Threading.CancellationTokenSource();
            _ct = _cts.Token;
        }

        private Task<string> DownloadFileInTask(Uri address, CancellationToken ct)
        {
            // This is the task that the method
            // is going to return
            var tcs =
                new TaskCompletionSource<string>(address);

            // Get the file name
            // from the last part of the URI
            string fileName =
                @"C:\MYFILES\" +
                address.Segments[address.Segments.Length - 1];

            // Get information for the file
            FileInfo info = new FileInfo(fileName);
            if (info.Exists)
            {
                // The file already exists
                tcs.TrySetException(
                    new InvalidOperationException(
                    String.Format("{0} already exists.",
                    fileName)));

                // Return the Task<string>
                // created by the 
                // TaskCompletionSource<string>
                return tcs.Task;
            }

            var wc = new WebClient();

            // If there is a request to cancel
            // the ct CancellationToken
            // the CancelAsync method cancels
            // the WebClient's async operation
            ct.Register(
                () =>
                {
                    if (wc != null)
                    {
                        // wc wasn't disposed
                        // and it is possible
                        // to cancel the async
                        // download
                        wc.CancelAsync();
                        // Set the canceled status
                        tcs.TrySetCanceled();
                    }
                });

            // Declare handler as null
            // to be able to use it 
            // within the delegate's code
            AsyncCompletedEventHandler handler = null;

            // Now, define the delegate that handles
            // the completion of the asynchronous operation
            handler =
                (hSender, hE) =>
                {
                    if (hE.Error != null)
                    {
                        // An error occurred
                        // Set an exception
                        tcs.TrySetException(hE.Error);
                    }
                    else if (hE.Cancelled)
                    {
                        // The async operation
                        // was cancelled
                        // Set the canceled status
                        tcs.TrySetCanceled();
                    }
                    else
                    {
                        // It worked!
                        // Set the result
                        tcs.TrySetResult(fileName);
                        //butDownloadFiles.Content = "Downloaded!";
                    }

                    // Unregister the callback
                    wc.DownloadFileCompleted -= handler;
                };

            wc.DownloadFileCompleted += handler;

            try
            {
                wc.DownloadFileAsync(address, fileName);
            }
            catch (Exception ex)
            {
                // Something went wrong when the async operation
                // was trying to start

                // Unregister the callback
                wc.DownloadFileCompleted -= handler;

                // Set an exception
                tcs.TrySetException(ex);
            }

            // Return the Task<string>
            // created by the 
            // TaskCompletionSource<string>
            return tcs.Task;
        }

        private void butDownloadFiles_Click(object sender, RoutedEventArgs e)
        {
            CreateToken();

            // This scheduler will execute tasks 
            // on the current SynchronizationContext
            // That is, it will access UI controls safely
            var uiScheduler =
                TaskScheduler.FromCurrentSynchronizationContext();

            // Clear the ListBox
            lstStatus.Items.Clear();

            // Enable butCancel
            butCancel.IsEnabled = true;

            Task.Factory.StartNew(() =>
            {
                _downloadTasks =
                    new List<Task<string>>(fileAddresses.Length);

                foreach (string address in fileAddresses)
                {
                    // Launch a Task<string>
                    // that will download the file
                    var downloadTask =
                        DownloadFileInTask(new Uri(address), _ct);

                    downloadTask.ContinueWith(
                        (t) =>
                        {
                            string line = "";

                            if (t.IsCanceled)
                            {
                                line = "Canceled.";
                            }
                            else if (t.IsFaulted)
                            {
                                foreach (Exception innerEx
                                    in t.Exception.InnerExceptions)
                                {
                                    // Just one exception
                                    // No need to worry about the usage
                                    // of a StringBuilder instead of +=
                                    line += innerEx.Message;
                                }
                            }
                            else
                            {
                                line = t.Result;
                            }
                            // This code runs 
                            // in the UI thread
                            lstStatus.Items.Add(line);
                        }, uiScheduler);

                    // Add the new Task<string> 
                    // to _downloadTasks
                    // There is no need to call the Start method
                    // because the Task comes 
                    // from TaskCompletionSource 
                    _downloadTasks.Add(downloadTask);
                }

                var uiTF = new TaskFactory(uiScheduler);
                uiTF.ContinueWhenAll(
                    _downloadTasks.ToArray(),
                    (antecedentTasks) =>
                    {
                        // Disable butCancel
                        butCancel.IsEnabled = false;

                        // Runs in the UI thread
                        var completed =
                            (from task in antecedentTasks
                             where !(task.IsCanceled || task.IsFaulted)
                             select task).Count();

                        if (completed == antecedentTasks.Length)
                        {
                            lstStatus.Items.Add(
                                "All downloads completed!");
                        }
                        _downloadTasks.Clear();
                    });
            });
        }

        private void butCancel_Click(object sender, RoutedEventArgs e)
        {
            // Communicate a request for cancellation
            _cts.Cancel();
        }
    }
}
