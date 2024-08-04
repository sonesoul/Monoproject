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
        public const int WS_SYSMENU = 0x80000;
        #endregion
    }
}