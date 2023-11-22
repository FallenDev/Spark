using System;
using System.IO;
using System.Runtime.InteropServices;

using Spark.Win32;

namespace Spark.Interop;

public sealed class SuspendedProcess : IDisposable
{
    private bool isDisposed;
    private bool resumeOnDispose;
    private Win32ProcessSafeHandle processHandle;
    private Win32ThreadSafeHandle threadHandle;

    #region Properties
    public int ProcessId { get; private set; }

    public int ThreadId { get; private set; }

    public bool IsSuspended { get; private set; }
    #endregion

    private SuspendedProcess(Win32ProcessInformation processInformation, bool resumeOnDispose)
    {
        IsSuspended = true;    // Suspended by default

        processHandle = new Win32ProcessSafeHandle(processInformation.ProcessHandle);
        threadHandle = new Win32ThreadSafeHandle(processInformation.ThreadHandle);

        ProcessId = processInformation.ProcessId;
        ThreadId = processInformation.ThreadId;

        this.resumeOnDispose = resumeOnDispose;
    }

    ~SuspendedProcess()
    {
        Dispose(false);
    }

    public static SuspendedProcess Start(string applicationPath, string commandLine = null, bool resumeOnDispose = true)
    {
        // Create the startup info and set the Size parameter to the size of the structure
        Win32StartupInfo startupInfo = new Win32StartupInfo();
        startupInfo.Size = Marshal.SizeOf(typeof(Win32StartupInfo));

        Win32ProcessInformation processInformation;

        // Attempt to create the process in a suspended state
        var didCreate = NativeMethods.CreateProcess(applicationPath,
            commandLine,
            IntPtr.Zero,    // NULL
            IntPtr.Zero,    // NULL
            false,
            Win32ProcessCreationFlags.Suspended,
            IntPtr.Zero,    // NULL
            null,
            ref startupInfo,
            out processInformation);

        // Check if the process was created
        if (!didCreate)
            throw new IOException("Unable to create process");

        return new SuspendedProcess(processInformation, resumeOnDispose);
    }

    public void Resume()
    {
        CheckIfDisposed();
        ResumeProcess();            
    }

    private void ResumeProcess()
    {
        while (NativeMethods.ResumeThread(threadHandle) > 1)
        {
            // ResumeThread returns the PREVIOUS suspension count:
            // -> If it is ZERO, the thread was not suspended.
            // -> If it is ONE, the thread was suspended and now has been resumed.
            // -> If it is GREATER THAN ONE, the thread is still suspended.
        }

        IsSuspended = false;
    }

    #region IDisposable Methods
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool isDisposing)
    {
        if (isDisposed)
            return;

        if (isDisposing)
        {
            // Dispose of managed resources here
        }

        // Dispose of unmanaged resources here
        if (resumeOnDispose)
            ResumeProcess();

        threadHandle.Dispose();
        processHandle.Dispose();
            
        isDisposed = true;
    }

    private void CheckIfDisposed()
    {
        if (isDisposed)
            throw new ObjectDisposedException(GetType().Name);
    }
    #endregion
}