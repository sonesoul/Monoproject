using System;
using System.Collections.Generic;
using System.Linq;
using GlobalTypes.Interfaces;

namespace GlobalTypes.Events
{
    public abstract class OrderedEventBase<TListener, TAction> : IOrderedCollection<TAction, TListener>
        where TListener : IHasOrderedAction<TAction> 
        where TAction : Delegate
    {
        public int LastOrder => Count > 0 ? _listeners[Count - 1].Order : 0;
        public int FirstOrder => Count > 0 ? _listeners[0].Order : 0;
        public int Count => _listeners.Count;
       
        public IReadOnlyList<TListener> Listeners => _listeners;

        protected readonly List<TListener> _listeners = new();

        public TAction this[int index] => _listeners[index].Action;

        public void Add(TListener listener)
        {
            int requiredOrder = listener.Order;

            if (Count == 0 || LastOrder <= requiredOrder)
            {
                _listeners.Add(listener);
                return;
            }

            int index = GetFirstLargerOrder(requiredOrder);

            if (index == -1)
                throw new InvalidOperationException("Unable to insert listener, no valid insertion point found.");
            else
                _listeners.Insert(index, listener);
        }
        public TListener Add(TAction action, int order = 0)
        {
            if (action == null) 
                throw new ArgumentNullException(nameof(action));

            TListener listener = CreateNew(action, order);

            Add(listener);
            return listener;
        }
        public TListener Append(TAction action) => Add(action, LastOrder + 1);
        public TListener Prepend(TAction action) => Add(action, FirstOrder - 1);

        public void Remove(TListener listener) => _listeners.Remove(listener);
        public void RemoveAt(int index) => _listeners.Remove(_listeners[index]);
        public void RemoveFirst(TAction action)
        {
            var found = _listeners.Find(l => l.Action == action);

            if(found.Action != null)
                _listeners.Remove(found);
        }
        public void RemoveLast(TAction action)
        {
            var found = _listeners.FindAll(l => l.Action == action).LastOrDefault();

            if (found.Action != null)
                _listeners.Remove(found);
        }
        
        public TListener SetOrder(TListener listener, int newOrder)
        {
            Remove(listener);
            return Add(listener.Action, newOrder);
        }

        public int GetFirstLargerOrder(int requiredOrder)
        {
            TListener item = CreateNew(null, requiredOrder);

            int index = _listeners.ToList().BinarySearch(
                item,
                Comparer<TListener>.Create((x, y) => x.Order.CompareTo(y.Order)));

            if (index < 0)
                index = ~index;

            if (index >= _listeners.Count || _listeners[index].Order < requiredOrder)
                return -1;

            return index;
        }

        private static TListener CreateNew(TAction action, int order)
        {
            return (TListener)typeof(TListener)
                .GetMethod("New", new[] { typeof(TAction), typeof(int) })
                .Invoke(null, new object[] { action, order });
        }

        public override string ToString() => $"<{_listeners.Count}>  [{FirstOrder}] ... [{LastOrder}]";
    }
}