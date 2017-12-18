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

namespace Listing4_10
{
    class PipeLineDemo
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

        private static string ProduceASentence(string[] possibleSentences)
        {
            var rnd = new Random();
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
            return newSentence;
        }

        public static void Main()
        {
            var cts = new System.Threading.CancellationTokenSource();
            var ct = cts.Token;
            var sw = Stopwatch.StartNew();

            BlockingCollection<string>[] sourceSentences = new BlockingCollection<string>[5];
            for (int i = 0; i < sourceSentences.Length; i++)
            {
                sourceSentences[i] = new BlockingCollection<string>(NUM_SENTENCES / 5);
            }

            string[] possibleSentences = 
            { 
               "ConcurrentBag is included in the System.Collections.Concurrent namespace.",
               "Is parallelism important for cloud-computing?",
               "Parallelism is very important for cloud-computing!",
               "ConcurrentQueue is one of the new concurrent collections added in .NET Framework 4",
               "ConcurrentStack is a concurrent collection that represents a LIFO collection",
               "ConcurrentQueue is a concurrent collection that represents a FIFO collection" 
            };

            Parallel.For(0, NUM_SENTENCES, (sentenceNumber) =>
                {
                    BlockingCollection<string>.TryAddToAny(
                       sourceSentences, 
                       ProduceASentence(possibleSentences), 
                       50);
                });

            for (int j = 0; j < sourceSentences.Length; j++)
            {
                sourceSentences[j].CompleteAdding();
            }

            char[] delimiterChars = 
               { ' ', ',', '.', ':', ';', '(', ')', '[', 
                 ']', '{', '}', '/', '?', '@', '\t', '"' };

            var filterCapitalizeWords = new PipelineFilter<string, string>
            (
                sourceSentences,
                (sentence) => CapitalizeWords(
                   delimiterChars, sentence, '\\'),
                ct,
                "CapitalizeWords"
            );

            char[] letterChars = 
               { 'A', 'B', 'C', 'e', 'i', 
                 'j', 'm', 'X', 'y', 'Z' };
            var filterRemoveLetters = new PipelineFilter<string, string>
            (
                filterCapitalizeWords.Output,
                (sentence) => RemoveLetters(letterChars, sentence),
                ct,
                "RemoveLetters"
             );

            var filterWriteLine = new PipelineFilter<string, string>
            (
                filterRemoveLetters.Output,
                (sentence) => Console.WriteLine(sentence),
                ct,
                "WriteLine"
             );

            var deferredCancelTask = Task.Factory.StartNew(() =>
            {
                // Sleep the thread that runs this task for 2 seconds
                System.Threading.Thread.Sleep(2000);
                // Send the signal to cancel
                cts.Cancel();
            });

            try
            {
                Parallel.Invoke(
                    () => filterCapitalizeWords.Run(),
                    () => filterRemoveLetters.Run(),
                    () => filterWriteLine.Run()
                );
            }
            catch (AggregateException ex)
            {
                foreach (var innerEx in ex.InnerExceptions)
                    Console.WriteLine(innerEx.Message);
            }

            Debug.WriteLine(sw.Elapsed.ToString());
            //  Display the results and wait for the user to press a key
            Console.WriteLine("Finished!");
            Console.ReadLine();
        }

        class PipelineFilter<TInput, TOutput>
        {
            private Func<TInput, TOutput> _processor = null;
            private Action<TInput> _outputProcessor = null;
            private System.Threading.CancellationToken _token;
            public BlockingCollection<TInput>[] Input;
            public BlockingCollection<TOutput>[] Output = null;
            public string Name { get; private set; }
            private const int OUT_COLLECTIONS = 5;
            private const int OUT_BOUNDING_CAPACITY = 1000;
            private const int TIMEOUT = 50;

            // Constructor for the Func<TInput, TOutput> filter
            public PipelineFilter(
                BlockingCollection<TInput>[] input,
                Func<TInput, TOutput> processor,
                System.Threading.CancellationToken token,
                string name)
            {
                Input = input;
                Output = new BlockingCollection<TOutput>[OUT_COLLECTIONS];
                // Create the output BlockingCollection instances
                for (int i = 0; i < Output.Length; i++)
                {
                    Output[i] = new BlockingCollection<TOutput>(OUT_BOUNDING_CAPACITY);
                }
                _processor = processor;
                _token = token;
                Name = name;
            }

            // Constructor for the Action<TInput> filter
            // The final consumer
            public PipelineFilter(
                BlockingCollection<TInput>[] input,
                Action<TInput> renderer,
                System.Threading.CancellationToken token,
                string name)
            {
                Input = input;
                _outputProcessor = renderer;
                _token = token;
                Name = name;
            }

            public void Run()
            {
                // Run while all the input BlockingCollection instances IsCompleted is false
                // and no cancellation was requested
                while ((!Input.All(inputBC => inputBC.IsCompleted))
                   && (!_token.IsCancellationRequested))
                {
                    TInput item;
                    int i = BlockingCollection<TInput>.TryTakeFromAny(
                       Input, out item, TIMEOUT, _token);
                    if (i >= 0)
                    {
                        if (Output != null)
                        {
                            // Process the item
                            TOutput result = _processor(item);
                            // Add the result to any of the output collections
                            BlockingCollection<TOutput>.AddToAny(Output, result, _token);
                        }
                        else
                        {
                            // The code is running the last consumer
                            _outputProcessor(item);
                        }
                    }
                }
                if (Output != null)
                {
                    // All the BlockingCollection instances finished
                    foreach (var outputBC in Output)
                    {
                        outputBC.CompleteAdding();
                    }
                }
            }
        }
    }
}
