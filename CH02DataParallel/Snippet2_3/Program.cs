using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snippet2_3
{
    class Program
    {

        static void Main(string[] args)
        {
            Parallel.Invoke(
            () =>
            {
                ConvertEllipses();
                // Do something else adding more lines
            },
            () =>
            {
                ConvertRectangles();
                // Do something else adding more lines
            },
            delegate() //匿名委托
            {
                ConvertLines();
                // Do something else adding more lines
            },
            delegate()
            {
                ConvertText();
            });
            System.Console.ReadLine();
        }

        static void ConvertEllipses()
        {
            System.Console.WriteLine("Ellipses converted.");
        }


        static void ConvertRectangles()
        {
            System.Console.WriteLine("Rectangles converted.");
        }

        static void ConvertLines()
        {
            System.Console.WriteLine("Lines converted.");
        }

        static void ConvertText()
        {
            System.Console.WriteLine("Text converted.");
        }
    }
}