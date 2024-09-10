using System;

namespace GlobalTypes
{
    public interface IOrderable
    {
        int Order { get; set; }
    }
    public interface IHasOrderedAction<TAction> : IOrderable
    {
        public TAction Action { get; set; }
    }

    public struct OrderedItem<T> : IOrderable
    {
        public readonly T Value { get; init; }
        public int Order { get; set; }

        public OrderedItem(T value, int order)
        {
            Value = value;
            Order = order;
        }
    }
    public struct OrderedAction<T> : IHasOrderedAction<Action<T>>
    {
        public Action<T> Action { get; set; }
        public int Order { get; set; } = 0;

        public OrderedAction(Action<T> action, int order)
        {
            this.Action = action;
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
            this.Action = action;
            Order = order;
        }

        public static OrderedAction New(Action action, int order) => new(action, order);
        public readonly override string ToString() => $"[{Order}] {Action.Method.DeclaringType.Name}.{Action.Method.Name}";
    }
}