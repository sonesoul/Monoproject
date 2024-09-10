using System;
using System.Collections;
using System.Collections.Generic;

namespace GlobalTypes.Collections
{
    public class LockCollection<T> : ILockCollection<T>
    {
        protected readonly ICollection<T> _collection;
      
        protected readonly Queue<Action> changeQueue = new();

        public bool IsLocked { get; private set; } = false;
        public int ChangeCount => changeQueue.Count;

        public int Count => _collection.Count;
        public bool IsReadOnly => _collection.IsReadOnly;

        public LockCollection(ICollection<T> source) => _collection = source;

        /// <summary>
        /// Locks the collection for a changes. Changes made while locked are applied when the collection is unlocked.
        /// </summary>
        public void Lock()
        {
            if (IsLocked)
                throw new InvalidOperationException("Collection already locked.");

            IsLocked = true;
        }
        /// <summary>
        /// Unlocks the collection. Applies changes that made while collection was locked.
        /// </summary>
        public void Unlock()
        {
            if (!IsLocked)
                throw new InvalidOperationException("Collection already unlocked.");

            IsLocked = false;
            ApplyChanges();
        }
        
        public void SafeUnlock() => IsLocked = false;
        public void SafeLock() => IsLocked = true;
        public void SetLock(bool state) => IsLocked = state;

        private void ApplyChanges()
        {
            while (changeQueue.Count > 0)
                changeQueue.Dequeue()();
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
            if(IsLocked)
                changeQueue.Enqueue(action);
            else 
                action?.Invoke();
        }

        public void Add(T value) => SafeRun(() => _collection.Add(value));
        public bool Remove(T value)
        {
            if (!Contains(value))
                return false;
            else
            {
                if (IsLocked)
                    changeQueue.Enqueue(() => _collection.Remove(value));
                else
                    _collection.Remove(value);

                return true;
            }
           
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
            set
            {
                if (value < InternalList.Count)
                    throw new ArgumentOutOfRangeException(nameof(value), "Capacity cannot be less than the current count.");
                InternalList.Capacity = value;
            }
        }

        public IReadOnlyList<T> Collection => InternalList;
        private List<T> InternalList => (List<T>)_collection;

        public T this[int index] { get => InternalList[index]; set => InternalList[index] = value; }

        public LockList() : this(new List<T>()) { }
        public LockList(List<T> source) : base(new List<T>(source)) { }
        
        public int IndexOf(T item) => InternalList.IndexOf(item);
        public int LastIndexOf(T item) => InternalList.LastIndexOf(item);
        public void Insert(int index, T item) => SafeRun(() => InternalList.Insert(index, item));
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