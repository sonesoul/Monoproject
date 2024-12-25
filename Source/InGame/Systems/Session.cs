using GlobalTypes;
using InGame.Interfaces;
using InGame.Managers;
using InGame.Overlays;
using InGame.Overlays.Screens;
using System;
using System.Collections;

namespace InGame
{
    public static class Session
    {
        [Init]
        private static void Init()
        {
            Level.Completed += NextLevel;
            SessionManager.Difficulty.ModifierAdded += ShowDiffcultyChange;
        }

        public static void Start()
        {
            ScreenFade.FadeTo(SessionManager.Start);
        }
        public static void Restart()
        {
            ScreenFade.FadeTo(SessionManager.Restart);
        }

        public static void NextLevel()
        {
            ScreenFade.FadeTo(SessionManager.LoadLevel);
        }

        public static void End()
        {
            ScreenFade.FadeTo(SessionManager.End);
        }

        public static void GoToMainMenu()
        {
            OverlayManager.ShowScreen<MainMenuScreen>();
        }

        private static void ShowDiffcultyChange(IDifficultyModifier m)
        {
            IEnumerator DelayedShow()
            {
                InfoWindow window = new(m.Message);

                OverlayManager.ShowScreen(window);

                yield return StepTask.Delay(2f);

                if (OverlayManager.Current == window)
                {
                    OverlayManager.HideScreen();
                }

            }

            StepTask.Run(DelayedShow);
        }
    }
}