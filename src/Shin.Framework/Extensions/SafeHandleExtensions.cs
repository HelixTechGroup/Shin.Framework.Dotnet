using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Shin.Extensions
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
