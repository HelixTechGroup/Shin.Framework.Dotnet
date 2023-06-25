#region Usings
using System;
using System.Threading;
using Shin.Framework.Extensions;
#endregion

namespace Shin.Framework.Threading.Native
{
    public interface ISynchonizableReaderWriter : ISynchronizeLockReader,
                                                  ISynchronizeLockWriter,
                                                  ISynchronizeLockUpgrader { }

    public sealed class ShinLockSlim :
        SynchronizableLock,
        ISynchonizableReaderWriter
    {
#region Members
        const uint MaxReaders = ReaderMask;
        const int MaxSpins = 20;

        const uint ReaderMask = 0x3FFFFFFF;

        const uint Writer = 0x80000000;

        const uint WriterWaiting = 0x40000000;

        readonly FastSemaphore readWait = new FastSemaphore(),
                               writeWait = new FastSemaphore();

        bool hasWaiter;
        //private bool m_isReadLockHeld;
        //private bool m_isUpgradeableLockHeld;
        //private bool m_isWriteLockHeld;

        uint readersWaiting,
             writersWaiting;

        int spinLock;

        // high bit is set if a writer owns the lock. the next bit is set if a writer is waiting. the low 30 bits are the number of readers
        uint state;
#endregion

#region Properties
        /// <inheritdoc />
        public bool IsReadLockHeld
        {
            get { return (state & ReaderMask) != 0; }
        }

        ///// <inheritdoc />
        //public bool IsUpgradeableLockHeld
        //{
        //    get { return m_isUpgradeableLockHeld; }
        //}

        /// <inheritdoc />
        public bool IsWriteLockHeld
        {
            get { return (state & Writer) != 0; }
        }
#endregion

#region Methods
        public void Downgrade()
        {
            if ((state & Writer) == 0) throw new SynchronizationLockException();

            EnterLock(ref spinLock);
            state    -= Writer - 1;
            spinLock =  0;
            Thread.EndCriticalRegion();
        }

        public ISynchronizeContext Create() { return new ShinLockSlimContext(this, SynchronizationAccess.Read); }

        public void EnterRead()
        {
            int spinCount = 0;
            while (true)
            {
                EnterLock(ref spinLock);
                if (state < ReaderMask)
                {
                    state++;
                    spinLock = 0;
                    return;
                }

                if (spinCount < MaxSpins)
                {
                    spinLock = 0;
                    SpinWait(spinCount++);
                }
                else
                {
                    readersWaiting++;
                    hasWaiter = true;
                    spinLock  = 0;
                    spinCount = 0;
                    readWait.Wait();
                }
            }
        }

        public void EnterWrite()
        {
            Thread.BeginCriticalRegion();
            EnterWriteCore(false);
        }

        public void ExitRead()
        {
            if ((state & ReaderMask) == 0) throw new SynchronizationLockException();

            EnterLock(ref spinLock);
            state--;
            ReleaseWaitingThreadsIfAny();
        }

        public void ExitWrite()
        {
            if ((state & Writer) == 0) throw new SynchronizationLockException();

            EnterLock(ref spinLock);
            state &= ~Writer;
            ReleaseWaitingThreadsIfAny();
            Thread.EndCriticalRegion();
        }

        public bool Upgrade()
        {
            if ((state & ReaderMask) == 0) throw new SynchronizationLockException();

            Thread.BeginCriticalRegion();

            int spinCount = 0;
            bool reserved = false;
            while (true)
            {
                EnterLock(ref spinLock);
                if ((state & ReaderMask) == 1) // if we're the only reader...
                {
                    state += Writer - 1; // convert the lock to be owned by us in write mode
                    if (reserved && writersWaiting == 0) state &= ~WriterWaiting;

                    spinLock = 0;
                    return true;
                }

                if (reserved)
                {
                    spinLock = 0;
                    if (spinCount < 100)
                        ThreadUtility.BriefWait();
                    else
                        ThreadUtility.Spin(spinCount++);
                }
                else if ((state & WriterWaiting) == 0)
                {
                    state    |= WriterWaiting;
                    spinLock =  0;
                    reserved =  true;
                    ThreadUtility.BriefWait();
                }
                else
                {
                    writersWaiting++;
                    state     = (state - 1) | WriterWaiting;
                    hasWaiter = true;
                    spinLock  = 0;
                    writeWait.Wait();
                    EnterWriteCore(true); // we're a writer now, so do the normal loop to enter write mode
                    return false;         // return false because we may not have been the first writer to take the lock
                }
            }
        }

        //// <inheritdoc />
        //public bool TryEnter()
        //{
        //    return TryEnter(SynchronizationAccess.Read);
        //}

        ///// <inheritdoc />
        //public bool TryExit()
        //{
        //    throw new NotImplementedException();
        //}

        /// <inheritdoc />
        protected override bool TryEnterLock(SynchronizationAccess access = SynchronizationAccess.Read,
                                             int maxRetries = 3,
                                             int retryDelay = 50,
                                             int lockTimeout = 50)
        {
            switch (access)
            {
                case SynchronizationAccess.Read:
                    EnterRead();
                    break;
                case SynchronizationAccess.Write:
                    EnterWrite();
                    break;
            }
            
            return true;
        }

        /// <inheritdoc />
        protected override bool TryExitLock(SynchronizationAccess access = SynchronizationAccess.Read,
                                            int maxRetries = 3,
                                            int retryDelay = 50,
                                            int lockTimeout = 50)
        {
            switch (access)
            {
                case SynchronizationAccess.Read:
                    ExitRead();
                    break;
                case SynchronizationAccess.Write:
                    ExitWrite();
                    break;
            }
            return true;
        }

        void EnterWriteCore(bool clearWaitBit)
        {
            int spinCount = 0;
            EnterLock(ref spinLock);
            if (clearWaitBit && writersWaiting == 0) state &= ~WriterWaiting;

            while (true)
            {
                if ((state & ~WriterWaiting) == 0) // if the lock is unowned
                {
                    state    |= Writer; // take it for ourselves
                    spinLock =  0;
                    return;
                }

                if (spinCount < MaxSpins)
                {
                    spinLock = 0;
                    SpinWait(++spinCount);
                    EnterLock(ref spinLock);
                }
                else
                {
                    writersWaiting++;
                    state     |= WriterWaiting;
                    hasWaiter =  true;
                    spinLock  =  0;
                    spinCount =  0;
                    writeWait.Wait();
                    EnterLock(ref spinLock);
                    if (writersWaiting == 0) state &= ~WriterWaiting;
                }
            }
        }

        void ReleaseWaitingThreads()
        {
            if (writersWaiting != 0)
            {
                if ((state & ReaderMask) == 0)
                {
                    if (--writersWaiting == 0 &&
                        readersWaiting == 0)
                        hasWaiter = false;

                    spinLock = 0;
                    writeWait.Release();
                    return;
                }
            }
            else if (readersWaiting != 0) // otherwise, if any readers were waiting, release all of them
            {
                uint count = readersWaiting;
                hasWaiter      = false;
                readersWaiting = 0;
                spinLock       = 0;
                readWait.Release(count);
                return;
            }

            spinLock = 0;
        }

        void ReleaseWaitingThreadsIfAny()
        {
            if (hasWaiter)
                ReleaseWaitingThreads();
            else
                spinLock = 0;
        }

        static void EnterLock(ref int spinLock)
        {
            if (Interlocked.CompareExchange(ref spinLock, 1, 0) != 0) EnterLockSpin(ref spinLock);
        }

        static void EnterLockSpin(ref int spinLock)
        {
            int spinCount = 0;
            do
            {
                spinCount++;
                if (spinCount <= 10 &&
                    ThreadUtility.MultiProcessor)
                    Thread.SpinWait(20 * spinCount);
                else if (spinCount <= 15)
                    Thread.Sleep(0);
                else
                    Thread.Sleep(1);
            } while (Interlocked.CompareExchange(ref spinLock, 1, 0) != 0);
        }

        static void SpinWait(int spinCount)
        {
            if (spinCount < 5 &&
                Environment.ProcessorCount > 1)
                Thread.SpinWait(20 * spinCount);
            else if (spinCount < MaxSpins - 3)
                Thread.Sleep(0);
            else
                Thread.Sleep(1);
        }
#endregion
    }

