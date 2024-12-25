using GlobalTypes;
using InGame.Interfaces;
using Monoproject;

namespace InGame.Overlays.Screens
{
    public class MainMenuScreen : IOverlayScreen
    {
        private HotKeyButton startButton, quitButton;

        public void Show()
        {
            startButton = new("Start") { Position = Window.Center };
            startButton.HotKeyReleased += () =>
            {
                Disable();
                Session.Start();
            };

            quitButton = new("Quit") { Position = startButton.Position.WhereY(y => y + (startButton.Size.Y * 1.5f)) };
            quitButton.HotKeyReleased += () =>
            {
                Disable();
                Main.Instance.Exit();
            };
        }
        public void Hide()
        {
            startButton?.Destroy();
            quitButton?.Destroy();

            startButton = null;
            quitButton = null;
        }

        public void Enable()
        {
            startButton.Enabled = true;
            quitButton.Enabled = true;
        }
        public void Disable()
        {
            startButton.Enabled = false;
            quitButton.Enabled = false;
        }
    }
}