using System;

namespace GlobalTypes.Events
{
    public class SingleTriggerEvent<T> : OrderedEventBase<EventListener<T>, Action<T>>
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

            _listeners.LockForEach(l =>
            {
                l.Action?.Invoke(parameter);
                _listeners.Remove(l);
            });
        }
    }
    public class SingleTriggerEvent : OrderedEventBase<EventListener, Action>
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

            _listeners.LockForEach(l =>
            {
                l.Action();
                _listeners.Remove(l);
            });
        }
    }
}