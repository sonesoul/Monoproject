using System;
using System.Collections.Generic;
using System.Linq;

namespace InGame
{
    public readonly struct Combo
    {
        public string Sequence { get; init; } = "";
        public int Length => Sequence.Length;

        public Combo(IEnumerable<char> combo) => Sequence = new string(combo.ToArray());
        public Combo(string combo) => Sequence = combo;

        public bool Contains(char c) => Sequence.Contains(c, StringComparison.OrdinalIgnoreCase);
        public bool Contains(string str) => Sequence.Contains(str, StringComparison.OrdinalIgnoreCase);
        public static Combo Random(int length, char[] pattern = null)
        {
            List<char> sequence = new();
            pattern ??= Level.KeyPattern;

            while (length > 0)
            {
                sequence.Add(pattern.RandomElement());
                length--;
            }

            return new(sequence);
        }
    }
}
