using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Shin.Framework.Extensions
{
    public static class SafeHandleExtensions
    {
        public static IntPtr Lock(this SafeHandle handle, ref object lockObject)
        {
            try
            {
                Monitor.Enter(lockObject);
                return handle.DangerousGetHandle();
            }
            finally
            {
                Monitor.Exit(lockObject);
            }
        }

        public static IntPtr Unlock(this SafeHandle handle, ref object lockObject)
        {
            try
            {
                Monitor.Enter(lockObject);
                return handle.DangerousGetHandle();
            }
            finally
            {
                Monitor.Exit(lockObject);
            }
        }
    }
}
