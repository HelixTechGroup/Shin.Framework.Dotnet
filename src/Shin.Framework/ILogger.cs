#region Usings
#endregion

#region Usings
using System;
using Shin.Framework.Logging;
#endregion

namespace Shin.Framework
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