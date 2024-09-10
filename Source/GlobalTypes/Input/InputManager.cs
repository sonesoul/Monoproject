using GlobalTypes.Collections;
using GlobalTypes.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;

namespace GlobalTypes.Input
{
    [Init(nameof(Init))]
    public static class InputManager
    {
        private static AxisCulture _axisCulture = AxisCulture.WASD;
        public static AxisCulture AxisCulture => _axisCulture;
        private static KeyboardState KeyState => FrameState.KeyState;
        private static MouseState MouseState => FrameState.MouseState;

        public static Vector2 Axis => _axis;
        private static Vector2 _axis = Vector2.Zero;
        private static KeyListener RightKey, LeftKey, UpKey, DownKey;
        private static bool isInited = false;

        private readonly static LockList<KeyListener> _pressKeys = new(), _holdKeys = new(), _releaseKeys = new();
        private readonly static LockList<MouseListener> _pressKeysMouse = new(), _holdKeysMouse = new(), _releaseKeysMouse = new();
       
        private readonly static string KeyListenerNullMessage = "Key listener is null";

        private static void Init()
        {
            if (isInited)
                return;

            AxisCulture.Deconstruct(out var up, out var down, out var left, out var right);

            RightKey = new(right, KeyEvent.Hold, null, -4);
            LeftKey = new(left, KeyEvent.Hold, null, -3);
            UpKey = new(up, KeyEvent.Hold, null, -2);
            DownKey = new(down, KeyEvent.Hold, null, -1);

            AddKeys(RightKey, LeftKey, UpKey, DownKey);
            FrameEvents.Update.Add(_ => Update(), UpdateOrders.InputManager);

            isInited = true;
        }

        private static void Update()
        {
            CheckKeyboard();
            CheckMouse();
            UpdateAxis();
        }

        private static void UpdateAxis()
        {
            _axis.X = (RightKey.IsDown ? 1 : 0) - (LeftKey.IsDown ? 1 : 0);
            _axis.Y = (UpKey.IsDown ? 1 : 0) - (DownKey.IsDown ? 1 : 0);

            if (_axis.LengthSquared() > 1)
                _axis.Normalize();
        }
        public static void UpdateAxisKeys()
        {
            AxisCulture.Deconstruct(out var up, out var down, out var left, out var right);

            RightKey.Key = right;
            LeftKey.Key = left;
            UpKey.Key = up;
            DownKey.Key = down;
        }

        #region Keyboard

        private static void CheckKeyboard()
        {
            _pressKeys.LockForEach(k =>
            {
                bool isDown = k.IsDown = KeyState.IsKeyDown(k.Key);

                if (!k.WasDown && isDown)
                    k.Action?.Invoke();

                k.WasDown = isDown;
            });
            _holdKeys.LockForEach(k =>
            {
                bool isDown = k.IsDown = KeyState.IsKeyDown(k.Key);

                if (k.WasDown && isDown)
                    k.Action?.Invoke();

                k.WasDown = isDown;
            });
            _releaseKeys.LockForEach(k =>
            {
                bool isDown = k.IsDown = KeyState.IsKeyDown(k.Key);
                bool isUp = k.IsUp;

                if (k.WasDown && isUp)
                    k.Action?.Invoke();

                k.WasDown = isDown;
            });
        }

        public static KeyListener AddKey(Keys key, KeyEvent keyEvent, Action action = null, int order = 0)
        {
            KeyListener keyListener = new(key, keyEvent, action, order);

            AddKey(keyListener);
            return keyListener;
        }
        public static void AddKey(KeyListener listener)
        {
            if (listener == null)
                throw new ArgumentNullException(nameof(listener).ToString(), KeyListenerNullMessage);

            LockList<KeyListener> keyCollection = GetCollection(listener.EventType);

            KeyListener found = keyCollection.Where(k => k.Order > listener.Order).FirstOrDefault();

            if (found == null)
                keyCollection.Add(listener);
            else 
                keyCollection.Insert(keyCollection.IndexOf(found), listener);
        }
        public static void AddKeys(params KeyListener[] listener)
        {
            foreach (var kl in listener)
                AddKey(kl);
        }
        public static void AddSingle(KeyListener listener)
        {
            KeyListener singleTriggerListener = new(listener.Key, listener.EventType, null, listener.Order);

            void SelfRemove()
            {
                listener.Action?.Invoke();
                RemoveKey(singleTriggerListener);
            };

            singleTriggerListener.Action = SelfRemove;
            AddKey(singleTriggerListener);
        }
        public static void AddSingle(Keys key, KeyEvent keyEvent, Action action = null, int order = 0)
        {
            AddSingle(new KeyListener(key, keyEvent, action, order));
        }
        public static void RemoveKey(KeyListener listener)
        {
            if (listener == null)
                throw new ArgumentNullException(nameof(listener).ToString(), KeyListenerNullMessage);

            GetCollection(listener.EventType).Remove(listener);
        }
        public static void MoveKey(KeyListener listener, KeyEvent newEvent)
        {
            if (listener == null)
                throw new ArgumentNullException(nameof(listener).ToString(), KeyListenerNullMessage);

            RemoveKey(listener);
            listener.EventType = newEvent;
            AddKey(listener);
        }

