using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Shin.Framework.Runtime
{
    public class SafeMemoryHandle : ConcurrentSafeHandle
    {
        /// <inheritdoc />
        public SafeMemoryHandle(bool ownsHandle = true) : base(ownsHandle) { }

        public SafeMemoryHandle(IntPtr pointer, bool ownsHandle = true) : base(ownsHandle)
        {
            SetHandle(pointer);
        }

        /// <inheritdoc />
        protected override bool ReleaseHandle()
        {
            if (!IsInvalid)
                SetHandleAsInvalid();

            return base.ReleaseHandle();
        }
    }
}
