using System.Threading;
using Shin.Framework.Threading;

namespace Shin.Framework.Extensions
{
    public static class LockExtensions
    {
        public static bool TryExit(this ReaderWriterLock rwLock, SynchronizationAccess access = SynchronizationAccess.Read, int maxRetries = 3, int retryDelay = 50, int lockTimeout = 50)
        {
            //if (lockSlim.IsWriteLockHeld && access == SynchronizationAccess.Write)
            //    Thread.Sleep(retryDelay);

            var locked = true;
            for (var i = 0; i <= maxRetries; i++)
            {
                switch (access)
                {
                    case SynchronizationAccess.Write:
                        rwLock.ReleaseWriterLock();
                        locked = rwLock.IsWriterLockHeld;
                        break;
                    default:
                        rwLock.ReleaseReaderLock();
                        locked = rwLock.IsReaderLockHeld;
                        break;
                }

                if (!locked)
                    break;

                Thread.Sleep(retryDelay);
            }

            return !locked;
        }

        //public static bool IsLockHeld(this ReaderWriterLock lockSlim)
        //{
        //    return (lockSlim.IsWriteLockHeld || lockSlim.IsUpgradeableReadLockHeld || lockSlim.IsReadLockHeld);
        //}        //public static bool IsLockHeld(this ReaderWriterLock lockSlim)
        //{
        //    return (lockSlim.IsWriteLockHeld || lockSlim.IsUpgradeableReadLockHeld || lockSlim.IsReadLockHeld);
        //}

        public static bool TryEnter(this ReaderWriterLock rwLock, SynchronizationAccess access = SynchronizationAccess.Read, int maxRetries = 3, int retryDelay = 50, int lockTimeout = 50)
        {
            var locked = false;

            if (rwLock.IsWriterLockHeld && access == SynchronizationAccess.Write)
                Thread.Sleep(retryDelay);

            //try
            //{
                for (var i = 0; i <= maxRetries; i++)
                {
                    switch (access)
                    {
                        case SynchronizationAccess.Write:
                            rwLock.AcquireWriterLock(lockTimeout);
                            locked = rwLock.IsWriterLockHeld;
                            break;
                        default:
                            rwLock.AcquireReaderLock(lockTimeout);
                            locked = rwLock.IsReaderLockHeld;
                            break;
                    }

                    if (locked)
                        break;

                    Thread.Sleep(retryDelay);
                }
            //}
            //catch
            //{
            //    if (lockSlim.IsWriteLockHeld)
            //        lockSlim.ExitWriteLock();
            //    else if (lockSlim.IsUpgradeableReadLockHeld)
            //        lockSlim.ExitUpgradeableReadLock();

            //    throw;
            //}

            return locked;
        }

        public static bool TryUpgradeLock(this ReaderWriterLock lockSlim, int maxRetries = 3, int retryDelay = 50, int lockTimeout = 50)
        {
            var locked = false;

            if (!lockSlim.IsWriterLockHeld)
                Thread.Sleep(retryDelay);

            for (var i = 0; i <= maxRetries; i++)
            {
                lockSlim.UpgradeToWriterLock(lockTimeout);
                locked = lockSlim.IsWriterLockHeld;

                if (locked)
                    break;

                Thread.Sleep(retryDelay);
            }

            return locked;
        }
    }
}
