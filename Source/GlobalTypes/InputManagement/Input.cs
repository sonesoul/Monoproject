using GlobalTypes.Collections;
using GlobalTypes.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GlobalTypes.InputManagement
{
    [Init(nameof(Init), InitOrders.Input)]
    public static class Input
    {
        public static AxisCulture AxisCulture { get; private set; } = AxisCulture.Arrows;

        public static Vector2 Axis => axis;
        public static Vector2 UnitAxis => axis.Normalized();

        public static event Action<Key> KeyPressed;
        public static event Action<Key> KeyHolded;
        public static event Action<Key> KeyReleased;

        private static ref KeyboardState KeyState => ref FrameInfo.KeyState;
        private static ref MouseState MouseState => ref FrameInfo.MouseState;

        private static Vector2 axis = Vector2.Zero;
        private static KeyBinding RightKey, LeftKey, UpKey, DownKey;

        private readonly static LockList<KeyBinding> pressKeys = new(), holdKeys = new(), releaseKeys = new();

        private static List<Key> wasPressed = new();

        private static void Init()
        {
            AxisCulture.Deconstruct(out var up, out var down, out var left, out var right);

            RightKey = new(right, KeyPhase.Hold, null, -4);
            LeftKey = new(left, KeyPhase.Hold, null, -3);
            UpKey = new(up, KeyPhase.Hold, null, -2);
            DownKey = new(down, KeyPhase.Hold, null, -1);

            Bind(RightKey, LeftKey, UpKey, DownKey);
            FrameEvents.Update.Add(_ => Update(), UpdateOrders.InputManager);
        }

        private static void Update()
        {
            Key[] pressed = GetPressedKeys();

            foreach (var key in pressed)
            {
                if (!wasPressed.Contains(key))
                    KeyPressed?.Invoke(key);
                else
                    KeyHolded?.Invoke(key);
            }

            foreach (var key in wasPressed)
            {
                if (!pressed.Contains(key))
                    KeyReleased?.Invoke(key);
            }

            wasPressed.Clear();
            wasPressed.AddRange(pressed);


            UpdateAxis();
            CheckInput();
        }

        private static void UpdateAxis()
        {
            axis.X = (IsKeyDown(RightKey.Key) ? 1 : 0) - (IsKeyDown(LeftKey.Key) ? 1 : 0);
            axis.Y = (IsKeyDown(UpKey.Key) ? 1 : 0) - (IsKeyDown(DownKey.Key) ? 1 : 0);
        }
        private static void UpdateAxisKeys()
        {
            AxisCulture.Deconstruct(out var up, out var down, out var left, out var right);

            RightKey.Key = right;
            LeftKey.Key = left;
            UpKey.Key = up;
            DownKey.Key = down;
        }

        public static Key[] GetPressedKeys()
        {
            List<Key> pressedKeys = new();
            pressedKeys.AddRange(KeyState.GetPressedKeys().Select(k => (Key)k));

            Key[] mouseKeys = 
            {
                Key.MouseLeft, 
                Key.MouseRight, 
                Key.MouseMiddle, 
                Key.MouseXButton1, 
                Key.MouseXButton2 
            };

            foreach (var k in mouseKeys)
            {
                if(IsKeyDown(k))
                    pressedKeys.Add(k);
            }

            return pressedKeys.ToArray();
        }

        private static void CheckInput()
        {
            pressKeys.LockForEach(k =>
            {
                bool isDown = k.IsDown = IsKeyDown(k.Key);

                if (!k.WasDown && isDown)
                    k.Action?.Invoke();

                k.WasDown = isDown;
            });
            
            holdKeys.LockForEach(k =>
            {
                bool isDown = k.IsDown = IsKeyDown(k.Key);

                if (k.WasDown && isDown)
                    k.Action?.Invoke();

                k.WasDown = isDown;
            });
            
            releaseKeys.LockForEach(k =>
            {
                bool isDown = k.IsDown = IsKeyDown(k.Key);
                bool isUp = k.IsUp;

                if (k.WasDown && isUp)
                    k.Action?.Invoke();

                k.WasDown = isDown;
            });
        }

        public static void Bind(KeyBinding binding)
        {
            ThrowIfBindingNull(binding);

            var keyCollection = GetCollection(binding.TriggerPhase);
            int index = keyCollection.ToList().FindIndex(k => k.Order > binding.Order);

            if (index == -1)
                keyCollection.Add(binding);
            else
                keyCollection.Insert(index, binding);
        }
        public static void Bind(params KeyBinding[] bindings)
        {
            foreach (var kl in bindings)
                Bind(kl);
        }
        public static KeyBinding Bind(Key key, KeyPhase phase, Action action = null, int order = 0)
        {
            KeyBinding binding = new(key, phase, action, order);

            Bind(binding);
            return binding;
        }

        public static void BindSingle(KeyBinding binding)
        {
            KeyBinding singleTriggerListener = new(binding.Key, binding.TriggerPhase, null, binding.Order);

            void SelfRemove()
            {
                binding.Action?.Invoke();
                Unbind(singleTriggerListener);
            };

            singleTriggerListener.Action = SelfRemove;
            Bind(singleTriggerListener);
        }
        public static void BindSingle(Key key, KeyPhase phase, Action action = null, int order = 0)
        {
            BindSingle(new KeyBinding(key, phase, action, order));
        }
        
        public static void Unbind(KeyBinding binding)
        {
            ThrowIfBindingNull(binding);

            GetCollection(binding.TriggerPhase).Remove(binding);
        }
        public static void Unbind(params KeyBinding[] bindings)
        {
            foreach (var item in bindings)
            {
                Unbind(item);
            }
        }
        public static void SetEvent(KeyBinding binding, KeyPhase newPhase)
        {
            Unbind(binding);
            binding.TriggerPhase = newPhase;
            Bind(binding);
        }
        public static void SetOrder(KeyBinding binding, int newOrder)
        {
            Unbind(binding);
            binding.Order = newOrder;
            Bind(binding);
        }

        public static bool IsKeyDown(Key key)
        {
            return IsMouseKey(key) ? IsMouseKeyDown(key) : KeyState.IsKeyDown((Keys)key);
        }
        public static bool IsKeyUp(Key key)
        {
            return IsMouseKey(key) ? IsMouseKeyUp(key) : KeyState.IsKeyUp((Keys)key);
        }

        public static bool IsKeyboardKey(Key key) => (int)key < 1000;
        public static bool IsMouseKey(Key key) => (int)key >= 1000;

        private static bool IsMouseKeyDown(Key key) => key switch
        {
            Key.MouseLeft => MouseState.LeftButton == ButtonState.Pressed,
            Key.MouseRight => MouseState.RightButton == ButtonState.Pressed,
            Key.MouseMiddle => MouseState.MiddleButton == ButtonState.Pressed,
            Key.MouseXButton1 => MouseState.XButton1 == ButtonState.Pressed,
            Key.MouseXButton2 => MouseState.XButton2 == ButtonState.Pressed,
            _ => throw new InvalidOperationException("Mouse key not found."),
        };
        private static bool IsMouseKeyUp(Key key) => key switch
        {
            Key.MouseLeft => MouseState.LeftButton == ButtonState.Released,
            Key.MouseRight => MouseState.RightButton == ButtonState.Released,
            Key.MouseMiddle => MouseState.MiddleButton == ButtonState.Released,
            Key.MouseXButton1 => MouseState.XButton1 == ButtonState.Released,
            Key.MouseXButton2 => MouseState.XButton2 == ButtonState.Released,
            _ => throw new InvalidOperationException("Mouse key not found."),
        };

        public static void SetAxisCulture(AxisCulture axisCulture)
        {
            AxisCulture = axisCulture;
            UpdateAxisKeys();
        }

        private static LockList<KeyBinding> GetCollection(KeyPhase phase)
        {
            return phase switch
            {
                KeyPhase.Press => pressKeys,
                KeyPhase.Hold => holdKeys,
                KeyPhase.Release => releaseKeys,
                _ => throw new InvalidOperationException("Key collection not found.")
            };
        }

        private static void ThrowIfBindingNull(KeyBinding binding)
        {
            if (binding == null)
                throw new ArgumentNullException("Key binding is null.");
        }

        internal static void Bind(Key jumpKey, KeyPhase press, object jump)
        {
            throw new NotImplementedException();
        }
    }
}