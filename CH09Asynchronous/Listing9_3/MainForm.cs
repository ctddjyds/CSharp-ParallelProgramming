﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
// Added
using System.Threading.Tasks;
using System.IO;

namespace Listing9_4
{
    public partial class MainForm : Form
    {
        public MainForm()
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
                    BUFFER_SIZE, true);

            // Wrap the APM read operation in a Task
            // Encapsulate the APM stream.BeginRead and
            // stream.EndRead methods in task
            Task<int> task =
                Task<int>.Factory.FromAsync(
                stream.BeginRead, stream.EndRead,
                data, 0, data.Length, null,
                TaskCreationOptions.None);

            // task will hold the number of bytes
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
                return new UTF8Encoding().
                    GetString(data);
            },
                TaskContinuationOptions.ExecuteSynchronously);
        }

        private void AdvanceProgressBar()
        {
            // This code runs in the UI thread
            // there is no possible concurrency
            // Neither a lock nor an atomic operation
            // is necessary
            pgbFiles.Value++;
        }

        private void butConcatTextFiles_Click(
            object sender, EventArgs e)
        {
            // This scheduler will execute tasks 
            // on the current SynchronizationContext
            // That is, it will access UI controls safely
            var uiScheduler =
                TaskScheduler.FromCurrentSynchronizationContext();

            // Reset the progress bar
            pgbFiles.Value = 0;
            // Set the maximum value for the progress bar
            pgbFiles.Maximum = fileNames.Length;

            Task.Factory.StartNew(() =>
            {
                fileTasks =
                    new List<Task<string>>(fileNames.Length);

                foreach (string fileName in fileNames)
                {
                    // Launch a Task<string>
                    // that will hold all the data
                    // read from the file in a string
                    // as the task's result
                    var readTask =
                        ReadAllTextAsync(
                            @"C:\MYFILES\" + fileName);

                    readTask.ContinueWith(
                        (t) =>
                        {
                            // This code runs 
                            // in the UI thread
                            AdvanceProgressBar();
                        }, uiScheduler);

                    // Add the new Task<string> to fileTasks
                    // There is no need to call the Start method
                    // because the task was created 
                    // with the FromAsync method
                    fileTasks.Add(readTask);
                }

                // Program a continuation
                // to start after all the asynchronous
                // operations finish

                var builderTask =
                    Task.Factory.ContinueWhenAll(
                    fileTasks.ToArray(),
                    (antecedentTasks) =>
                    {
                        // Wait for all the antecedent 
                        // tasks to ensure the propagation 
                        // of any exception occured 
                        // in any of the antecedent tasks
                        Task.WaitAll(antecedentTasks);

                        var sb = new StringBuilder();
                        // Write all the data read from the files
                        foreach (Task<string> fileTask
                            in antecedentTasks)
                        {
                            sb.Append(fileTask.Result);
                        }

                        return sb.ToString();
                    });

                builderTask.ContinueWith(
                        (antecedentTask) =>
                        {
                            // This code runs 
                            // in the UI thread
                            txtAllText.Text =
                                antecedentTask.Result;
                        }, uiScheduler);
            });
        }
    }
}
