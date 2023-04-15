using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO.Pipes;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NbDevTools
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Win32 constants
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int VK_LWIN = 0x5B;
        private const int VK_RWIN = 0x5C;
        private const int VK_RETURN = 0x0D;

        // Win32 functions
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern short GetKeyState(int nVirtKey); // Note that GetKeyState returns a ushort, so we need to use the bitwise AND operator (&) to check whether the high-order bit (bit 15) is set. If the bit is set, the key is down; otherwise, it is up

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        // Delegate for keyboard hook procedure
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        // Keyboard hook handle
        private static IntPtr _hookHandle;

        private static IntPtr KeyboardHookProcedure(int nCode, IntPtr wParam, IntPtr lParam)
        {
            Debug.WriteLine("hook");

            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                bool windowsKey = (GetKeyState(VK_LWIN) & 0x8000) != 0 || (GetKeyState(VK_RWIN) & 0x8000) != 0;
                if (windowsKey && vkCode == VK_RETURN)
                {
                    // Windows key + Enter was pressed, send message to WPF app
                    using (var client = new NamedPipeClientStream(".", "MyAppPipe", PipeDirection.Out))
                    {
                        client.Connect();
                        using (var writer = new StreamWriter(client))
                        {
                            writer.Write("BringToFront");
                        }
                    }
                }
            }
            return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        public App()
        {
            // Start named pipe server
            StartNamedPipeServer();

            // Set up keyboard hook
            var hookProc = new LowLevelKeyboardProc(KeyboardHookProcedure);
            using (var process = Process.GetCurrentProcess())
            using (var module = process.MainModule)
            {
                _hookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, hookProc, GetModuleHandle(module.ModuleName), 0);
            }
        }

        ~App()
        {
            // Unhook keyboard hook
            UnhookWindowsHookEx(_hookHandle);
        }

        private static void StartNamedPipeServer()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    using (var server = new NamedPipeServerStream("MyAppPipe", PipeDirection.In))
                    {
                        server.WaitForConnection();
                        using (var reader = new StreamReader(server))
                        {
                            string message = reader.ReadToEnd();
                            if (message == "BringToFront")
                            {
                                // Handle "BringToFront" message here
                                Debug.WriteLine("Bring to front");
                            }
                        }
                    }
                }
            });
        }

    }
}
