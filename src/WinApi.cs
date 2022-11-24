//https://stackoverflow.com/a/71457/13250396

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DeathNote;

public static class WinApi
{
    [Flags]
    private enum ThreadAccess : int
    {
        SUSPEND_RESUME = 0x0002
    }

    [DllImport("kernel32.dll")]
    private static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

    [DllImport("kernel32.dll")]
    private static extern uint SuspendThread(IntPtr hThread);

    [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool CloseHandle(IntPtr handle);

    public static void SuspendProcess(int pid)
    {
        var process = Process.GetProcessById(pid); // throws exception if process does not exist

        foreach (ProcessThread pT in process.Threads)
        {
            var pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint) pT.Id);

            if (pOpenThread == IntPtr.Zero)
            {
                continue;
            }

            SuspendThread(pOpenThread);
            CloseHandle(pOpenThread);
        }
    }
}