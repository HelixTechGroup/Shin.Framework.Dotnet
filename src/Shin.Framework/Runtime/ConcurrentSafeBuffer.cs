using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Shin.Framework.Extensions;
using Shin.Framework.Threading;

namespace Shin.Framework.Runtime
{
    public class ConcurrentSafeBuffer : SafeBuffer
    {
        protected readonly ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        protected bool m_ownsHandle;

        public ConcurrentSafeBuffer(bool ownsHandle) : base(ownsHandle)
        {
            m_ownsHandle = ownsHandle;
        }

        public new void Initialize(ulong numBytes)
        {
            m_lock.TryEnter();
            try
            {
                base.Initialize(numBytes);
            }
            finally
            {
                m_lock.TryExit();
            }
        }

        public new void Initialize<T>(uint numElements) where T : struct
        {
            m_lock.TryEnter();
            try
            {
                base.Initialize<T>(numElements);
            }
            finally
            {
                m_lock.TryExit();
            }
        }

        public new unsafe void AcquirePointer(ref byte* pointer)
        {
            m_lock.TryEnter();
            try
            {
                base.AcquirePointer(ref pointer);
            }
            finally
            {
                m_lock.TryExit();
            }
        }

        public new void ReleasePointer()
        {
            m_lock.TryEnter();
            try
            {
                base.ReleasePointer();
            }
            finally
            {
                m_lock.TryExit();
            }
        }

        public new T Read<T>(ulong byteOffset) where T : struct
        {
            m_lock.TryEnter();
            try
            {
                return base.Read<T>(byteOffset);
            }
            finally
            {
                m_lock.TryExit();
            }
        }

        public new void ReadArray<T>(ulong byteOffset, T[] array, int index, int count)
            where T : struct
        {
            m_lock.TryEnter();
            try
            {
                base.ReadArray(byteOffset, array, index, count);
            }
            finally
            {
                m_lock.TryExit();
            }
        }

        public new void Write<T>(ulong byteOffset, T value) where T : struct
        {
            m_lock.TryEnter();
            try
            {
                base.Write(byteOffset, value);
            }
            finally
            {
                m_lock.TryExit();
            }
        }

        public new void WriteArray<T>(ulong byteOffset, T[] array, int index, int count)
            where T : struct
        {
            m_lock.TryEnter();
            try
            {
                base.WriteArray(byteOffset, array, index, count);
            }
            finally
            {
                m_lock.TryExit();
            }
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

        public ConcurrentSafeBuffer Lock()
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
