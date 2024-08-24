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
using System.Linq;

namespace GlobalTypes
{
    public static class Monoconsole
    {
        private static class CommandManager
        {
            private static class Commands
            {
                public static void Mem()
                {
                    WriteLine(
                        $"usg: [{GC.GetTotalMemory(false).ToSizeString()}]\n" +
                        $"avg: [{GC.GetTotalMemory(true).ToSizeString()}]",
                        ConsoleColor.Cyan);

                    WriteLine(
                        $"GC0: [{GC.CollectionCount(0)}]\n" +
                        $"GC1: [{GC.CollectionCount(1)}]\n" +
                        $"GC2: [{GC.CollectionCount(2)}]",
                        ConsoleColor.DarkCyan);

                }
                public static void SetColor(string arg)
                {
                    switch (arg)
                    {
                        case "all":
                            var values = Enum.GetValues<ConsoleColor>()
                                 .Cast<ConsoleColor>()
                                 .Where(v => v != ConsoleColor.Black)
                                 .OrderBy(color => color.ToString(), StringComparer.OrdinalIgnoreCase).ToList();

                            foreach (var value in values)
                                WriteLine(value, value).Wait();

                             break;
                        case "reset":
                            Console.ResetColor();
                            WriteLine($"Color set to {CurrentColor}", CurrentColor);
                            break;
                        default:
                            if (!Enum.TryParse<ConsoleColor>(arg, true, out var color))
                                WriteLine($"Color not found", ConsoleColor.Red);
                            else
                            {
                                if (color == ConsoleColor.Black)
                                {
                                    WriteLine("I can't see black text on my black console background. Suffer with me :3");
                                    return;
                                }

                                SetOutputColor(color);
                                WriteLine($"Color set to {color}", CurrentColor);
                            }
                            break;
                    }
                }
            }

            private readonly static Dictionary<string, Action<string>> commands = new()
            {
                { "new", (arg) => New() },
                { "exit", (arg) => Close() },
                { "clear", (arg) => { Console.Clear(); WriteLine(openString); } },
                { "f1", (arg) => Main.Instance.Exit() },
                { "mem", (arg) => Commands.Mem() },
                { "color", Commands.SetColor},
            };

            public static void Execute(string input)
            {
                input = input.ToLower().Trim();

                if (string.IsNullOrEmpty(input))
                    return;

                string[] commandArgPair = input.Split(" ");
                if (!commands.TryGetValue(commandArgPair[0], out var action))
                {
                    WriteLine($"Unexpected command: \"{input}\"", ConsoleColor.Red);
                    return;
                }

                action?.Invoke(commandArgPair.Length > 1 ? commandArgPair[1] : "");
            }            
        }

        private readonly static TextReader _originalIn = Console.In;
        private readonly static TextWriter _originalOut = Console.Out;
        private readonly static TextWriter _originalErr = Console.Error;

        public static Keys ToggleKey => Keys.OemTilde;
        public static ConsoleColor CurrentColor => Console.ForegroundColor;
        public static bool IsOpened { get; private set; } = false;
        private static string openString = "";
        private static CancellationTokenSource _cancellationTokenSource;
        private readonly static object _lock = new();


