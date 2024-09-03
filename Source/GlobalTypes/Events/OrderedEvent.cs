using System;
using System.Diagnostics;

namespace GlobalTypes.Events
{
    [DebuggerDisplay("{ToString(),nq}")]
    public class OrderedEvent<T> : OrderedEventBase<EventListener<T>, Action<T>>
    {
        public void Trigger(T parameter)
        {
            if (IsLocked)
                throw new InvalidOperationException("Trying to trigger an event while it is iterating.");

            _listeners.LockForEach(l => l.Action?.Invoke(parameter));
        }

        public void AddSingle(EventListener<T> listener) => Add(ToSingleTrigger(listener));
        public void InsertSingle(Action<T> action, int order = 0)
        {
            EventListener<T> listener = default;
            listener.Order = order;
            listener.Action = action ?? throw new ArgumentNullException(nameof(action));

            Add(ToSingleTrigger(listener));
        }
        public void AppendSingle(Action<T> action) => InsertSingle(action, LastOrder + 1);
        public void PrependSingle(Action<T> action) => InsertSingle(action, FirstOrder - 1);
        
        private EventListener<T> ToSingleTrigger(EventListener<T> listener)
        {
            Action<T> action = listener.Action;

            void SelfRemove(T p)
            {
                action?.Invoke(p);
                _listeners.Remove(listener);
            }
            listener.Action = SelfRemove;
            return listener;
        }
    }

    [DebuggerDisplay("{ToString(),nq}")]
    public class OrderedEvent : OrderedEventBase<EventListener, Action>
    {
        public void Trigger()
        {
            if (IsLocked)
                throw new InvalidOperationException("Trying to trigger an event while it is iterating.");

            _listeners.LockForEach(l => l.Action?.Invoke());
        }

        public void AddSingle(EventListener listener) => Add(ToSingleTrigger(listener));
        public void InsertSingle(Action action, int order = 0)
        {
            EventListener listener = default;
            listener.Order = order;
            listener.Action = action ?? throw new ArgumentNullException(nameof(action));

            Add(ToSingleTrigger(listener));
        }
        public void AppendSingle(Action action) => InsertSingle(action, LastOrder + 1);
        public void PrependSingle(Action action) => InsertSingle(action, FirstOrder - 1);

        private EventListener ToSingleTrigger(EventListener listener)
        {
            Action action = listener.Action;

            void SelfRemove()
            {
                action?.Invoke();
                _listeners.Remove(listener);
            }
            listener.Action = SelfRemove;
            return listener;
        }
    }
}