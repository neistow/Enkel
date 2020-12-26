using System;
using System.IO;
using Enkel.Core.Exceptions;
using Enkel.Core.Interpreter.Interfaces;
using Enkel.Interpreter;
using Enkel.Lexer;
using Enkel.Parser;
using Enkel.Utilities;

namespace Enkel
{
    public class Enkel
    {
        private readonly IInterpreter _interpreter = new EnkelInterpreter();
        private readonly ILogger _logger;

        public Enkel(ILogger logger)
        {
            _logger = logger;
        }

        public void RunPrompt()
        {
            Console.CancelKeyPress += ConsoleOnCancelKeyPress;

            while (true)
            {
                Console.Write("> ");

                var line = Console.In.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                try
                {
                    Run(line);
                }
                catch (EnkelException ex)
                {
                    _logger.LogWarning(ex.Message);
                }
            }
        }

        public void RunFile(string path)
        {
            var fileName = Path.GetFileName(path);
            if (!File.Exists(path))
            {
                _logger.LogCritical($"Can't find file '{fileName}' script in path");
                return;
            }

            try
            {
                _logger.LogInfo($"Running script: {fileName}");

                var source = File.ReadAllLines(path);
                Run(source);
            }
            catch (EnkelException ex)
            {
                _logger.LogWarning(ex.Message);
            }
        }

        private void ConsoleOnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("\nUser interrupt exiting...");
        }

        private void Run(params string[] sourceCode)
        {
            var scanner = new EnkelLexer(sourceCode);
            var tokens = scanner.Tokens();

            var parser = new EnkelParser(tokens);
            var statements = parser.Parse();

            _interpreter.Interpret(statements);
        }
    }
}