        public static bool Open()
        {
            if (!IsOpened)
            {
                IsOpened = true;
                bool result = AllocConsole();

                if (result)
                {
                    SetHandles();
                    SetOutputColor(ConsoleColor.Yellow);
                    Console.Title = "monoconsole";
                    openString = $"Opened ({GenerateKey(8)})";
                    WriteLine(openString);

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
            CommandManager.Execute(command);
        }
        public static void SetOutputColor(ConsoleColor color) => Console.ForegroundColor = color;

        private static void ConsoleThread(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    CommandManager.Execute(Console.ReadLine());   
                }
                catch (Exception ex)
                {
                    WriteLine($"Unhandled exception: {ex.Message}", ConsoleColor.Red);
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
        private static async Task WriteAsync(Action writeAction, ConsoleColor color = ConsoleColor.White)
        {
            await Task.Run(() =>
            {
                lock (_lock)
                {
                    if (IsOpened)
                    {
                        var temp = Console.ForegroundColor;
                        SetOutputColor(color);

                        writeAction();

                        SetOutputColor(temp);
                    }
                }
            });
        }

        public static Task WriteLine(ConsoleColor color = ConsoleColor.Gray) => WriteAsync(() => Console.WriteLine(), color);
        public static Task WriteLine(bool value, ConsoleColor color = ConsoleColor.Gray) => WriteAsync(() => Console.WriteLine(value), color);
        public static Task WriteLine(char value, ConsoleColor color = ConsoleColor.Gray) => WriteAsync(() => Console.WriteLine(value), color);
        public static Task WriteLine(char[] buffer, ConsoleColor color = ConsoleColor.Gray) => WriteAsync(() => Console.WriteLine(buffer), color);
        public static Task WriteLine(decimal value, ConsoleColor color = ConsoleColor.Gray) => WriteAsync(() => Console.WriteLine(value), color);
        public static Task WriteLine(double value, ConsoleColor color = ConsoleColor.Gray) => WriteAsync(() => Console.WriteLine(value), color);
        public static Task WriteLine(float value, ConsoleColor color = ConsoleColor.Gray) => WriteAsync(() => Console.WriteLine(value), color);
        public static Task WriteLine(int value, ConsoleColor color = ConsoleColor.Gray) => WriteAsync(() => Console.WriteLine(value), color);
        public static Task WriteLine(long value, ConsoleColor color = ConsoleColor.Gray) => WriteAsync(() => Console.WriteLine(value), color);
        public static Task WriteLine(object value, ConsoleColor color = ConsoleColor.Gray) => WriteAsync(() => Console.WriteLine(value), color);
        public static Task WriteLine(string value, ConsoleColor color = ConsoleColor.Gray) => WriteAsync(() => Console.WriteLine(value), color);
        public static Task WriteLine(uint value, ConsoleColor color = ConsoleColor.Gray) => WriteAsync(() => Console.WriteLine(value), color);
        public static Task WriteLine(ulong value, ConsoleColor color = ConsoleColor.Gray) => WriteAsync(() => Console.WriteLine(value), color);

        public static Task Write(bool value, ConsoleColor color = ConsoleColor.Gray) => WriteAsync(() => Console.Write(value), color);
        public static Task Write(char value, ConsoleColor color = ConsoleColor.Gray) => WriteAsync(() => Console.Write(value), color);
        public static Task Write(char[] buffer, ConsoleColor color = ConsoleColor.Gray) => WriteAsync(() => Console.Write(buffer), color);
        public static Task Write(decimal value, ConsoleColor color = ConsoleColor.Gray) => WriteAsync(() => Console.Write(value), color);
        public static Task Write(double value, ConsoleColor color = ConsoleColor.Gray) => WriteAsync(() => Console.Write(value), color);
        public static Task Write(float value, ConsoleColor color = ConsoleColor.Gray) => WriteAsync(() => Console.Write(value), color);
        public static Task Write(int value, ConsoleColor color = ConsoleColor.Gray) => WriteAsync(() => Console.Write(value), color);
        public static Task Write(long value, ConsoleColor color = ConsoleColor.Gray) => WriteAsync(() => Console.Write(value), color);
        public static Task Write(object value, ConsoleColor color = ConsoleColor.Gray) => WriteAsync(() => Console.Write(value), color);
        public static Task Write(string value, ConsoleColor color = ConsoleColor.Gray) => WriteAsync(() => Console.Write(value), color);
        public static Task Write(uint value, ConsoleColor color = ConsoleColor.Gray) => WriteAsync(() => Console.Write(value), color);
        public static Task Write(ulong value, ConsoleColor color = ConsoleColor.Gray) => WriteAsync(() => Console.Write(value), color);

        #endregion
    }
}