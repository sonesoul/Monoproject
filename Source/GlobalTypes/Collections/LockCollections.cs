using System;
using System.Collections;
using System.Collections.Generic;

namespace GlobalTypes.Collections
{
    public interface ILockCollection<T> : ICollection<T>
    {
        public bool IsLocked { get; }
        public int ChangesCount { get; }

        public void Lock();
        public void Unlock();
        protected void ApplyChanges() { }
    }

    public class LockCollection<T> : ILockCollection<T>
    {
        protected enum ChangeType
        {
            Add,
            Remove,
            Clear,
        }
        protected readonly struct CollectionChange
        {
            public readonly T Value { get; init; }
            public readonly ChangeType Change { get; init; }

            public CollectionChange(T value, ChangeType type)
            {
                Value = value;
                Change = type;
            }

            public readonly void Deconstruct(out T value, out ChangeType type)
            {
                value = Value; 
                type = Change;
            }
        }

        protected readonly ICollection<T> _collection;
        private readonly Queue<CollectionChange> _changeQueue = new();

        public bool IsLocked { get; private set; } = false;
        public int ChangesCount => _changeQueue.Count;

        public int Count => _collection.Count;
        public bool IsReadOnly => _collection.IsReadOnly;

        public LockCollection(ICollection<T> original) => _collection = original;

        public void Lock()
        {
            if (IsLocked)
                throw new InvalidOperationException("Collection already locked.");

            IsLocked = true;
        }
        public void Unlock()
        {
            if (!IsLocked)
                throw new InvalidOperationException("Collection already unlocked.");

            IsLocked = false;
            ApplyChanges();
        }

        protected virtual void ApplyChanges()
        {
            while (_changeQueue.Count > 0)
            {
                _changeQueue.Dequeue().Deconstruct(out var value, out var type);

                switch (type)
                {
                    case ChangeType.Add:
                        Add(value);
                        break;
                    case ChangeType.Remove:
                        Remove(value);
                        break;
                    case ChangeType.Clear:
                        Clear();
                        break;
                    default:
                        break;
                }
            }
        }

        public void Add(T value)
        {
            if (IsLocked)
            {
                _changeQueue.Enqueue(new(value, ChangeType.Add));
                return;
            }

            _collection.Add(value);
        }
        public bool Remove(T value)
        {
            if (IsLocked)
            {
                _changeQueue.Enqueue(new(value, ChangeType.Remove));
                return true;
            }

            return _collection.Remove(value);
        }
        public void Clear()
        {
            if(IsLocked)
            {
                _changeQueue.Enqueue(new(default, ChangeType.Clear));
                return;
            }

            _collection.Clear();
        }
        public bool Contains(T item) => _collection.Contains(item);
        public void ForEach(Action<T> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            foreach (var item in _collection)
                action(item);
        }
        public void LockForEach(Action<T> action)
        {
            Lock();
            ForEach(action);
            Unlock();
        }

        public void CopyTo(T[] array, int arrayIndex) => _collection.CopyTo(array, arrayIndex);

        public IEnumerator<T> GetEnumerator() => _collection.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    public class LockList<T> : LockCollection<T>, IList<T>
    {
        new protected enum ChangeType
        {
            Insert,
            RemoveAt,
            Sort
        }
        public LockList() : base(new List<T>()) { }
        public LockList(List<T> source) : base(new List<T>(source)) { }

        private readonly Queue<ListChange> _changeQueue = new();
        private List<T> InternalList => (List<T>)_collection;

        public int Capacity
        {
            get => InternalList.Capacity;
            set
            {
                if (value < InternalList.Count)
                    throw new ArgumentOutOfRangeException(nameof(value), "Capacity cannot be less than the current count.");
                InternalList.Capacity = value;
            }
        }

        public T this[int index] { get => InternalList[index]; set => InternalList[index] = value; }

        protected override void ApplyChanges()
        {
            base.ApplyChanges();

            while (_changeQueue.Count > 0)
            {
                var change = _changeQueue.Dequeue();
                switch (change.Change)
                {
                    case ChangeType.Insert:
                        InternalList.Insert(change.Index, change.Value);
                        break;
                    
                    case ChangeType.RemoveAt:
                        InternalList.RemoveAt(change.Index);
                        break;

                    case ChangeType.Sort:
                        if(change.Comparison != null)
                            InternalList.Sort(change.Comparison);
                        break;
                }
            }
        }

        public int IndexOf(T item) => InternalList.IndexOf(item);
        public int LastIndexOf(T item) => InternalList.LastIndexOf(item);
        public void Insert(int index, T item)
        {
            if (IsLocked)
            {
                _changeQueue.Enqueue(new ListChange(item, ChangeType.Insert) { Index = index });
                return;
            }
            
            InternalList.Insert(index, item);
        }
        public void RemoveAt(int index)
        {
            if (IsLocked)
            {
                T item = InternalList[index];
                _changeQueue.Enqueue(new ListChange(item, ChangeType.RemoveAt) { Index = index });
                return;
            }
            
            InternalList.RemoveAt(index);
        }
        public T Find(Predicate<T> match) => InternalList.Find(match);
        public List<T> FindAll(Predicate<T> match) => InternalList.FindAll(match);
        public void Sort(Comparison<T> comparison)
        {
            if (comparison == null)
                throw new ArgumentNullException(nameof(comparison));

            if (IsLocked)
            {
                _changeQueue.Enqueue(new(default, ChangeType.Sort) { Comparison = comparison });
                return;
            }
            
            InternalList.Sort(comparison);
        }

        private readonly struct ListChange
        {
            public readonly T Value { get; init; }
            public readonly ChangeType Change { get; init; }
            public int Index { get; init; }
            public Comparison<T> Comparison { get; init; }

            public ListChange(T value, ChangeType type)
            {
                Value = value;
                Change = type;
                Index = -1;
                Comparison = null;
            }
        }
    }
}