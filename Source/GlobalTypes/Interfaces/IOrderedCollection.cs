namespace GlobalTypes.Interfaces
{
    public interface IOrderedCollection<TValue, TItem>
    {
        public int FirstOrder { get; }
        public int LastOrder { get; }
        public TValue this[int index] { get; }

        public TItem Add(TValue value, int order);
        public void Add(TItem item);
        public TItem Append(TValue value) => Add(value, LastOrder + 1);
        public TItem Prepend(TValue value) => Add(value, FirstOrder - 1);

        public void Remove(TItem item);
        public void RemoveAt(int index);
        public void RemoveFirst(TValue item);
        public void RemoveLast(TValue item);
    }
}