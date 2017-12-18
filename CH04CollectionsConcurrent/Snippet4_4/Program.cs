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

namespace Snippet4_4
{
    class Program
    {
        private static string[] _invalidHexValues = { "AF", "BD", "BF", "CF", "DA", "FA", "FE", "FF" };

        static void Main(string[] args)
        {
            var invalidHexValuesStack = new ConcurrentStack<string>(_invalidHexValues);

            while (!invalidHexValuesStack.IsEmpty)
            {
                string hexValue;
                invalidHexValuesStack.TryPop(out hexValue);
                Console.WriteLine(hexValue);
            }

            Console.ReadLine();
        }
    }
}