    public struct ShinLockSlimContext : ISynchronizeContext
    {
#region Events
        /// <inheritdoc />
        public event EventHandler Disposed;

        /// <inheritdoc />
        public event EventHandler Disposing;
#endregion

#region Members
        private readonly bool m_isSynchronized;
        private SynchronizationAccess m_accessLevel;
        private bool m_isDisposed;

        ShinLockSlim m_owner;
#endregion

#region Properties
        /// <inheritdoc />
        public SynchronizationAccess AccessLevel
        {
            get { return m_accessLevel; }
        }

        public bool Exclusive
        {
            get { return m_accessLevel == SynchronizationAccess.Write; }
        }

        /// <inheritdoc />
        public bool IsDisposed
        {
            get { return m_isDisposed; }
        }

        /// <inheritdoc />
        public bool IsSynchronized
        {
            get { return m_isSynchronized; }
        }
#endregion

        internal ShinLockSlimContext(ShinLockSlim owner,
                                     SynchronizationAccess access)
        {
            m_owner          = owner;
            m_accessLevel    = access;
            m_isDisposed     = false;
            m_isSynchronized = false;

            Disposed  = null;
            Disposing = null;
        }

#region Methods
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Downgrade()
        {
            AssertNotDisposed();
            if (m_accessLevel == SynchronizationAccess.Read) throw new SynchronizationLockException();

            m_owner.Downgrade();
            m_accessLevel = SynchronizationAccess.Read;
        }

