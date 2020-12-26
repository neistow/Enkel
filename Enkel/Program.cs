using System;
using System.Collections.Immutable;
using System.Linq;
using Enkel.Utilities;

namespace Enkel
{
    class Program
    {
        static void Main(string[] args)
        {
            var logger = new ConsoleLogger();
            var enkel = new Enkel(logger);
            
            if (!args.Any())
            {
                enkel.RunPrompt();
                return;
            }
            
            if (args.Length == 1)
            {
                enkel.RunFile(args[0]);
                return;
            }
            
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Invalid amount of arguments. Usage: enkel *filename*");
        }
    }
}