namespace Enkel.Utilities
{
    public interface ILogger
    {
        public void LogDebug(string message);
        void LogInfo(string message);
        void LogWarning(string message);
        void LogCritical(string message);
    }
}