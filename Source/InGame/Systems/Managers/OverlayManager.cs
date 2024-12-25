using InGame.Interfaces;

namespace InGame.Managers
{
    public static class OverlayManager
    {
        public static IOverlayScreen Current { get; private set; }

        public static void ShowScreen<T>() where T : IOverlayScreen, new() => ShowScreen(new T());
        public static void ShowScreen(IOverlayScreen screen)
        {
            HideScreen();

            Current = screen;
            Current.Show();
        }

        public static void EnableScreen() => Current?.Enable();
        public static void DisableScreen() => Current?.Disable();

        public static void HideScreen()
        {
            Current?.Hide();
            Current = null;
        }
    }
}