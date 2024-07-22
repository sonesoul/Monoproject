using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;

namespace GlobalTypes.Events
{
    public static class GameEvents
    {
        public struct EventListener
        {
            public Action<GameTime> action;
            public int order;
            public EventListener(Action<GameTime> action, int order)
            {
                this.action = action;
                this.order = order;
            }
        }
        public class ListenerCollection : IEnumerable<EventListener>
        {
            private readonly List<EventListener> _items = new();
            public EventListener this[int index]
            {
                get => _items[index];
                set => _items[index] = value;
            }

            public void AddListener(Action<GameTime> sub, int order)
            {
                _items.Add(new(sub, order));
                Sort();
            }
            public void AddListener(Action<GameTime> sub)
            {
                _items.Add(new(sub, 0));
                Sort();
            }
            public void RemoveListener(EventListener sub) => _items.Remove(sub);
            public void RemoveListenerAt(int index) => _items.RemoveAt(index);

            public void Sort() => _items.Sort((first, second) => first.order.CompareTo(second.order));
            public void Trigger(GameTime gt)
            {
                foreach (var item in _items)
                    item.action(gt);
            }

            public IEnumerator<EventListener> GetEnumerator()
            {
                return _items.GetEnumerator();
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public readonly static ListenerCollection OnUpdate = new();
        public readonly static ListenerCollection OnBeforeDraw = new();
        public readonly static ListenerCollection OnAfterDraw = new();

        public static void Update(GameTime gt) => OnUpdate.Trigger(gt);
        public static void BeforeDraw(GameTime gt) => OnBeforeDraw.Trigger(gt);
        public static void AfterDraw(GameTime gt) => OnAfterDraw.Trigger(gt);
    }
}