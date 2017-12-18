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
using System.Threading;

namespace Listing5_2
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

        // This code has references to two methods presented in
        // Listing 4-7: RemoveLetters and CapitalizeWords
        private const int NUM_SENTENCES = 2000000;
        private static ConcurrentBag<string> _sentencesBag;
        private static ConcurrentBag<string> _capWordsInSentencesBag;
        private static ConcurrentBag<string> _finalSentencesBag;

        private static ManualResetEventSlim _mresProduceSentences;
        private static ManualResetEventSlim _mresCapitalizeWords;

        private static void ProduceSentences()
        {

            string[] possibleSentences = 
            { 
               "ConcurrentBag is included in the Systems.Collections.Concurrent namespace.",
               "Is parallelism important for cloud-computing?",
               "Parallelism is very important for cloud-computing!",
               "ConcurrentQueue is one of the new concurrent collections added in .NET Framework 4",
               "ConcurrentStack is a concurrent collection that represents a LIFO collection",
               "ConcurrentQueue is a concurrent collection that represents a FIFO collection" 
            };

            try
            {
                // Signal/Set
                _mresProduceSentences.Set();

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
                // Switch to unsignaled/unset
                _mresProduceSentences.Reset();
            }
        }

        // 5,000 milliseconds = 5 seconds timeout
        private const int TIMEOUT = 5000;

        private static void CapitalizeWordsInSentences()
        {
            char[] delimiterChars = { ' ', ',', '.', ':', ';', '(', ')', '[', ']', '{', '}', '/', '?', '@', '\t', '"' };

            // Start after ProduceSentences began working
            // Wait for _mresProduceSentences to become signaled
            _mresProduceSentences.Wait();

            try
            {
                // Signal
                _mresCapitalizeWords.Set();
                // This condition running in a loop (spinning)
                // is very inefficient
                // This example uses this spinning for educational purposes
                // It isn’t a best practice, as explained in Chapter 4
                // Chapter 4 explained an improved version
                while ((!_sentencesBag.IsEmpty) || (_mresProduceSentences.IsSet))
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
                // Switch to unsignaled/unset
                _mresCapitalizeWords.Reset();
            }
        }

        private static void RemoveLettersInSentences()
        {
            char[] letterChars = { 'A', 'B', 'C', 'e', 'i', 'j', 'm', 'X', 'y', 'Z' };

            // Start after CapitalizeWordsInSentences began working
            // Wait for _mresCapitalizeWords to become signaled
            _mresCapitalizeWords.Wait();

            // This condition running in a loop (spinning)
            // is very inefficient
            // This example uses this spinning for educational purposes
            // It isn’t a best practice, as explained in Chapter 4
            // Chapter 4 explained an improved version
            while ((!_capWordsInSentencesBag.IsEmpty) || (_mresCapitalizeWords.IsSet))
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

            // Construct the two ManualResetEventSlim instances
            // with a spincount of 100, it will spin-wait 100 times before
            // switching to a kernel-based wait
            _mresProduceSentences = new ManualResetEventSlim(false, 100);
            _mresCapitalizeWords = new ManualResetEventSlim(false, 100);

            try
            {
                Parallel.Invoke(
                    () => ProduceSentences(),
                    () => CapitalizeWordsInSentences(),
                    () => RemoveLettersInSentences()
                    );
            }
            finally
            {
                // Dispose the two ManualResetEventSlim instances
                _mresProduceSentences.Dispose();
                _mresCapitalizeWords.Dispose();
            }

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