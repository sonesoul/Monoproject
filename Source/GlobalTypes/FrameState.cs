using Microsoft.Xna.Framework;
using GlobalTypes.Events;
using Microsoft.Xna.Framework.Input;

namespace GlobalTypes
{
    [Init(nameof(Init), InitOrders.FrameInfo)]
    public static class FrameState
    {
        public const float FixedDeltaTime = 1.0f / 60.0f;

        public static GameTime GameTime { get; private set; }
        public static float DeltaTime { get; private set; }
        public static float DeltaTimeMs { get; private set; }
        public static ref KeyboardState KeyState => ref _keyState;
        public static ref MouseState MouseState => ref _mouseState;

        public static Keys[] KeysDown => KeyState.GetPressedKeys();
        public static Vector2 MousePosition => MouseState.Position.ToVector2();
        public static int MouseX => MouseState.X;
        public static int MouseY => MouseState.Y;
        public static int MouseScroll => MouseState.ScrollWheelValue;


        private static KeyboardState _keyState;
        private static MouseState _mouseState;
        private static float _updateBuffer = 0.0f;
        private static bool isInited = false;

        private static void Init()
        {
            if (isInited)
                return;

            FrameEvents.Update.Add(UpdateValues, UpdateOrders.FrameInfo);
            isInited = true;
        }

        private static void UpdateValues(GameTime gameTime)
        {
            _keyState = Keyboard.GetState();
            _mouseState = Mouse.GetState();
            GameTime = gameTime;
            DeltaTimeMs = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _updateBuffer += DeltaTime;
            while (_updateBuffer >= FixedDeltaTime)
            {
                FrameEvents.FixedUpdate.Trigger(gameTime);
                _updateBuffer -= FixedDeltaTime;
            }
        }
    }
}