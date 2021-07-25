using System.Threading;
using Shin.Framework.Threading;

namespace Shin.Framework.Extensions
{
    public static class LockSlimExtensions
    {
        public static bool TryExit(this ReaderWriterLockSlim lockSlim, SynchronizationAccess access = SynchronizationAccess.Read, int maxRetries = 3, int retryDelay = 50, int lockTimeout = 50)
        {
            //if (lockSlim.IsWriteLockHeld && access == SynchronizationAccess.Write)
            //    Thread.Sleep(retryDelay);


            var locked = true;
            for (var i = 0; i <= maxRetries; i++)
            {
                switch (access)
                {
                    case SynchronizationAccess.Write:
                        if (lockSlim.RecursiveWriteCount > 0)
                            lockSlim.ExitWriteLock();
                        locked = false;
                        break;
                    default:
                        if (lockSlim.RecursiveUpgradeCount > 0)
                            lockSlim.ExitUpgradeableReadLock();

                        //lockSlim.ExitUpgradeableReadLock();
                        locked = false;
                        break;
                }

                if (!locked)
                    break;

                Thread.Sleep(retryDelay);
            }

            return !locked;
        }

        public static bool IsLockHeld(this ReaderWriterLockSlim lockSlim)
        {
            return (lockSlim.IsWriteLockHeld || lockSlim.IsUpgradeableReadLockHeld || lockSlim.IsReadLockHeld);
        }

        public static bool TryEnter(this ReaderWriterLockSlim lockSlim, SynchronizationAccess access = SynchronizationAccess.Read, int maxRetries = 3, int retryDelay = 50, int lockTimeout = 50)
        {
            var locked = false;

            if (lockSlim.IsWriteLockHeld && access == SynchronizationAccess.Write)
                Thread.Sleep(retryDelay);

            //try
            //{
                for (var i = 0; i <= maxRetries; i++)
                {
                    switch (access)
                    {
                        case SynchronizationAccess.Write:
                            locked = lockSlim.TryEnterWriteLock(lockTimeout);
                            break;
                        default:
                            locked = lockSlim.TryEnterUpgradeableReadLock(lockTimeout);
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

        public static bool TryUpgradeLock(this ReaderWriterLockSlim lockSlim, int maxRetries = 3, int retryDelay = 50, int lockTimeout = 50)
        {
            var locked = false;

            if (!lockSlim.IsWriteLockHeld)
                Thread.Sleep(retryDelay);

            for (var i = 0; i <= maxRetries; i++)
            {
                locked = lockSlim.TryEnterWriteLock(lockTimeout);

                if (locked)
                    break;

                Thread.Sleep(retryDelay);
            }

            return locked;
        }
    }
}
