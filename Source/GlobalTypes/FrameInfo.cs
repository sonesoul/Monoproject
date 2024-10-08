using Microsoft.Xna.Framework;
using GlobalTypes.Events;
using Microsoft.Xna.Framework.Input;
using System;

namespace GlobalTypes
{
    public static class FrameInfo
    {
        public const float FixedDeltaTime = 1.0f / 60.0f;

        public static GameTime GameTime { get; private set; }
        public static float DeltaTime { get; private set; }
        public static float DeltaTimeMs { get; private set; }

        public static float FPS { get; private set; } = 0;
        public static float FPSUpdateStep { get; set; } = 0.1f;

        public static ref KeyboardState KeyState => ref _keyState;
        public static ref MouseState MouseState => ref _mouseState;

        public static Keys[] KeysDown => KeyState.GetPressedKeys();
        public static Vector2 MousePosition => MouseState.Position.ToVector2();
        public static int MouseX => MouseState.X;
        public static int MouseY => MouseState.Y;
        public static int MouseScroll => MouseState.ScrollWheelValue;

        private static KeyboardState _keyState;
        private static MouseState _mouseState;

        private static float fixedUpdateBuffer = 0.0f;
        
        private static float frameCount = 0;
        private static double fpsBuffer = 0;
        
        public static void UpdateGameTime(GameTime gameTime)
        {
            GameTime = gameTime ?? throw new ArgumentNullException(nameof(gameTime));

            DeltaTimeMs = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
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
            fpsBuffer += DeltaTimeMs;
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