using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace GlobalTypes.Events
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

        public EventListener AddListener(Action<GameTime> sub, int order = 0)
        {
            EventListener listener = new(sub, order);
            AddListener(listener);
            return listener;
        }
       
        public void AddListener(EventListener listener)
        {
            _listeners.Add(listener);
            Sort();
        }
        public void RemoveListener(EventListener sub) => _listeners.Remove(sub);
        
        public void Sort() => _listeners.Sort((first, second) => first.order.CompareTo(second.order));
        public void Trigger(GameTime gt) => _listeners.ForEach(l => l.action(gt));

        public IEnumerator<EventListener> GetEnumerator() => _listeners.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}