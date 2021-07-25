using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Shin.Framework.Extensions;
using Shin.Framework.Threading;

namespace Shin.Framework.Runtime
{
    public class SafeMemoryBuffer : ConcurrentSafeBuffer, IEquatable<SafeMemoryBuffer>
    {
        /// <inheritdoc />
        public SafeMemoryBuffer(bool ownsHandle = false) : base(ownsHandle)
        {

        }

        public SafeMemoryBuffer(int length) : this(true)
        {
            Throw.If(length <= 0).ArgumentOutOfRangeException(nameof(length));

            SetHandle(Marshal.AllocHGlobal(length));
            Initialize((ulong)length);
        }

        public SafeMemoryBuffer(IntPtr pointer, int length) : this()
        {
            Throw.If(pointer == IntPtr.Zero).ArgumentException(nameof(pointer));
            Throw.If(length <= 0).ArgumentOutOfRangeException(nameof(length));

            SetHandle(pointer);
            Initialize((ulong)length);
        }

        /// <inheritdoc />
        protected override bool ReleaseHandle()
        {
            if (!IsInvalid)
            {
                Marshal.FreeHGlobal(handle);
                handle = IntPtr.Zero;
                SetHandleAsInvalid();
            }

            return base.ReleaseHandle();
        }

        public virtual void Resize(int length)
        {
            Throw.If(length <= 0).ArgumentOutOfRangeException(nameof(length));
            Throw.If(IsInvalid).InvalidOperationException();

            if (!m_ownsHandle)
                return;

            m_lock.TryEnter(SynchronizationAccess.Write);
            try
            {
                var pointer = Marshal.ReAllocHGlobal(handle, (IntPtr)length);
                if (pointer != handle)
                {
                    Marshal.FreeHGlobal(handle);
                    SetHandle(pointer);
                }
            }
            finally
            {
                m_lock.TryExit(SynchronizationAccess.Write);
            }

            Initialize((ulong)length);

        }

        /// <inheritdoc />
        public bool Equals(SafeMemoryBuffer other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return handle == other.handle && ByteLength == other.ByteLength;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is SafeMemoryBuffer other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return handle.GetHashCode();
        }

        public static bool operator ==(SafeMemoryBuffer left, SafeMemoryBuffer right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SafeMemoryBuffer left, SafeMemoryBuffer right)
        {
            return !Equals(left, right);
        }
    }
}
