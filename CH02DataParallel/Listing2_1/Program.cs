using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Listing2_1
{
    class Program
    {

        static void Main(string[] args)
        {
            Parallel.Invoke(
            () => ConvertEllipses(),
            () => ConvertRectangles(),
            () => ConvertLines(),
            () => ConvertText());
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
