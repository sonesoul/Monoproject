using GlobalTypes;
using InGame.Difficulty;
using InGame.GameObjects;
using InGame.Overlays.Screens;
using Monoproject;
using System;

namespace InGame.Managers
{
    public static class SessionManager 
    {
        public static bool IsStarted { get; private set; } = false;
        public static bool IsFreezed { get; private set; } = false;
        public static DifficultyScaler Difficulty { get; set; } = new();
        public static Score Score { get; set; } = null;

        public static event Action Started, Ended;
        
        private static Player player;

        [Init]
        private static void Init()
        {
            Level.Failed += Over;
        }

        public static void Start()
        {
            if (IsStarted)
                return;

            OverlayManager.HideScreen();

            player = new Player();
            Level.AddObject(player);
            IsStarted = true;

            Unfreeze();
            LoadLevel();

            Difficulty.ResetProgress();
            Difficulty.StartScaling();
            Score?.Destroy();
            Score = new(player);

            Started?.Invoke();
        }
        public static void End()
        {
            if (!IsStarted)
                return;

            Unfreeze();

            Level.Clear();
            Level.RemoveObject(player);
            player = null;

            IsStarted = false;

            Main.Instance.ForceGC();

            Difficulty.CancelAll();
            Difficulty.StopScaling();

            OverlayManager.HideScreen();

            Ended?.Invoke();
        }
        public static void Over()
        {
            Freeze();
            OverlayManager.ShowScreen<GameOverScreen>();
        }
        public static void Restart()
        {
            End();
            Start();
        }

        public static void LoadLevel()
        {
            LevelConfig.MapIndex = Level.RegularLevels.Pop();
            Level.Load();
        }        
        
        public static void Freeze()
        {
            FrameState.TimeScale = 0;
            SetPlayerActive(false);
            IsFreezed = true;
        }
        public static void Unfreeze()
        {
            FrameState.TimeScale = 1;
            SetPlayerActive(true);
            IsFreezed = false;
        }


        private static void SetPlayerActive(bool value)
        {
            if (player == null)
                return;

            player.Codes.CanCombinate = value;
            player.Movement.CanJump = value;
            player.Movement.CanMove = value;
        }
    }
}