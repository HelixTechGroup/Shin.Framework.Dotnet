using System;
using System.Threading;
using Microsoft.Win32.SafeHandles;
using Shin.Extensions;
using Shin.Threading;

namespace Shin.Runtime
{
    public class ConcurrentSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        protected readonly ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        protected bool m_ownsHandle;
        //protected static readonly object m_lock = new object();
        //protected static readonly Mutex m_mutex = new Mutex(true);
        //protected static readonly SemaphoreSlim m_semaphore = new SemaphoreSlim(0, 1);
        //protected bool m_hasLock;

        /// <inheritdoc />
        public ConcurrentSafeHandle(bool ownsHandle) : base(ownsHandle)
        {
            m_ownsHandle = ownsHandle;
        }

        /// <inheritdoc />
        protected override bool ReleaseHandle()
        {
            if (m_lock.IsLockHeld())
                m_lock.TryExit();

            return m_lock.IsLockHeld();
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            m_lock.Dispose();

            base.Dispose(disposing);
        }

        public ConcurrentSafeHandle Lock()
        {
            m_lock.TryEnter();
            try
            {
                return this;
            }
            finally
            {
                m_lock.TryExit();
            }
        }

        public void Unlock()
        {
            if (!m_lock.IsLockHeld()) 
                return;

            m_lock.TryExit();
        }

        protected new void SetHandle(IntPtr handle)
        {
            m_lock.TryEnter(SynchronizationAccess.Write);
            try
            {
                this.handle = handle;
            }
            finally
            {
                m_lock.TryExit(SynchronizationAccess.Write);
            }
        }

        public new IntPtr DangerousGetHandle()
        {
            m_lock.TryEnter();
            try
            {
                return handle;
            }
            finally
            {
                m_lock.TryExit();
            }
        }
    }
}
