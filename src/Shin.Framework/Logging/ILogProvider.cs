﻿#region Usings
#endregion

namespace Shin.Framework.Logging
{
    public interface ILogProvider : IDispose
    {
        bool IsBuffering { get; }

        #region Methods
        void Flush(ILogEntry entry);
        #endregion
    }
}