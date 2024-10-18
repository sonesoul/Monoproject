using GlobalTypes.Interfaces;
using System.Collections.Generic;
using System;

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

        public readonly override bool Equals(object obj)
        {
            if (obj is OrderedItem<T> other)
            {
                return EqualityComparer<T>.Default.Equals(Value, other.Value) && Order == other.Order;
            }
            return false;
        }

        public readonly override int GetHashCode() => HashCode.Combine(Value, Order);

        public static bool operator ==(OrderedItem<T> left, OrderedItem<T> right) => left.Equals(right);

        public static bool operator !=(OrderedItem<T> left, OrderedItem<T> right) => !(left == right);
    }
}
