using GlobalTypes.Events;
using Microsoft.Xna.Framework.Input;
using System;

namespace GlobalTypes.Input
{
    public class KeyListener : IHasOrderedAction<Action>
    {
        public Action Action { get; set; }
        public int Order { get; set; }

        public KeyEvent EventType { get; set; }
        public Keys Key { get; set; }

        public bool IsDown { get; set; } = false;
        public bool IsUp => !IsDown;
        public bool WasDown { get; set; } = false;

        public KeyListener(Keys key, KeyEvent keyEvent, Action action = null, int order = 0)
        {
            Key = key;
            EventType = keyEvent;
            Action = action;
            Order = order;
        }
    }
    public class MouseListener : IHasOrderedAction<Action>
    {
        public Action Action { get; set; }
        public int Order { get; set; }

        public KeyEvent EventType { get; set; }
        public MouseKey Key { get; set; }

        public bool IsDown { get; set; } = false;
        public bool IsUp => !IsDown;
        public bool WasDown { get; set; } = false;

        public MouseListener(MouseKey key, KeyEvent keyEvent, Action action = null, int order = 0)
        {
            Key = key;
            EventType = keyEvent;
            Action = action;
            Order = order;
        }
    }
}