using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace Shin.Framework.Runtime
{
    public class ConcurrentSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        protected readonly ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        //protected static readonly object m_lock = new object();
        //protected static readonly Mutex m_mutex = new Mutex(true);
        //protected static readonly SemaphoreSlim m_semaphore = new SemaphoreSlim(0, 1);
        protected bool m_hasLock;

        /// <inheritdoc />
        public ConcurrentSafeHandle(bool ownsHandle) : base(ownsHandle)
        {
            //m_mutex = new Mutex(true);
        }

        /// <inheritdoc />
        protected override bool ReleaseHandle()
        {
            m_lock.Dispose();
            //m_semaphore.Release();
            //if (m_hasLock)
            //m_mutex.ReleaseMutex();
                //Monitor.Exit(m_lock);

            return m_hasLock;
        }

        protected bool TryLock(int maxRetries = 3, int retryDelay = 50, int lockTimeout = 250)
        {
            for (var i = 0; i <= maxRetries; i++)
            {
                if (m_hasLock)
                    Thread.Sleep(retryDelay);

                m_hasLock = m_lock.TryEnterReadLock(lockTimeout);
                //m_hasLock = true;
                //m_semaphore.Wait();
                if (!m_hasLock)
                    Thread.Sleep(retryDelay);
                else
                    return true;

                //if (m_hasLock)
                //    return true;
                //    return null;
            }

            return false;
        }

        public ConcurrentSafeHandle Lock()
        {
            TryLock();
            //Monitor.Enter(m_lock);
            //lock (m_lock)
            //{
                try
                {
                    //Monitor.Enter(m_lock, ref m_hasLock);
                    //m_wait = m_semaphore.AvailableWaitHandle;
                    //m_hasLock = m_mutex.WaitOne();
                    //m_hasLock = m_semaphore.AvailableWaitHandle.WaitOne();
                    return this;
                }
                catch
                {
                    //Monitor.Exit(m_lock);
                    //m_mutex.ReleaseMutex();
                    
                    m_lock.ExitReadLock();
                    m_hasLock = false;
                    throw;
                }
                finally
                {
                    //Monitor.Exit(m_lock);
                }
            //}
        }

        public void Unlock()
        {
            if (!m_hasLock) 
                return;

            m_lock.ExitReadLock();
            //lock (m_lock)
            //{
                //Monitor.Wait(m_lock);
                //Monitor.Exit(m_lock);
                //m_mutex.ReleaseMutex();
                //m_semaphore.Release();
                m_hasLock = false;
            //}
        }
    }
}
