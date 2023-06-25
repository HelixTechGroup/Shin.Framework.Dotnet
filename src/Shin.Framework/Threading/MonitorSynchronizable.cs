using System;
using System.Collections.Generic;
using System.Text;

namespace Shin.Framework.Threading
{
    public class MonitorSynchronizable : SynchronizableLock
    {
        /// <inheritdoc />
        protected override bool TryEnterLock(SynchronizationAccess access = SynchronizationAccess.Read, int maxRetries = 3, int retryDelay = 50, int lockTimeout = 50)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        protected override bool TryExitLock(SynchronizationAccess access = SynchronizationAccess.Read, int maxRetries = 3, int retryDelay = 50, int lockTimeout = 50)
        {
            throw new NotImplementedException();
        }
    }
}
