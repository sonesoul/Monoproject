using InGame.GameObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace InGame
{
    [DebuggerDisplay("{ToString(),nq}")]
    public readonly struct Code
    {
        public string Sequence { get; init; }
        public int Length => Sequence.Length;

        public char this[int index] => Sequence[index];

        public Code(IEnumerable<char> code) => Sequence = new string(code.ToArray());
        public Code(string seq) => Sequence = seq.ToUpper();

        public bool Contains(char c) => Sequence.Contains(c, StringComparison.OrdinalIgnoreCase);
        public bool Contains(string str) => Sequence.Contains(str, StringComparison.OrdinalIgnoreCase);
        public bool StartsWith(string str) => Sequence.StartsWith(str, StringComparison.OrdinalIgnoreCase);

        public static Code NewRandom(int length, CodePattern pattern = null)
        {
            List<char> sequence = new();
            pattern ??= LevelConfig.CodePattern;

            while (length > 0)
            {
                sequence.Add(pattern.RandomChar());
                length--;
            }
            
            return new(sequence);
        }
        public static Code NewRandom() => NewRandom(LevelConfig.CodeSize, LevelConfig.CodePattern);
        public static bool operator ==(Code left, Code right) => left.Equals(right);
        public static bool operator !=(Code left, Code right) => left.Equals(right);
        
        public override bool Equals(object obj)
        {
            if (obj is not Code code)
                return false;
            
            return Sequence.Equals(code.Sequence);
        }
        public override string ToString() => Sequence;
        public override int GetHashCode() => Sequence.GetHashCode();
    }
}