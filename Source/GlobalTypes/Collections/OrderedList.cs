using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GlobalTypes.Collections
{
    public class OrderedList<T> : IOrderedCollection<T, OrderedItem<T>>, IEnumerable<T>
    {
        public int Count => _items.Count;
        public T this[int index] => _items[index].Value;

        public int LastOrder => Count > 0 ? _items[Count - 1].Order : 0;
        public int FirstOrder => Count > 0 ? _items[0].Order : 0;

        private readonly List<OrderedItem<T>> _items = new();

        public OrderedItem<T> Add(T item, int order)
        {
            OrderedItem<T> orderedItem = new(item, order);

            Add(orderedItem);

            return orderedItem;
        }
        public void Add(OrderedItem<T> orderedObj)
        {
            int index = FirstLarger(orderedObj.Order, _items.Select(i => i.Order).ToList());

            _items.Insert(index, orderedObj);
        }
        public OrderedItem<T> Append(T item) => Add(item, LastOrder + 1);
        public OrderedItem<T> Prepend(T item) => Add(item, FirstOrder - 1);

        public void Remove(OrderedItem<T> orderedObj) => _items.Remove(orderedObj);
        public void RemoveFirst(T item) => _items.Remove(_items.Find(m => m.Value.Equals(item)));
        public void RemoveLast(T item) => _items.Remove(_items.LastOrDefault());
        public void RemoveAt(int index) => _items.RemoveAt(index);
        public bool Contains(T item) => _items.Select(i => i.Value).Contains(item);

        public void Clear() => _items.Clear();

        public override string ToString()
        {
            return string.Join(", ", _items.Select(i => i.Value));
        }
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < _items.Count; i++)
            {
                yield return _items[i].Value;
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < _items.Count; i++)
            {
                yield return _items[i].Value;
            }
        }

        public static int FirstLarger(int order, List<int> orders)
        {
            int index = orders.BinarySearch(order);

            if (index >= 0)
            {
                while (index < orders.Count && orders[index] == order)
                {
                    index++;
                }
                return index;
            }
            else
            {
                return ~index;
            }
        }

    }
}