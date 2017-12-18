using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snippet6_6
{
    class Program
    {
        static string[] words = {
            "Day", 
            "Car", 
            "Land", 
            "Road", 
            "Mountain", 
            "River", 
            "Sea", 
            "Shore", 
            "Mouse" };

        static void Main(string[] args)
        {
            var query = from word in words.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                        where (word.Contains('a'))
                        orderby word descending
                        select word;

            foreach (var result in query)
            {
                Console.WriteLine(result);
            }
            Console.ReadLine();
        }
    }
}