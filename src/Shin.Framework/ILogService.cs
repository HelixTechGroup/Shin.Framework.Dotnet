#region Usings
#endregion

#region Usings
using System;
using Shin.Framework.Logging;
#endregion

namespace Shin.Framework
{
    public interface ILogService : IApplicationService
    {
        #region Methods
        void AddLogProvider(ILogProvider logProvider);

        void LogInfo(string message);

        void LogWarn(string message);

        void LogError(string message);

        void LogDebug(string message);

        void LogException(Exception exception);

        void Log(string message, LogCategory category, LogPriority priority);

        void Log(ILogEntry entry);
        #endregion
    }
}