using GlobalTypes.Interfaces;

namespace GlobalTypes.Collections
{
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
}
