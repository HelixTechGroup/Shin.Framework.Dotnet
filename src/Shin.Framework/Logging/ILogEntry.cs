﻿#region Usings
#endregion

#region Usings
using System;
#endregion

namespace Shin.Framework.Logging
{
    public interface ILogEntry : IDispose
    {
        #region Properties
        LogLevel Level { get; }
        Guid Id { get; }

        string LogDate { get; }

        string LogTime { get; }

        string Message { get; set; }

        LogPriority Priority { get; }
        #endregion
    }
}