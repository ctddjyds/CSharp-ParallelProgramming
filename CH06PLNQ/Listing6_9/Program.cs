using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Listing6_9
{
    class Program
    {

        public static List<string> words = new List<string> 
            { "there", "is", "a", 
                "great", "house", "and", 
                "an", "amazing", "lake", 
                "there", "is", "a", 
                "computer", "running", "a", 
                "new", "query", "there", 
                "is", "a", "great", 
                "server", "ready", "to", 
                "process", 
                "map", "and", "reduce" };

        static void Main(string[] args)
        {
            // Map
            // Generate a (word, 1) key, value pair
            ILookup<string, int> map = words.AsParallel().ToLookup(p => p, k => 1);
            // End of Map

            // Reduce
            // Calculate the number of times a word appears and select the words that appear more than once
            var reduce = from IGrouping<string, int> wordMap 
                         in map.AsParallel()
                         where wordMap.Count() > 1
                         select new 
                            { Word = wordMap.Key, Count = wordMap.Count() };
            // End of Reduce

            // Show each word and the number of times it appears
            foreach (var word in reduce)
                Console.WriteLine("Word: '{0}'; Count: {1}", 
                    word.Word, word.Count);

            Console.ReadLine();
        }
    }
}
