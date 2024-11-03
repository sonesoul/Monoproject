using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GlobalTypes.Collections
{
    public class LockCollection<T> : ILockCollection<T>
    {
        protected readonly ICollection<T> _collection;
        protected readonly ConcurrentQueue<Action> changeQueue = new();

        public bool IsLocked { get; private set; } = false;
        public int ChangeCount => changeQueue.Count;

        public int Count => _collection.Count;
        public bool IsReadOnly => _collection.IsReadOnly;

        private readonly object _lock = new();

        public LockCollection(ICollection<T> source) => _collection = source;
         
        public void Lock()
        {
            lock (_lock)
            {
                if (IsLocked)
                    throw new InvalidOperationException("Collection already locked.");

                IsLocked = true;
            }
        }
        public void Unlock()
        {
            lock (_lock)
            {
                if (!IsLocked)
                    throw new InvalidOperationException("Collection already unlocked.");

                IsLocked = false;
                ApplyChanges();
            }
        }
        
        private void ApplyChanges()
        {
            while (changeQueue.Count > 0)
            {
                if (changeQueue.TryDequeue(out var action))
                {
                    action?.Invoke();
                }
            }            
        }
        public void ClearChanges() => changeQueue.Clear();

        public void ForEach(Action<T> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            foreach (var item in _collection)
                action(item);
        }
        public void LockForEach(Action<T> action) => LockRun(() => ForEach(action));
        public void LockRun(Action action)
        {
            if(!IsLocked)
                Lock();

            action();
            
            if(IsLocked)
                Unlock();
        }
        
        public void SafeRun(Action action)
        {
            lock (_lock)
            {
                if (IsLocked)
                    changeQueue.Enqueue(action);
                else
                    action?.Invoke();
            }
        }

        public void Add(T value) => SafeRun(() => _collection.Add(value));
        public bool Remove(T value)
        {
            if (!Contains(value))
                return false;

            SafeRun(() => _collection.Remove(value));
            
            return true;
        }
        public void Clear() => SafeRun(() => _collection.Clear());
        public bool Contains(T item) => _collection.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => _collection.CopyTo(array, arrayIndex);

        public IEnumerator<T> GetEnumerator() => _collection.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    public class LockList<T> : LockCollection<T>, IList<T>, IReadOnlyList<T>
    {
        public int Capacity
        {
            get => InternalList.Capacity;
            set => InternalList.Capacity = value;
        }

        public IReadOnlyList<T> Collection => InternalList;
        private List<T> InternalList => (List<T>)_collection;

        public T this[int index] { get => InternalList[index]; set => InternalList[index] = value; }

        public LockList() : this(new List<T>()) { }
        public LockList(List<T> source) : base(new List<T>(source)) { }
        
        public int IndexOf(T item) => InternalList.IndexOf(item);
        public int LastIndexOf(T item) => InternalList.LastIndexOf(item);
        public void Insert(int index, T item)
        {
            SafeRun(() =>
            {
                if (index > InternalList.Count)
                    index = InternalList.Count - 1;

                InternalList.Insert(index, item);
            });  
        }
        public void RemoveAt(int index) => SafeRun(() => InternalList.Remove(InternalList[index]));

        public T Find(Predicate<T> match) => InternalList.Find(match);
        public List<T> FindAll(Predicate<T> match) => InternalList.FindAll(match);
        public void Sort(Comparison<T> comparison)
        {
            if (comparison == null)
                throw new ArgumentNullException(nameof(comparison));

            if (IsLocked)
            {
                changeQueue.Enqueue(() => InternalList.Sort(comparison));
                return;
            }
            
            InternalList.Sort(comparison);
        }
    }
}