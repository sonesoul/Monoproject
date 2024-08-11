using Microsoft.Win32.SafeHandles;
using Microsoft.Xna.Framework.Input;
using Monoproject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static GlobalTypes.NativeInterop.NativeMethods;
using static GlobalTypes.NativeInterop.Constants;
using System.Diagnostics;

namespace GlobalTypes
{
    public static class GameConsole
    {
        private static class CommandManager
        {
            private static class Commands
            {
                public static void Mem(string arg)
                {
                    WriteLine(
                            $"GC0: [{GC.CollectionCount(0)}]\n" +
                            $"GC1: [{GC.CollectionCount(1)}]\n" +
                            $"GC2: [{GC.CollectionCount(2)}]\n" +
                            $"usg: [{GC.GetTotalMemory(false).ToSizeString()}]\n" +
                            $"avg: [{GC.GetTotalMemory(true).ToSizeString()}]");
                }
            }

            private readonly static Dictionary<string, Action<string>> commands = new() 
            {
                { "new", (arg) => New() },
                { "exit", (arg) => Close() },
                { "clear", (arg) => { Console.Clear(); WriteLine(openString); } },
                { "f1", (arg) => Main.Instance.Exit() },
                { "mem", Commands.Mem },
            };

            public static void Handle(string input)
            {
                input = input.ToLower().Trim();
                if (string.IsNullOrEmpty(input))
                    return;

                if (!commands.TryGetValue(input, out var action))
                {
                    WriteLine($"Unexpected command: \"{input}\"");
                    return;
                }


                int index = input.IndexOf(" "); 
                action?.Invoke(index != -1 ? input[index..] : "");
            }            
        }

        private readonly static TextReader _originalIn = Console.In;
        private readonly static TextWriter _originalOut = Console.Out;
        private readonly static TextWriter _originalErr = Console.Error;

        public static Keys ToggleKey => Keys.OemTilde;
        public static bool IsOpened { get; private set; } = false;
        private static string openString = "";
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
                    openString = $"Opened ({GenerateKey(8)})";
                    Console.WriteLine(openString);
                    
                    RemoveSystemMenu();
                    
                    Console.OutputEncoding = Encoding.UTF8;
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
                FreeConsole();

                _cancellationTokenSource?.Cancel();
                Reset();

                IsOpened = false;
                return true;
            }
            return false;
        }
        public static bool ToggleState() => !IsOpened ? Open() : Close();
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
        public static void Execute(string command)
        {
            if (!IsOpened)
                return;
            WriteLine("-> " + command);
            CommandManager.Handle(command);
        }

        private static void ConsoleThread(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    CommandManager.Handle(Console.ReadLine());   
                }
                catch
                {
                    Close();
                    return;
                }
            }
        }

        private static void RemoveSystemMenu()
        {
            IntPtr hWnd = GetConsoleWindow();
            int style = GetWindowLong(hWnd, GWL_STYLE);
            _ = SetWindowLong(hWnd, GWL_STYLE, style & ~WS_SYSMENU);
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
        private static string GenerateKey(int length)
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            List<char> result = new();
            Random rnd = new();
            for (int i = 0; i < length; i++) 
                result.Add(chars[rnd.Next(chars.Length)]);

            return string.Join("", result);
        }
        
        #region ConsoleOutputOverrides
        private static async Task WriteAsync(Action writeAction)
        {
            if (IsOpened)
                await Task.Run(writeAction);
        }

        public static Task WriteLine() => WriteAsync(() => Console.WriteLine());
        public static Task WriteLine(bool value) => WriteAsync(() => Console.WriteLine(value));
        public static Task WriteLine(char value) => WriteAsync(() => Console.WriteLine(value));
        public static Task WriteLine(char[] buffer) => WriteAsync(() => Console.WriteLine(buffer));
        public static Task WriteLine(decimal value) => WriteAsync(() => Console.WriteLine(value));
        public static Task WriteLine(double value) => WriteAsync(() => Console.WriteLine(value));
        public static Task WriteLine(float value) => WriteAsync(() => Console.WriteLine(value));
        public static Task WriteLine(int value) => WriteAsync(() => Console.WriteLine(value));
        public static Task WriteLine(long value) => WriteAsync(() => Console.WriteLine(value));
        public static Task WriteLine(object value) => WriteAsync(() => Console.WriteLine(value));
        public static Task WriteLine(string value) => WriteAsync(() => Console.WriteLine(value));
        public static Task WriteLine(uint value) => WriteAsync(() => Console.WriteLine(value));
        public static Task WriteLine(ulong value) => WriteAsync(() => Console.WriteLine(value));

        public static Task Write(bool value) => WriteAsync(() => Console.Write(value));
        public static Task Write(char value) => WriteAsync(() => Console.Write(value));
        public static Task Write(char[] buffer) => WriteAsync(() => Console.Write(buffer));
        public static Task Write(decimal value) => WriteAsync(() => Console.Write(value));
        public static Task Write(double value) => WriteAsync(() => Console.Write(value));
        public static Task Write(float value) => WriteAsync(() => Console.Write(value));
        public static Task Write(int value) => WriteAsync(() => Console.Write(value));
        public static Task Write(long value) => WriteAsync(() => Console.Write(value));
        public static Task Write(object value) => WriteAsync(() => Console.Write(value));
        public static Task Write(string value) => WriteAsync(() => Console.Write(value));
        public static Task Write(uint value) => WriteAsync(() => Console.Write(value));
        public static Task Write(ulong value) => WriteAsync(() => Console.Write(value));

        #endregion
    }
}