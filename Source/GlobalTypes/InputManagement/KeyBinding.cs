using GlobalTypes.Events;
using System;

namespace GlobalTypes.InputManagement
{
    public class KeyBinding : IHasOrderedAction<Action>
    {
        public Action Action { get; set; }
        public int Order { get; set; }

        public KeyPhase TriggerPhase { get; set; }
        public Key Key { get; set; }
        
        public bool IsDown { get; set; } = false;
        public bool IsUp => !IsDown;
        public bool WasDown { get; set; } = false;

        public KeyBinding(Key key, KeyPhase keyPhase, Action action = null, int order = 0)
        {
            Key = key;
            TriggerPhase = keyPhase;
            Action = action;
            Order = order;
        }
    }
}