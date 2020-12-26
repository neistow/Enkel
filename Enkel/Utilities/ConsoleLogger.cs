using System;

namespace Enkel.Utilities
{
    public class ConsoleLogger : ILogger
    {
        public void LogDebug(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            LogAndReset(message);
        }
        
        public void LogInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            LogAndReset(message);
        }

        public void LogWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            LogAndReset(message);
        }

        public void LogCritical(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            LogAndReset(message);
        }

        private void LogAndReset(string message)
        {
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}