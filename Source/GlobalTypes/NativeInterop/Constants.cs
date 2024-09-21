namespace GlobalTypes.NativeInterop
{
    /// <summary>
    /// Contains constants used for interacting with the Windows API.
    /// </summary>
    internal class Constants
    {
        #region Console
        public const uint STD_INPUT_HANDLE = 0xFFFFFFF6;
        public const uint STD_OUTPUT_HANDLE = 0xFFFFFFF5;
        public const uint STD_ERROR_HANDLE = 0xFFFFFFF4;
        #endregion

        #region Window
        public const int GWL_STYLE = -16;
        public const int GWL_EXSTYLE = -20;

        public const int WS_SYSMENU = 0x80000;
        public const int WS_EX_TOOLWINDOW = 0x00000080;
        public const int WS_EX_APPWINDOW = 0x00040000;

        public const int SW_HIDE = 0;
        public const int SW_SHOW = 5;
        #endregion
    }
}