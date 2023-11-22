using System;
using Microsoft.Win32.SafeHandles;

namespace Spark.Win32
{
    internal sealed class Win32ThreadSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public Win32ThreadSafeHandle()
            : base(true) { }

        public Win32ThreadSafeHandle(IntPtr handle)
            : this()
        {
            this.SetHandle(handle);
        }

        #region SafeHandle Methods
        protected override bool ReleaseHandle()
        {
            return NativeMethods.CloseHandle(handle);
        }
        #endregion
    }
}
