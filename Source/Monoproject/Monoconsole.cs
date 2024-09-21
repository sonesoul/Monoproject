using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static GlobalTypes.NativeInterop.NativeMethods;
using static GlobalTypes.NativeInterop.Constants;
using System.Linq;
using Monoproject;
using GlobalTypes.InputManagement;

namespace GlobalTypes
{
    public static class OLDMonoconsole
    {
        public static Key ToggleKey => Key.OemTilde;
        public static ConsoleColor TextColor { get => Console.ForegroundColor; set => Console.ForegroundColor = value; }
        
        public static bool IsOpened { get; private set; } = false;
        public static bool WriteExecuted { get; set; } = true;

        public static Action<string> Handler { get; set; }
        public static Thread ConsoleThread { get; private set; } = null;

        private readonly static object _lock = new();

        private static CancellationTokenSource _cts;
        private readonly static TextReader _originalIn = Console.In;
        private readonly static TextWriter _originalOut = Console.Out;
        private readonly static TextWriter _originalErr = Console.Error;

        public static bool Open()
        {
            if (!IsOpened)
            {
                IsOpened = true;
                bool result = AllocConsole();

                if (result)
                {
                    SetHandles();
                    TextColor = ConsoleColor.Yellow;
                    Console.Title = "monoconsole";
                    
                    SetWindow();
                    
                    Console.OutputEncoding = Encoding.UTF8;
                    _cts = new();

                    ConsoleThread = new Thread(() => ConsoleRead(_cts.Token))
                    {
                        IsBackground = true
                    };
                    ConsoleThread.Start();

                    List<ConsoleColor> colors = Enum.GetValues<ConsoleColor>().Where(c => c != ConsoleColor.Black).ToList();
                    WriteLine($"| monoconsole [{GenerateKey(8)}]", colors[new Random().Next(colors.Count)]);
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

                _cts?.Cancel();
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
            try
            {
                ConsoleColor color;
                Thread current = Thread.CurrentThread;
                
                if (current == Main.Instance.WindowThread)
                    color = ConsoleColor.DarkYellow;
                else if (current == ConsoleThread)
                    color = ConsoleColor.DarkCyan;
                else 
                    color = ConsoleColor.Blue;

                Task.Run(() =>
                {
                    if (WriteExecuted)
                        WriteLine("> " + command, color).Wait();

                    Handler?.Invoke(command);
                }).Wait();
            }
            catch (Exception ex)
            {
                WriteLine($"{ex.Message} [sync exec]", ConsoleColor.Red).Wait();
            }
            
        }
        public static async Task ExecuteAsync(string command)
        {
            try
            {
                if (WriteExecuted)
                    await WriteLine("> " + command, ConsoleColor.Blue);
                
                await Task.Run(() => Handler?.Invoke(command));
            }
            catch (Exception ex)
            {
                await WriteLine($"{ex.Message} [async exec]", ConsoleColor.Red);
            }
        }
        public static void ExecuteUnsafe(string command)
        {
            Main.Instance.PostToMainThread(() =>
            {
                Task.Run(() =>
                {
                    if (WriteExecuted)
                        WriteLine("> " + command, ConsoleColor.Magenta);

                    Handler?.Invoke(command);
                }).Wait();
            });
        }

        private static void ConsoleRead(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    Handler?.Invoke(Console.ReadLine());
                }
                catch (Exception ex)
                {
                    WriteLine($"{ex.Message} [console read]", ConsoleColor.Red).Wait(CancellationToken.None);
                }
            }
        }

        private static void SetWindow()
        {
            IntPtr hWnd = GetConsoleWindow();
            int style = GetWindowLong(hWnd, GWL_STYLE);
            _ = SetWindowLong(hWnd, GWL_STYLE, style & ~WS_SYSMENU);

            int exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
            _ = SetWindowLong(hWnd, GWL_EXSTYLE, (exStyle & ~WS_EX_APPWINDOW) | WS_EX_TOOLWINDOW);
        }
        private static void Reset()
        {
            Console.SetOut(_originalOut);
            Console.SetError(_originalErr);
            Console.SetIn(_originalIn);
        }
        private static void SetHandles()
        {
            Console.SetIn(
                new StreamReader(
                    new FileStream(
                        new SafeFileHandle(GetStdHandle(STD_INPUT_HANDLE), false), FileAccess.Read)));
            Console.SetOut(
                new StreamWriter(
                    new FileStream(
                        new SafeFileHandle(GetStdHandle(STD_OUTPUT_HANDLE), false), FileAccess.Write)) { AutoFlush = true });
            Console.SetError(
                new StreamWriter(
                    new FileStream(
                        new SafeFileHandle(GetStdHandle(STD_ERROR_HANDLE), false), FileAccess.Write)) { AutoFlush = true });
        }
        private static string GenerateKey(int length)
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            StringBuilder sb = new();
            Random rnd = new();

            for (int i = 0; i < length; i++)
                sb.Append(chars[rnd.Next(chars.Length)]);

            return sb.ToString();
        }

        #region ConsoleOutput
        private static async Task WriteAsync(Action writeAction, ConsoleColor color = ConsoleColor.White)
        {
            await Task.Run(() =>
            {
                lock (_lock)
                {
                    if (IsOpened)
                    {
                        var temp = Console.ForegroundColor;
                        TextColor = color;

                        writeAction();

                        TextColor = temp;                        
                    }
                }
            });
        }

        public static Task WriteState(string value) => WriteLine(value, ConsoleColor.Cyan);
        public static Task WriteError(string value) => WriteLine(value, ConsoleColor.Red);

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