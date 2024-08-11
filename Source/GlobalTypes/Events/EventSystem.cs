using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using GlobalTypes.Collections;
using System.Linq;

namespace GlobalTypes.Events
{
    [DebuggerDisplay("{ToString(),nq}")]
    public struct EventListener
    {
        public Action<GameTime> action;
        public int order = 0;
        public EventListener(Action<GameTime> action, int order)
        {
            this.action = action;
            this.order = order;
        }

        public readonly override string ToString() => $"{action.Method.DeclaringType.Name}.{action.Method.Name}";
    }
    public class ListenerCollection : IEnumerable<EventListener>
    {
        public int LastOrder => _listeners.LastOrDefault().order;
        public int FirstOrder => _listeners.FirstOrDefault().order;

        private readonly LockList<EventListener> _listeners = new();
        private bool IsLocked => _listeners.IsLocked;

        public EventListener this[int index]
        {
            get => _listeners[index];
            set => _listeners[index] = value;
        }

        public EventListener AddListener(Action<GameTime> action, int order = 0)
        {
            if(action == null) throw new ArgumentNullException(nameof(action));

            EventListener listener = new(action, order);
            AddListener(listener);
            return listener;
        }
        public void AddListener(EventListener listener)
        {
            _listeners.Add(listener);
            Sort();
        }
        public void RemoveListener(EventListener listener) => _listeners.Remove(listener);
       
        public EventListener SetOrder(EventListener listener, int newOrder)
        {
            RemoveListener(listener);
            return AddListener(listener.action, newOrder);
        }
        public void Sort() => _listeners.Sort((first, second) => first.order.CompareTo(second.order));
        public void Trigger(GameTime gt)
        {
            if (IsLocked)
                throw new InvalidOperationException("Trying to trigger an event while it is iterating.");

            _listeners.Lock();
            _listeners.ForEach(l => l.action(gt));

            if (_listeners.ChangesCount > 0)
                Sort();

            _listeners.Unlock();
        }

        public IEnumerator<EventListener> GetEnumerator() => _listeners.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}