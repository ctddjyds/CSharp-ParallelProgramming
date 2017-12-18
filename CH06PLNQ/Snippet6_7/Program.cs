using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snippet6_7
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

        static int CountLetters(String key)
        {
            int letters = 0;
            for (int i = 0; i < key.Length; i++)
            {
                if (Char.IsLetter(key, i))
                {
                    letters++;
                }
            }
            return letters;
        }

        static void Main(string[] args)
        {
            var query = (from word in words.AsParallel()
                        where (word.Contains('a'))
                        select CountLetters(word)).Sum();

            Console.WriteLine("The total number of letters for the words that contain an 'a' is {0}", query);
            Console.ReadLine();
        }
    }
}