        public bool Upgrade()
        {
            AssertNotDisposed();
            if (m_accessLevel == SynchronizationAccess.Write) throw new SynchronizationLockException();

            bool firstUpgrader = m_owner.Upgrade();
            m_accessLevel = SynchronizationAccess.Write;
            return firstUpgrader;
        }

        private void Dispose(bool disposing)
        {
            if (m_isDisposed) return;

            Disposing.Raise(this, EventArgs.Empty);
            if (disposing)
            {
                if (m_owner != null)
                {
                    if (m_accessLevel == SynchronizationAccess.Write)
                        m_owner.ExitWrite();
                    else
                        m_owner.ExitRead();

                    m_owner = null;
                }
            }

            Disposed.Raise(this, EventArgs.Empty);

            Disposing.Dispose();
            Disposed.Dispose();
            m_isDisposed = true;
        }

        void AssertNotDisposed()
        {
            if (m_owner == null)
            {
                throw new ObjectDisposedException(GetType()
                                                     .FullName);
            }
        }
#endregion
    }

    sealed class FastSemaphore
    {
#region Members
        uint count;
#endregion

#region Methods
        public void Release()
        {
            lock(this)
            {
                if (count == uint.MaxValue) throw new InvalidOperationException();

                count++;
                Monitor.Pulse(this);
            }
        }

        public void Release(uint count)
        {
            if (count != 0)
            {
                lock(this)
                {
                    this.count += count;
                    if (this.count < count) // if it overflowed, undo the addition and throw an exception
                    {
                        this.count -= count;
                        throw new InvalidOperationException();
                    }

                    if (count == 1)
                        Monitor.Pulse(this);
                    else
                        Monitor.PulseAll(this);
                }
            }
        }

        public void Wait()
        {
            lock(this)
            {
                while (count == 0) Monitor.Wait(this);

                count--;
            }
        }
#endregion
    }

    internal static class ThreadUtility
    {
#region Members
        public static bool MultiProcessor = Environment.ProcessorCount > 1;

        private static int dummy;
#endregion

#region Methods
        public static void BriefWait()
        {
            if (MultiProcessor)
                Thread.SpinWait(1);
            else
                Thread.Yield();

            //Thread.Sleep(0); // or use Thread.Yield() in .NET 4
            dummy++;
        }

        public static void Spin(int spinCount)
        {
            if (spinCount < 10 && MultiProcessor)
                Thread.SpinWait(20 * (spinCount + 1));
            else if (spinCount < 15)
                Thread.Sleep(0); // or use Thread.Yield() in .NET 4
            else
                Thread.Sleep(1);

            dummy++;
        }
#endregion
    }
}