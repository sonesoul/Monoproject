using System;
using System.Diagnostics;

namespace GlobalTypes.Events
{
    [DebuggerDisplay("{ToString(),nq}")]
    public class OrderedEvent<T> : OrderedEventBase<OrderedAction<T>, Action<T>>
    {
        public void Trigger(T parameter)
        {
            _listeners.For(l => l.Action?.Invoke(parameter));
        }

        public void AddSingle(OrderedAction<T> listener) => Add(ToSingleTrigger(listener));
        public void AddSingle(Action<T> action, int order = 0)
        {
            OrderedAction<T> listener = default;
            listener.Order = order;
            listener.Action = action ?? throw new ArgumentNullException(nameof(action));

            Add(ToSingleTrigger(listener));
        }
        public void AppendSingle(Action<T> action) => AddSingle(action, LastOrder + 1);
        public void PrependSingle(Action<T> action) => AddSingle(action, FirstOrder - 1);
        
        private OrderedAction<T> ToSingleTrigger(OrderedAction<T> listener)
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
    public class OrderedEvent : OrderedEventBase<OrderedAction, Action>
    {
        public void Trigger()
        {
            _listeners.For(l => l.Action?.Invoke());
        }

        public void AddSingle(OrderedAction listener) => Add(ToSingleTrigger(listener));
        public void AddSingle(Action action, int order = 0)
        {
            OrderedAction listener = default;
            listener.Order = order;
            listener.Action = action ?? throw new ArgumentNullException(nameof(action));

            Add(ToSingleTrigger(listener));
        }
        public void AppendSingle(Action action) => AddSingle(action, LastOrder + 1);
        public void PrependSingle(Action action) => AddSingle(action, FirstOrder - 1);

        private OrderedAction ToSingleTrigger(OrderedAction listener)
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