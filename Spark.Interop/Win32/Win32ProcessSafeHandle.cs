﻿using System;
using Microsoft.Win32.SafeHandles;

namespace Spark.Win32
{
    internal sealed class Win32ProcessSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public Win32ProcessSafeHandle(IntPtr handle)
            :this()
        {
            this.SetHandle(handle);
        }
        
        public Win32ProcessSafeHandle()
            : base(true) { }

        #region SafeHandle Methods
        protected override bool ReleaseHandle()
        {
            return NativeMethods.CloseHandle(handle);
        }
        #endregion
    }
}
