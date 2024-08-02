using Microsoft.Win32.SafeHandles;
using Microsoft.Xna.Framework.Input;
using Monoproject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalTypes
{
    static class GameConsole
    {
        [DllImport("kernel32.dll")]
        static extern bool AllocConsole();
        [DllImport("kernel32.dll")]
        static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        static extern IntPtr GetStdHandle(uint nStdHandle);

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        private const uint STD_INPUT_HANDLE = 0xFFFFFFF6;
        private const uint STD_OUTPUT_HANDLE = 0xFFFFFFF5;
        private const uint STD_ERROR_HANDLE = 0xFFFFFFF4;

        private readonly static TextReader _originalIn = Console.In;
        private readonly static TextWriter _originalOut = Console.Out;
        private readonly static TextWriter _originalErr = Console.Error;

        public static Keys OpenCloseKey => Keys.OemTilde;
        public static bool IsOpened { get; private set; } = false;

        private static CancellationTokenSource _cancellationTokenSource;
        public static bool Open()
        {
            if (!IsOpened)
            {
                IsOpened = true;
                bool result = AllocConsole();

                if (result)
                {
                    SetHandles();
                    Console.Title = "monoconsole";
                    Console.WriteLine($"Opened ({GetKey(8)})");

                    SetForegroundWindow(Main.Instance.Window.Handle);

                    _cancellationTokenSource = new CancellationTokenSource();
                    new Thread(() => ConsoleThread(_cancellationTokenSource.Token))
                    {
                        IsBackground = true
                    }.Start();
                }

                return result;
            }
            return false;
        }
        public static bool Close()
        {
            if (IsOpened)
            {
                IsOpened = false;
                FreeConsole();

                _cancellationTokenSource?.Cancel();
                Reset();
                return true;
            }
            return false;
        }
        public static bool SwitchState() => !IsOpened ? Open() : Close();
        public static void New()
        {
            if(!IsOpened)
                Open();
            else
            {
                Close();
                Open();
            }
        }

        private static void Reset()
        {
            Console.SetOut(_originalOut);
            Console.SetError(_originalErr);
            Console.SetIn(_originalIn);
        }
        private static void SetHandles()
        {
            SafeFileHandle sfhIn = new(GetStdHandle(STD_INPUT_HANDLE), false);
            SafeFileHandle sfhOut = new(GetStdHandle(STD_OUTPUT_HANDLE), false);
            SafeFileHandle sfhErr = new(GetStdHandle(STD_ERROR_HANDLE), false);

            Console.SetIn(new StreamReader(new FileStream(sfhIn, FileAccess.Read)));
            Console.SetOut(new StreamWriter(new FileStream(sfhOut, FileAccess.Write)) { AutoFlush = true });
            Console.SetError(new StreamWriter(new FileStream(sfhErr, FileAccess.Write)) { AutoFlush = true });
        }

        private static void ConsoleThread(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (Console.KeyAvailable)
                {
                    string input = Console.ReadLine();

                    if (input == "close")
                        Close();
                    else if (input == "new")
                        New();
                    else if (input == "clear")
                        Console.Clear();
                    else if (input == "exit")
                        Main.Instance.Exit();
                    else if (input == "mem")
                        Console.WriteLine(
                            $"collcount 0: [{GC.CollectionCount(0)}]\n" +
                            $"collcount 1: [{GC.CollectionCount(1)}]\n" +
                            $"collcount 2: [{GC.CollectionCount(2)}]\n" +
                            $"current used: [{GC.GetTotalMemory(false).SizeString()}]\n" +
                            $"cleared: [{GC.GetTotalMemory(true).SizeString()}]");
                }

                Thread.Sleep(100);
            }
        }

        private static string GetKey(int length)
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            List<char> result = new();
            Random rnd = new();
            for (int i = 0; i < length; i++) 
                result.Add(chars[rnd.Next(chars.Length)]);

            return string.Join("", result);
        }
    }
}