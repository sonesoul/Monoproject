using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace InGame.Pools
{
    public class IndexPool
    {
        public int MaxIndex { get; }

        private readonly Stack<int> stack = new();

        public IndexPool(int maxIndex)
        {
            if (maxIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(maxIndex));


            MaxIndex = maxIndex;
            Shuffle();
        }
        public IndexPool(ICollection collection) : this(collection.Count - 1) { }

        public int Pop()
        {
            int pop = stack.Pop();

            if (stack.Count < 1)
                Shuffle();

            return pop;
        }
        public int Peek() => stack.Peek();

        public void Shuffle()
        {
            List<int> indexes = Enumerable.Range(0, MaxIndex + 1).ToList();

            while (indexes.Count > 0)
            {
                int index = indexes.RandomElement();
                indexes.Remove(index);
                
                stack.Push(index);
            }
        }
    }
}