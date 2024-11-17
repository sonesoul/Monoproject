using System;

namespace GlobalTypes.Events
{
    public class SingleTriggerEvent<T> : OrderedEventBase<OrderedAction<T>, Action<T>>
    {
        public void Trigger(T parameter)
        {
            _listeners.For(l => 
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
            _listeners.For(l =>
            {
                l.Action?.Invoke();

                _listeners.Remove(l);
            });
        }
    }
}