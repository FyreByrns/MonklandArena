using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.IO;

namespace MonkArena {
    public static class RWConsole {
        public static StreamWriter Output;

        public static void Initialize() {
            AllocConsole();
            IntPtr stdHandle = GetStdHandle(STD_OUTPUT_HANDLE);
            FileStream fileStream = new FileStream(stdHandle, FileAccess.Write);
            Output = new StreamWriter(fileStream, Encoding.ASCII) {
                AutoFlush = true,
            };
        }

        public static void Log(object message, string prefix = "INFO") {
            Output.WriteLine($"[{prefix}][{DateTime.UtcNow}] {message}");
        }
        public static void LogError(object message) {
            Log(message, "ERROR");
        }
        public static void LogInfo(object message) {
            Log(message, "INFO");
        }

        #region Interop
        [DllImport(
            "kernel32.dll",
            EntryPoint = "GetStdHandle",
            SetLastError = true,
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr GetStdHandle(int nStdHandle);
        [DllImport(
            "kernel32.dll",
            EntryPoint = "AllocConsole",
            SetLastError = true,
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        private static extern int AllocConsole();
        private const int STD_OUTPUT_HANDLE = -11;
        #endregion
    }
}
