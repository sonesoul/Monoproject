using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace InGame
{
    [DebuggerDisplay("{ToString(),nq}")]
    public readonly struct Combo
    {
        public string Sequence { get; init; } = "";
        public int Length => Sequence.Length;

        public Combo(IEnumerable<char> combo) => Sequence = new string(combo.ToArray());
        public Combo(string combo) => Sequence = combo;

        public bool Contains(char c) => Sequence.Contains(c, StringComparison.OrdinalIgnoreCase);
        public bool Contains(string str) => Sequence.Contains(str, StringComparison.OrdinalIgnoreCase);
        public bool StartsWith(string str) => Sequence.StartsWith(str, StringComparison.OrdinalIgnoreCase);

        public static Combo NewRandom(int length, char[] pattern = null)
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
        public static Combo NewRandom() => NewRandom(Level.FillerSize, Level.KeyPattern);
        public static bool operator ==(Combo left, Combo right) => left.Equals(right);
        public static bool operator !=(Combo left, Combo right) => left.Equals(right);
        
        public override bool Equals(object obj)
        {
            if (obj is not Combo combo)
                return false;
            
            return Sequence.Equals(combo.Sequence);
        }
        public override string ToString() => Sequence;
        public override int GetHashCode() => Sequence.GetHashCode();
    }
}