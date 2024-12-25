using GlobalTypes.Events;
using Microsoft.Xna.Framework.Input;
using Monoproject;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GlobalTypes.InputManagement
{
    public static class Input
    {
        public static event Action<Key> KeyPressed, KeyHolded, KeyReleased;

        public static AxisCulture AxisCulture { get; private set; } = AxisCulture.Arrows;
        public static Vector2 Axis => axis;
        public static Vector2 UnitAxis => axis.Normalized();
        

        private static ref KeyboardState KeyState => ref FrameState.KeyState;
        private static ref MouseState MouseState => ref FrameState.MouseState;

        private static Vector2 axis = Vector2.Zero;
        private static KeyBinding RightKey, LeftKey, UpKey, DownKey;

        private readonly static List<KeyBinding> binds = new();
        private readonly static HashSet<Key> manuallyDownKeys = new();
        private readonly static List<Key> wasPressed = new();
        
        [Init(InitOrders.Input)]
        private static void Init()
        {
            AxisCulture.Deconstruct(out var up, out var down, out var left, out var right);

            RightKey = new(right, KeyPhase.Hold, null, -4);
            LeftKey = new(left, KeyPhase.Hold, null, -3);
            UpKey = new(up, KeyPhase.Hold, null, -2);
            DownKey = new(down, KeyPhase.Hold, null, -1);

            AddBinds(RightKey, LeftKey, UpKey, DownKey);
            FrameEvents.UpdateUnscaled.Add(Update, UpdateUnscaledOrders.InputManager);

            Main.Instance.Window.KeyDown += (obj, k) =>
            {
                Key key = (Key)k.Key;

                if (wasPressed.Contains(key))
                    return;

                KeyPressed?.Invoke(key);
                wasPressed.Add(key);
            };
            Main.Instance.Window.KeyUp += (obj, k) =>
            {
                Key key = (Key)k.Key;

                KeyReleased?.Invoke(key);
                wasPressed.Remove(key);
            };
        }

        private static void Update()
        {
            Key[] pressed = GetPressedKeys();

            foreach (var key in pressed)
            {
                if (wasPressed.Contains(key))
                    KeyHolded?.Invoke(key);
            }

            UpdateAxis();
            binds.For(b => b?.Update());
        }

        private static void UpdateAxis()
        {
            axis.X = (IsKeyDown(RightKey.Key) ? 1 : 0) - (IsKeyDown(LeftKey.Key) ? 1 : 0);
            axis.Y = (IsKeyDown(DownKey.Key) ? 1 : 0) - (IsKeyDown(UpKey.Key) ? 1 : 0);
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

        public static void AddBind(KeyBinding binding)
        {
            ThrowIfBindingNull(binding);

            int index = binds.ToList().FindIndex(k => k.Order > binding.Order);

            if (index == -1)
                binds.Add(binding);
            else
                binds.Insert(index, binding);
        }
        public static void AddBinds(params KeyBinding[] bindings)
        {
            foreach (var b in bindings)
                AddBind(b);
        }
        public static KeyBinding Bind(Key key, KeyPhase phase, Action action = null, int order = 0)
        {
            KeyBinding binding = new(key, phase, action, order);

            AddBind(binding);
            return binding;
        }

        public static void BindSingle(KeyBinding binding)
        {
            KeyBinding bind = new(binding.Key, binding.TriggerPhase, null, binding.Order);

            void SelfRemove()
            {
                binding.Action?.Invoke();
                Unbind(bind);
            };

            bind.Action = SelfRemove;
            AddBind(bind);
        }
        public static void BindSingle(Key key, KeyPhase phase, Action action = null, int order = 0)
        {
            BindSingle(new KeyBinding(key, phase, action, order));
        }
        
        public static void Unbind(KeyBinding binding)
        {
            ThrowIfBindingNull(binding);

            binds.Remove(binding);
        }
        public static void Unbind(params KeyBinding[] bindings)
        {
            foreach (var item in bindings)
            {
                Unbind(item);
            }
        }
        public static void SetOrder(KeyBinding binding, int newOrder)
        {
            Unbind(binding);
            binding.Order = newOrder;
            AddBind(binding);
        }

        public static void SetKey(Key key, bool setDown)
        {
            if (setDown)
            {
                manuallyDownKeys.Add(key);
            }
            else
            {
                manuallyDownKeys.Remove(key);
            }
        }

        public static bool IsKeyDown(Key key)
        {
            if (manuallyDownKeys.Contains(key))
            {
                return true;
            }

            if (IsMouseKey(key))
            {
                return IsMouseKeyDown(key);
            }
            else
            {
                return KeyState.IsKeyDown((Keys)key);
            }
        }
        public static bool IsKeyUp(Key key) => !IsKeyDown(key);

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
        private static bool IsMouseKeyUp(Key key) => !IsMouseKeyDown(key);

        public static void SetAxisCulture(AxisCulture axisCulture)
        {
            AxisCulture = axisCulture;
            UpdateAxisKeys();
        }

        private static void ThrowIfBindingNull(KeyBinding binding)
        {
            if (binding == null)
                throw new ArgumentNullException("Key binding is null.");
        }
    }
}