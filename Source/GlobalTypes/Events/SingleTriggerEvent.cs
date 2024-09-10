using System;

namespace GlobalTypes.Events
{
    public class SingleTriggerEvent<T> : OrderedEventBase<OrderedAction<T>, Action<T>>
    {
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
    public class SingleTriggerEvent : OrderedEventBase<OrderedAction, Action>
    {
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