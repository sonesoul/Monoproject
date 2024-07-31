using Microsoft.Xna.Framework.Input;
using System;
using System.Runtime.InteropServices;
namespace GlobalTypes
{
    static class GameConsole
    {
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        public static Keys OpenCloseKey => Keys.OemTilde;
        public static bool IsOpened { get; private set; } = false;

        public static bool Open()
        {
            IsOpened = true;

            bool result = AllocConsole();
            Console.WriteLine("Succesfully created");
            return result;
        }
        public static bool Close()
        {
            IsOpened = false;
            return FreeConsole();
        }

        public static void SwitchState()
        {
            if (IsOpened)
                Close();
            else 
                Open();
        }
    }
}