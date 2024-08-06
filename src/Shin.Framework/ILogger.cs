#region Usings
using System;
using Shin.Logging;
#endregion

namespace Shin
{
    public interface ILogger : IInitialize, IDisposable
    {
        #region Methods
        void AddLogProvider(ILogProvider logProvider);

        void LogNone(string message);

        void LogInfo(string message);

        void LogWarn(string message);

        void LogError(string message);

        void LogDebug(string message);

        void LogException(Exception exception);

        void Log(string message, LogLevel category, LogPriority priority);

        void Log(ILogEntry entry);
        #endregion
    }
}