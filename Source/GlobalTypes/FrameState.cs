using GlobalTypes.Events;
using Microsoft.Xna.Framework.Input;
using System;
using GlobalTypes.InputManagement;

namespace GlobalTypes
{
    public static class FrameState
    {
        public const float FixedDeltaTime = 1.0f / 60;

        public static GameTime GameTime { get; private set; }

        public static float DeltaTime { get; private set; }
        public static float DeltaTimeMs { get; private set; }
        public static float DeltaTimeUnscaled { get; private set; }

        public static float FPS { get; private set; } = 0;
        public static float FPSUpdateStep { get; set; } = 0.1f;

        public static ref KeyboardState KeyState => ref _keyState;
        public static ref MouseState MouseState => ref _mouseState;

        public static float TimeScale { get; set; } = 1f;

        public static Key[] KeysDown => Input.GetPressedKeys();
        public static Vector2 MousePosition => MouseState.Position.ToVector2();
        public static int MouseScroll => MouseState.ScrollWheelValue;

        private static KeyboardState _keyState;
        private static MouseState _mouseState;

        private static double fixedUpdateBuffer = 0;
        
        private static float frameCount = 0;
        private static double fpsBuffer = 0;

        public static void UpdateGameTime(GameTime gameTime)
        {
            GameTime = gameTime ?? throw new ArgumentNullException(nameof(gameTime));
            var elapsed = gameTime.ElapsedGameTime;

            DeltaTimeMs = (float)elapsed.TotalMilliseconds;
            DeltaTime = (float)elapsed.TotalSeconds;

            DeltaTimeUnscaled = DeltaTime;
            
            DeltaTime *= TimeScale;
            DeltaTimeMs *= TimeScale;
        }
        public static void Update() 
        {
            _keyState = Keyboard.GetState();
            _mouseState = Mouse.GetState();

            UpdateFPS();

            fixedUpdateBuffer += DeltaTime;
            while (fixedUpdateBuffer >= FixedDeltaTime)
            {
                FrameEvents.FixedUpdate.Trigger();
                fixedUpdateBuffer -= FixedDeltaTime;
            }
        }

        private static void UpdateFPS()
        {
            frameCount++;
            fpsBuffer += DeltaTimeUnscaled * 1000;
            double bufferSeconds = fpsBuffer / 1000;

            if (bufferSeconds >= FPSUpdateStep)
            {
                FPS = (int)(frameCount / bufferSeconds);
                frameCount = 0;
                fpsBuffer = 0;
            }
        }
    }
}