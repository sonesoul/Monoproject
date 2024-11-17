using GlobalTypes.Interfaces;
using System;

namespace GlobalTypes.Events
{
    public struct OrderedAction<T> : IHasOrderedAction<Action<T>>
    {
        public Action<T> Action { get; set; }
        public int Order { get; set; } = 0;

        public OrderedAction(Action<T> action, int order)
        {
            Action = action;
            Order = order;
        }

        public static OrderedAction<T> New(Action<T> action, int order) => new(action, order);
        public readonly override string ToString() => $"[{Order}] {Action.Method.DeclaringType.Name}.{Action.Method.Name}";
    }
    public struct OrderedAction : IHasOrderedAction<Action>
    {
        public Action Action { get; set; }
        public int Order { get; set; } = 0;

        public OrderedAction(Action action, int order)
        {
            Action = action;
            Order = order;
        }

        public static OrderedAction New(Action action, int order) => new(action, order);
        public readonly override string ToString() => $"[{Order}] {Action.Method.DeclaringType.Name}.{Action.Method.Name}";
    }
}