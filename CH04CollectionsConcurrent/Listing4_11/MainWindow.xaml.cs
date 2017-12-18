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
// Added for Stopwatch
using System.Diagnostics;
// Added for Tasks
using System.Threading.Tasks;
// Added for Concurrent Collections
using System.Collections.Concurrent;

namespace Listing4_11
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

        private ConcurrentDictionary<string, RectangleInfo> _rectanglesDict;
        private const int MAX_RECTANGLES = 20000;
        private void GenerateRectangles()
        {
            Parallel.For(1, MAX_RECTANGLES + 1, (i) =>
             {
                 for (int j = 0; j < 50; j++)
                 {
                     var newKey = String.Format("Rectangle {0}", i % 5000);
                     var newRect = new RectangleInfo(
                         String.Format("Rectangle {0}", i),
                         new Point(j, j * i),
                         new Size(j * i, i / 2));

                     _rectanglesDict.AddOrUpdate(
                         newKey, newRect,
                         (key, existingRect) =>
                         {
                             // The key already exists
                             if (existingRect != newRect)
                             {
                                 // The rectangles are different
                                 // It is necessary to update the existing rectangle
                                 // to update the existing rectangle
                                 // AddOrUpdate doesn’t run 
                                 // the add or update delegates 
                                 // while holding a lock
                                 // Lock existingRect before calling 
                                 // the Update method
                                 lock (existingRect)
                                 {
                                     // Call the Update method within
                                     // a critical section
                                     existingRect.Update(newRect.Location, newRect.Size);
                                 }
                                 return existingRect;
                             }
                             else
                             {
                                 // The rectangles are the same
                                 // No need to update the existing one
                                 return existingRect;
                             }
                         });
                 }
             });
        }

        private void butTest_Click(object sender, RoutedEventArgs e)
        {
            var sw = Stopwatch.StartNew();

            _rectanglesDict = new ConcurrentDictionary<string, RectangleInfo>();
            GenerateRectangles();

            foreach (var keyValuePair in _rectanglesDict)
            {
                listRectangles.Items.Add(
                   String.Format("{0}, {1}. Updated {2} times.",
                   keyValuePair.Key, keyValuePair.Value.Size,
                   keyValuePair.Value.UpdatedTimes));
            }

            Debug.WriteLine(sw.Elapsed.ToString());
        }
    }

    class RectangleInfo : IEqualityComparer<RectangleInfo>
    {
        // Name property has private set because you use it in a hash code
        // You don’t want to be able to change the Name
        // If you change the name, you may never be able
        // to find the object again in the dictionary
        public string Name { get; private set; }
        public Point Location { get; set; }
        public Size Size { get; set; }
        public DateTime LastUpdate { get; set; }
        public int UpdatedTimes { get; private set; }

        public RectangleInfo(string name, Point location, Size size)
        {
            Name = name;
            Location = location;
            Size = size;
            LastUpdate = DateTime.Now;
            UpdatedTimes = 0;
        }

        public RectangleInfo(string key)
        {
            Name = key;
            Location = new Point();
            Size = new Size();
            LastUpdate = DateTime.Now;
            UpdatedTimes = 0;
        }

        public void Update(Point location, Size size)
        {
            Location = location;
            Size = size;
            UpdatedTimes++;
        }

        public bool Equals(RectangleInfo rectA, RectangleInfo rectB)
        {
            return
                ((rectA.Name == rectB.Name) &&
                 (rectA.Size == rectB.Size) &&
                 (rectA.Location == rectB.Location));
        }

        public int GetHashCode(RectangleInfo obj)
        {
            RectangleInfo rectInfo = (RectangleInfo)obj;
            return rectInfo.Name.GetHashCode();
        }
    }
}
