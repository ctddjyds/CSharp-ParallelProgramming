using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snippet6_4
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
            var query = from word in words.AsParallel().AsOrdered()
                        where (word.Contains('a'))
                        select word;

            foreach (var result in query)
            {
                Console.WriteLine(result);
            }
            Console.ReadLine();
        }
    }
}