        public static bool IsKeyDown(Keys key) => KeyState.IsKeyDown(key);
        public static bool IsKeyUp(Keys key) => KeyState.IsKeyUp(key);

        public static void SetAxisCulture(AxisCulture axisCulture)
        {
            _axisCulture = axisCulture;
            UpdateAxisKeys();
        }

        private static LockList<KeyListener> GetCollection(KeyEvent keyEvent)
        {
            return keyEvent switch
            {
                KeyEvent.Press => _pressKeys,
                KeyEvent.Hold => _holdKeys,
                KeyEvent.Release => _releaseKeys,
                _ => throw new InvalidOperationException("Key collection not found.")
            };
        }

        #endregion

        #region Mouse

        private static void CheckMouse()
        {
            _pressKeysMouse.LockForEach(mouseKey =>
            {
                bool isDown = mouseKey.IsDown = IsMouseKeyDown(mouseKey.Key);

                if (!mouseKey.WasDown && isDown)
                    mouseKey.Action?.Invoke();

                mouseKey.WasDown = isDown;
            });
            _holdKeysMouse.LockForEach(mouseKey =>
            {
                bool isDown = mouseKey.IsDown = IsMouseKeyDown(mouseKey.Key);

                if (mouseKey.WasDown && isDown)
                    mouseKey.Action?.Invoke();

                mouseKey.WasDown = isDown;
            });
            _releaseKeysMouse.LockForEach(mouseKey =>
            {
                bool isDown = mouseKey.IsDown = IsMouseKeyDown(mouseKey.Key);
                bool isUp = mouseKey.IsUp;

                if (mouseKey.WasDown && isUp)
                    mouseKey.Action?.Invoke();

                mouseKey.WasDown = isDown;
            });
        }

        public static MouseListener AddKey(MouseKey key, KeyEvent keyEvent, Action action = null, int order = 0)
        {
            MouseListener listener = new(key, keyEvent, action, order);

            AddKey(listener);
            return listener;
        }
        public static void AddKey(MouseListener mouseListener)
        {
            if (mouseListener == null)
                throw new ArgumentNullException(nameof(mouseListener).ToString(), KeyListenerNullMessage);

            LockList<MouseListener> keyCollection = GetMouseCollection(mouseListener.EventType);

            MouseListener listener = keyCollection.Where(k => k.Order > mouseListener.Order).FirstOrDefault();

            if (listener == null)
                keyCollection.Add(mouseListener);
            else
                keyCollection.Insert(keyCollection.IndexOf(listener), mouseListener);
        }
        public static void AddKeys(params MouseListener[] mouseListeners)
        {
            foreach (var kl in mouseListeners)
                AddKey(kl);
        }
        public static void AddSingle(MouseListener mouseListener)
        {
            MouseListener singleTriggerListener = new(mouseListener.Key, mouseListener.EventType, null, mouseListener.Order);

            void SelfRemove()
            {
                mouseListener.Action?.Invoke();
                RemoveKey(singleTriggerListener);
            };

            singleTriggerListener.Action = SelfRemove;
            AddKey(singleTriggerListener);
        }
        public static void AddSingle(MouseKey key, KeyEvent keyEvent, Action action = null, int order = 0)
        {
            AddSingle(new MouseListener(key, keyEvent, action, order));
        }

        public static void RemoveKey(MouseListener mouseKey)
        {
            if (mouseKey == null)
                throw new ArgumentNullException(nameof(mouseKey).ToString(), KeyListenerNullMessage);

            GetMouseCollection(mouseKey.EventType).Remove(mouseKey);
        }
        public static void MoveKey(MouseListener key, KeyEvent newEvent)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key).ToString(), KeyListenerNullMessage);

            RemoveKey(key);
            key.EventType = newEvent;
            AddKey(key);
        }

        public static bool IsKeyDown(MouseKey key) => IsMouseKeyDown(key);
        public static bool IsKeyUp(MouseKey key) => !IsMouseKeyDown(key);

        private static bool IsMouseKeyDown(MouseKey key)
        {
            return key switch
            {
                MouseKey.Left => MouseState.LeftButton == ButtonState.Pressed,
                MouseKey.Right => MouseState.RightButton == ButtonState.Pressed,
                MouseKey.Middle => MouseState.MiddleButton == ButtonState.Pressed,
                _ => false,
            };
        }
        private static LockList<MouseListener> GetMouseCollection(KeyEvent keyEvent)
        {
            return keyEvent switch
            {
                KeyEvent.Press => _pressKeysMouse,
                KeyEvent.Hold => _holdKeysMouse,
                KeyEvent.Release => _releaseKeysMouse,
                _ => throw new InvalidOperationException("Key collection not found.")
            };
        }

        #endregion
    }
}