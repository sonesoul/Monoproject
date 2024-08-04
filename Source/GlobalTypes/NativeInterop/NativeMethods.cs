using System;
using System.Runtime.InteropServices;

namespace GlobalTypes.NativeInterop
{
    /// <summary>
    /// Contains P/Invoke methods for interacting with native Windows API functions.
    /// </summary>
    internal class NativeMethods
    {
        #region Kernel32
        [DllImport("kernel32.dll")]
        public static extern bool AllocConsole();
        [DllImport("kernel32.dll")]
        public static extern bool FreeConsole();
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetStdHandle(uint nStdHandle);
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetConsoleWindow();
        #endregion

        #region User32
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        #endregion
    }
}