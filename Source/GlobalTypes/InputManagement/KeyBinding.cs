using GlobalTypes.Interfaces;
using System;

namespace GlobalTypes.InputManagement
{
    public class KeyBinding : IHasOrderedAction<Action>
    {
        public Action Action { get; set; }
        public int Order { get; set; }

        public KeyPhase TriggerPhase { get; set; }
        public Key Key { get; set; }

        public bool IsDown { get; private set; }
        private bool WasDown { get; set; } = false;

        public KeyBinding(Key key, KeyPhase keyPhase, Action action = null, int order = 0)
        {
            Key = key;
            TriggerPhase = keyPhase;
            Action = action;
            Order = order;
        }

        public void Update()
        {
            bool isDown = IsDown = Input.IsKeyDown(Key);

            if (TriggerPhase == KeyPhase.Press)
            {
                if (!WasDown && isDown)
                    Action?.Invoke();
            }
            else if (TriggerPhase == KeyPhase.Hold) 
            {
                if (WasDown && isDown)
                    Action?.Invoke();
            }
            else if (TriggerPhase == KeyPhase.Release)
            {
                if (WasDown && !isDown)
                    Action?.Invoke();
            }

            WasDown = isDown;
        }
    }
}