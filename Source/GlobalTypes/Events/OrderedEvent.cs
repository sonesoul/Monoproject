using System;
using System.Diagnostics;

namespace GlobalTypes.Events
{
    [DebuggerDisplay("{ToString(),nq}")]
    public class OrderedEvent<T> : OrderedEventBase<EventListener<T>, Action<T>>
    {
        public event Action<T> Triggered
        {
            add => Append(new(value));
            remove => RemoveFirst(value);
        }

        public void Trigger(T parameter)
        {
            if (IsLocked)
                throw new InvalidOperationException("Trying to trigger an event while it is iterating.");

            _listeners.LockForEach(l => l.Action?.Invoke(parameter));
        }
    }

    [DebuggerDisplay("{ToString(),nq}")]
    public class OrderedEvent : OrderedEventBase<EventListener, Action>
    {
        public event Action Triggered
        {
            add => Append(new(value));
            remove => RemoveFirst(value);
        }

        public void Trigger()
        {
            if (IsLocked)
                throw new InvalidOperationException("Trying to trigger an event while it is iterating.");

            _listeners.LockForEach(l => l.Action?.Invoke());
        }
    }
}