using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Shin.Framework.Runtime
{
    public class SafeMemoryHandle : ConcurrentSafeHandle, IEquatable<SafeMemoryHandle>
    {
        /// <inheritdoc />
        public SafeMemoryHandle(bool ownsHandle = false) : base(ownsHandle) { }

        public SafeMemoryHandle(IntPtr pointer) : base(true)
        {
            SetHandle(pointer);
        }

        /// <inheritdoc />
        protected override bool ReleaseHandle()
        {
            if (handle != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(handle);
                handle = IntPtr.Zero;
            }

            return base.ReleaseHandle();
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (!IsInvalid)
                SetHandleAsInvalid();

            base.Dispose(disposing);
        }

        /// <inheritdoc />
        public bool Equals(SafeMemoryHandle other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return handle == other.handle;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is SafeMemoryHandle other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return handle.GetHashCode();
        }

        public static bool operator ==(SafeMemoryHandle left, SafeMemoryHandle right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SafeMemoryHandle left, SafeMemoryHandle right)
        {
            return !Equals(left, right);
        }
    }
}
