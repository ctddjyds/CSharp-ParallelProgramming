using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snippet2_1

{
    class Program
    {

        static void Main(string[] args)
        {
            //对给定的独立任务提供潜在的并行执行
            //自动创建指向方法的委托
            Parallel.Invoke(ConvertEllipses,
                ConvertRectangles,
                ConvertLines,
                ConvertText);
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