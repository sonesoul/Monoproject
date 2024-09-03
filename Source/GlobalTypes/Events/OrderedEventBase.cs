using System;
using System.Collections.Generic;
using GlobalTypes.Collections;
using System.Linq;
using System.Diagnostics;

namespace GlobalTypes.Events
{
    public interface IHasOrderedAction<TAction>
    {
        public int Order { get; set; }
        public TAction Action { get; set; }
    }

    public struct EventListener<T> : IHasOrderedAction<Action<T>>
    {
        public Action<T> Action { get; set; }
        public int Order { get; set; } = 0;

        public EventListener(Action<T> action, int order)
        {
            this.Action = action;
            Order = order;
        }

        public readonly override string ToString() => $"[{Order}] {Action.Method.DeclaringType.Name}.{Action.Method.Name}";
    }
    public struct EventListener : IHasOrderedAction<Action>
    {
        public Action Action { get; set; }
        public int Order { get; set; } = 0;

        public EventListener(Action action, int order)
        {
            this.Action = action;
            Order = order;
        }

        public readonly override string ToString() => $"[{Order}] {Action.Method.DeclaringType.Name}.{Action.Method.Name}";
    }

    public abstract class OrderedEventBase<TListener, TAction>
        where TListener : struct, IHasOrderedAction<TAction> 
        where TAction : Delegate
    {
        public int LastOrder => Count > 0 ? _listeners[Count - 1].Order : 0;
        public int FirstOrder => Count > 0 ? _listeners[0].Order : 0;
        protected bool IsLocked => _listeners.IsLocked;
        public int Count => _listeners.Count;

        public IReadOnlyList<TListener> Listeners => _listeners.Collection;

        protected readonly LockList<TListener> _listeners = new();

        public TListener this[int index]
        {
            get => _listeners[index];
            set => _listeners[index] = value;
        }

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
        public TListener Insert(TAction action, int order = 0)
        {
            TListener listener = default;
            listener.Order = order;
            listener.Action = action ?? throw new ArgumentNullException(nameof(action));

            Add(listener);
            return listener;
        }
        public TListener Append(TAction action) => Insert(action, LastOrder + 1);
        public TListener Prepend(TAction action) => Insert(action, FirstOrder - 1);

        public void Remove(TListener listener) => _listeners.Remove(listener);
        public void RemoveFirst(TAction method)
        {
            var found = _listeners.Find(l => l.Action == method);

            if(found.Action != null)
                _listeners.Remove(found);
        }
        public void RemoveLast(TAction method)
        {
            var found = _listeners.FindAll(l => l.Action == method).LastOrDefault();

            if (found.Action != null)
                _listeners.Remove(found);
        }

        public TListener SetOrder(TListener listener, int newOrder)
        {
            Remove(listener);
            return Insert(listener.Action, newOrder);
        }

        public int GetFirstLargerOrder(int requiredOrder)
        {
            TListener item = default;
            item.Order = requiredOrder;

            int index = _listeners.ToList().BinarySearch(
                item,
                Comparer<TListener>.Create((x, y) => x.Order.CompareTo(y.Order)));

            if (index < 0)
                index = ~index;

            if (index >= _listeners.Count || _listeners[index].Order < requiredOrder)
                return -1;

            return index;
        }

        public override string ToString() => $"<{_listeners.Count}>  [{FirstOrder}] ... [{LastOrder}]";
    }
}