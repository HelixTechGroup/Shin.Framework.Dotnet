using System;
using System.Threading;
using Shin.Framework.Threading;

namespace Shin.Framework.Extensions
{
    public static class LockSlimExtensions
    {
        public static bool TryExitAll(this ReaderWriterLockSlim lockSlim)
        {
            while (lockSlim.RecursiveWriteCount > 0)
                lockSlim.ExitWriteLock();

            while (lockSlim.RecursiveReadCount > 0)
                lockSlim.ExitReadLock();

            while (lockSlim.RecursiveUpgradeCount > 0)
                lockSlim.ExitUpgradeableReadLock();

            return true;
        }

        public static bool TryExit(this ReaderWriterLockSlim lockSlim, SynchronizationAccess access = SynchronizationAccess.Read, int maxRetries = 3, int retryDelay = 50, int lockTimeout = 50)
        {
            //if (lockSlim.IsWriteLockHeld && access == SynchronizationAccess.Write)
            //    Thread.Sleep(retryDelay);

            if (!lockSlim.IsLockHeld())
                return false;

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
                    case SynchronizationAccess.Read:
                        if (lockSlim.RecursiveReadCount > 0)
                            lockSlim.ExitReadLock();

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

            if (lockSlim.IsLockHeld() && access == SynchronizationAccess.Write)
                Thread.Sleep(retryDelay);

            //try
            //{
                for (var i = 0; i <= maxRetries; i++)
                {
                    switch (access)
                    {
                        case SynchronizationAccess.Write:
                            try
                            {
                                if (lockSlim.IsReadLockHeld)
                                {
                                    lockSlim.ExitReadLock();
                                }
                                //    break;

                                if (lockSlim.IsUpgradeableReadLockHeld)
                                {
                                    locked = lockSlim.TryUpgradeLock();
                                    break;
                                }

                                locked = lockSlim.TryEnterWriteLock(lockTimeout);
                            }
                            catch (LockRecursionException lrEx)
                            {
                                if (lockSlim.IsUpgradeableReadLockHeld)
                                    locked = lockSlim.TryUpgradeLock();
                            }
                            break;
                        case SynchronizationAccess.Read:
                            locked = lockSlim.TryEnterReadLock(lockTimeout);
                            break;
                        default:
                            //if (lockSlim.IsReadLockHeld)
                            //    locked = lockSlim.TryEnterReadLock(lockTimeout);

                            //if (!locked)
                            //    locked = lockSlim.TryEnterUpgradeableReadLock(lockTimeout);

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
            //        lockSlim.TryExit(SynchronizationAccess.Write);
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
