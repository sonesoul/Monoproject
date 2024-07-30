using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace GlobalTypes.Events
{
    public static class GameEvents
    {
        [DebuggerDisplay("{ToString(),nq}")]
        public struct EventListener
        {
            public Action<GameTime> action;
            public int order;
            public EventListener(Action<GameTime> action, int order)
            {
                this.action = action;
                this.order = order;
            }

            public readonly override string ToString() => $"{action.Method.DeclaringType.Name}.{action.Method.Name}";
        }
        public class ListenerCollection : IEnumerable<EventListener>
        {
            private readonly List<EventListener> _listeners = new();
            public EventListener this[int index]
            {
                get => _listeners[index];
                set => _listeners[index] = value;
            }

            public void AddListener(Action<GameTime> sub, int order)
            {
                _listeners.Add(new(sub, order));
                Sort();
            }
            public void AddListener(Action<GameTime> sub)
            {
                _listeners.Add(new(sub, 0));
                Sort();
            }
            public void RemoveListener(EventListener sub) => _listeners.Remove(sub);
            public void RemoveListenerAt(int index) => _listeners.RemoveAt(index);

            public void Sort() => _listeners.Sort((first, second) => first.order.CompareTo(second.order));
            public void Trigger(GameTime gt) => _listeners.ForEach(l => l.action(gt));

            public IEnumerator<EventListener> GetEnumerator() => _listeners.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public readonly static ListenerCollection OnUpdate = new();
        public readonly static ListenerCollection OnBeforeDraw = new();
        public readonly static ListenerCollection OnAfterDraw = new();
    }
}