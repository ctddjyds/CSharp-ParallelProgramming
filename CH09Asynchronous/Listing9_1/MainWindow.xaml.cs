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
using System.Threading.Tasks;
using System.IO;

namespace Listing9_1
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

        // The file names to read
        private string[] fileNames = {
            "file01.txt", 
            "file02.txt", 
            "file03.txt", 
            "file04.txt", 
            "file05.txt", 
            "file06.txt", 
            "file07.txt", 
            "file08.txt", 
            "file09.txt", 
            "file10.txt", 
            "file11.txt", 
            "file12.txt", 
            "file13.txt", 
            "file14.txt", 
            "file15.txt", 
            "file16.txt"
        };

        // The list of tasks
        // Each Task instance returns a string
        // and reads a file with an APM operation
        private List<Task<string>> fileTasks;

        // The buffer size for the stream reader
        private const int BUFFER_SIZE = 0x2000;

        private Task<string> ReadAllTextAsync(string path)
        {
            // Get information for the file
            FileInfo info = new FileInfo(path);
            if (!info.Exists)
            {
                // The file doesn't exist
                throw new FileNotFoundException(
                    String.Format("{0} does not exist.", path),
                    path);
            }

            // This array will hold the data
            // read from the file
            byte[] data = new byte[info.Length];

            // Create the new FileStream
            // to read from the file with
            // useAsync set to true
            // in order to use asynchronous input
            FileStream stream =
                new FileStream(path, FileMode.Open,
                    FileAccess.Read, FileShare.Read,
                    BUFFER_SIZE,  true);

            // Wrap the APM read operation in a Task
            // Encapsulate the APM stream.BeginRead and
            // stream.EndRead methods in task
            Task<int> task = 
                Task<int>.Factory.FromAsync(
                stream.BeginRead, stream.EndRead,
                data, 0, data.Length, null,
                TaskCreationOptions.None);

            // The task Task will hold the number of bytes
            // read as a result
            // Add a continuation to task
            // and return it
            return task.ContinueWith((t) =>
                {
                    stream.Close();
                    Console.WriteLine(
                        "One task has read {0} bytes from {1}.",
                        t.Result, stream.Name);

                    // This continuation Task returns
                    // the data read from the file
                    // as a string with UTF8 encoding
                    return (t.Result > 0) ?
                        new UTF8Encoding().GetString(data) : "";
                }, 
                TaskContinuationOptions.ExecuteSynchronously);
        }

        private void butConcatTextFiles_Click(object sender, RoutedEventArgs e)
        {
            fileTasks = new List<Task<string>>(fileNames.Length);
            foreach (string fileName in fileNames)
            {
                // Launch a Task<string>
                // that will hold all the data
                // read from the file in a string
                // as the task's result
                var readTask = 
                    ReadAllTextAsync(
                        @"C:\MYFILES\" + fileName);
                // Add the new Task<string> to fileTasks
                // There is no need to call the Start method
                // because the task was created 
                // with the FromAsync method
                fileTasks.Add(readTask);
            }
            // Block this thread waiting 
            // until all the tasks finish their work
            Task.WaitAll(fileTasks.ToArray());

            // Write all the data read from the files
            foreach (Task<string> fileTask in fileTasks)
            {
                Console.WriteLine(fileTask.Result);
            }
        }
    }
}
