﻿using System;
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

namespace Listing4_9
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
        private static BlockingCollection<string> _sentencesBC;
        private static BlockingCollection<string> _capWordsInSentencesBC;
        private static BlockingCollection<string> _finalSentencesBC;

        private static void ProduceSentences(System.Threading.CancellationToken ct)
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

            var rnd = new Random();

            for (int i = 0; i < NUM_SENTENCES; i++)
            {
                var sb = new StringBuilder();
                string newSentence;
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
                    newSentence = sb.ToString();
                }
                else
                {
                    newSentence = sb.ToString().ToUpper();
                }
                try
                {
                    if (!_sentencesBC.TryAdd(newSentence, 2000, ct))
                    {
                        throw new TimeoutException("_sentencesBC took more than 2 seconds to add an item");
                    }
                }
                catch (OperationCanceledException ex)
                {
                    // The operation was cancelled
                    Debug.WriteLine(ex.ToString());
                    break;
                    // The next statement after the loop will let
                    // the consumer know th producer's work is done
                }
                catch (TimeoutException ex)
                {
                    Debug.WriteLine(ex.ToString());
                    break;
                }
            }
            // Let the consumer know the producer's work is done
            _sentencesBC.CompleteAdding();
        }

        private static void CapitalizeWordsInSentences()
        {
            char[] delimiterChars = { ' ', ',', '.', ':', ';', '(', ')', '[', ']', '{', '}', '/', '?', '@', '\t', '"' };

            while (!_sentencesBC.IsCompleted)
            {
                string sentence;
                if (_sentencesBC.TryTake(out sentence))
                {
                    _capWordsInSentencesBC.Add(CapitalizeWords(delimiterChars, sentence, '\\'));
                }
            }
            // Let the consumer know the producer's work is done
            _capWordsInSentencesBC.CompleteAdding();
        }

        private static void RemoveLettersInSentences()
        {
            char[] letterChars = { 'A', 'B', 'C', 'e', 'i', 'j', 'm', 'X', 'y', 'Z' };

            while (!_capWordsInSentencesBC.IsCompleted)
            {
                string sentence;
                if (_capWordsInSentencesBC.TryTake(out sentence))
                {
                    _finalSentencesBC.Add(RemoveLetters(letterChars, sentence));
                }
            }
            _finalSentencesBC.CompleteAdding();
        }

        static void Main(string[] args)
        {
            var sw = Stopwatch.StartNew();

            // Set the maximum number of elements to NUM_SENTENCES / 20
            _sentencesBC =
                new BlockingCollection<string>(new ConcurrentStack<string>(), NUM_SENTENCES / 20);
            _capWordsInSentencesBC = new BlockingCollection<string>(NUM_SENTENCES / 20);
            _finalSentencesBC = new BlockingCollection<string>(NUM_SENTENCES / 20);


            var cts = new System.Threading.CancellationTokenSource();
            var ct = cts.Token;
            var deferredCancelTask = Task.Factory.StartNew(() =>
            {
                // Sleep the thread that runs this task for 2 seconds
                System.Threading.Thread.Sleep(500);
                // Send the signal to cancel
                cts.Cancel();
            });

            Parallel.Invoke(
                () => ProduceSentences(ct),
                () => CapitalizeWordsInSentences(),
                () => RemoveLettersInSentences(),
                () =>
                {
                    foreach (var sentence in _finalSentencesBC.GetConsumingEnumerable(ct))
                    {
                        Console.WriteLine(sentence);
                    }
                });

            Console.WriteLine(
                "Number of sentences with capitalized words in the collection: {0}",
                _capWordsInSentencesBC.Count());
            Console.WriteLine(
                "Number of sentences with removed letters in the collection: {0}",
                _finalSentencesBC.Count());
            Debug.WriteLine(sw.Elapsed.ToString());
            //  Display the results and wait for the user to press a key
            Console.WriteLine("Finished!");
            Console.ReadLine();
        }
    }
}
