using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// Added for the Stopwatch
using System.Diagnostics;
// Added for the cryptography classes
using System.Security.Cryptography;
// This namespace will be used later to run code in parallel
using System.Threading.Tasks;
// Added to access the custom partitioner
using System.Collections.Concurrent;

namespace Snippet5_15
{
    class Program
    {
        private static string RemoveLetters(char[] letters, string sentence)
        {
            var sb = new StringBuilder();
            bool match = false;

            for (int i = 0; i < sentence.Length; i++)
            {
                for (int j = 0; j < letters.Length; j++)
                {
                    if (sentence[i] == letters[j])
                    {
                        match = true;
                        break;
                    }
                }
                if (!match)
                {
                    sb.Append(sentence[i]);
                }
                match = false;
            }
            return sb.ToString();
        }

        private static string CapitalizeWords(char[] delimiters, string sentence, char newDelimiter)
        {
            string[] words = sentence.Split(delimiters);
            var sb = new StringBuilder();
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 1)
                {
                    sb.Append(words[i][0].ToString().ToUpper());
                    sb.Append(words[i].Substring(1).ToLower());
                }
                else
                {
                    // Word with just 1 letter must be lowercase
                    sb.Append(words[i].ToLower());
                }
                sb.Append(newDelimiter);
            }
            return sb.ToString();
        }

        private const int NUM_SENTENCES = 2000000;
        private static ConcurrentBag<string> _sentencesBag;
        private static ConcurrentBag<string> _capWordsInSentencesBag;
        private static ConcurrentBag<string> _finalSentencesBag;
        private static volatile bool _producingSentences = false;
        private static volatile bool _capitalizingWords = false;

        private static void ProduceSentences()
        {

            string[] possibleSentences = 
            { 
               "ConcurrentBag is included in the System.Collections.Concurrent namespace.",
               "Is parallelism important for cloud-computing?",
               "Parallelism is very important for cloud-computing!",
               "ConcurrentQueue is one of the new concurrent collections added in .NET Framework 4",
               "ConcurrentStack is a concurrent collection that represents a LIFO collection",
               "ConcurrentQueue is a concurrent collection that represents a FIFO collection" 
            };

            try
            {
                // This code doesn't set _producingSentences to true
                // to generate an error
                //_producingSentences = true;
                var rnd = new Random();

                for (int i = 0; i < NUM_SENTENCES; i++)
                {
                    var sb = new StringBuilder();
                    for (int j = 0; j < possibleSentences.Length; j++)
                    {
                        if (rnd.Next(2) > 0)
                        {
                            sb.Append(possibleSentences[rnd.Next(possibleSentences.Length)]);
                            sb.Append(' ');
                        }
                    }
                    if (rnd.Next(20) > 15)
                    {
                        _sentencesBag.Add(sb.ToString());
                    }
                    else
                    {
                        _sentencesBag.Add(sb.ToString().ToUpper());
                    }
                }
            }
            finally
            {
                _producingSentences = false;
            }
        }
        
        // 5,000 milliseconds = 5 seconds timeout
        private const int TIMEOUT = 5000;

        private static void CapitalizeWordsInSentences()
        {
            char[] delimiterChars = { ' ', ',', '.', ':', ';', '(', ')', '[', ']', '{', '}', '/', '?', '@', '\t', '"' };

            // Start after Produce sentences began working
            // use a 5 seconds timeout
            if (!System.Threading.SpinWait.SpinUntil(() => _producingSentences, TIMEOUT))
            {
                throw new TimeoutException(
                    String.Format(
                        "CapitalizeWordsInSentences has been waiting " +
                        "for {0} seconds to access sentences.",
                        TIMEOUT));
            }

            try
            {
                _capitalizingWords = true;
                // This condition running in a loop (spinning) is very inefficient
                // This example uses this spinning for educational purposes
                // It isn’t a best practice, as explained in Chapter 4
                // Chapter 4 explained an improved version
                while ((!_sentencesBag.IsEmpty) || (_producingSentences))
                {
                    string sentence;
                    if (_sentencesBag.TryTake(out sentence))
                    {
                        _capWordsInSentencesBag.Add(CapitalizeWords(delimiterChars, sentence, '\\'));
                    }
                }
            }
            finally
            {
                _capitalizingWords = false;
            }
        }

        private static void RemoveLettersInSentences()
        {
            char[] letterChars = { 'A', 'B', 'C', 'e', 'i', 'j', 'm', 'X', 'y', 'Z' };

            // Start after CapitalizedWordsInsentences began working
            System.Threading.SpinWait.SpinUntil(() => _capitalizingWords);
            // This condition running in a loop (spinning) is very inefficient
            // This example uses this spinning for educational purposes
            // It isn’t a best practice
            // Subsequent sections and chapters explain an improved version
            while ((!_capWordsInSentencesBag.IsEmpty) || (_capitalizingWords))
            {
                string sentence;
                if (_capWordsInSentencesBag.TryTake(out sentence))
                {
                    _finalSentencesBag.Add(RemoveLetters(letterChars, sentence));
                }
            }
        }

        static void Main(string[] args)
        {
            var sw = Stopwatch.StartNew();

            _sentencesBag = new ConcurrentBag<string>();
            _capWordsInSentencesBag = new ConcurrentBag<string>();
            _finalSentencesBag = new ConcurrentBag<string>();

            // This code doesn't set _producingSentences to true
            // to generate an error

            //// Set _productingSentences to true before calling
            //// ProduceSentences concurrently with the other methods
            //// This way, CapitalizeWordsInSentences 
            //// and RemoveLettersInSentences
            //// will see the true value for this shared variable
            //_producingSentences = true;

            Parallel.Invoke(
                () => ProduceSentences(),
                () => CapitalizeWordsInSentences(),
                () => RemoveLettersInSentences()
                );

            Console.WriteLine(
                "Number of sentences with capitalized words in the bag: {0}",
                _capWordsInSentencesBag.Count);
            Console.WriteLine(
                "Number of sentences with removed letters in the bag: {0}",
                _finalSentencesBag.Count);

            Debug.WriteLine(sw.Elapsed.ToString());
            //  Display the results and wait for the user to press a key
            Console.WriteLine("Finished!");
            Console.ReadLine();
        }
    }
}