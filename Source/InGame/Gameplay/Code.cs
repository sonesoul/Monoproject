using InGame.GameObjects;
using System;
using System.Diagnostics;
using System.Text;

namespace InGame
{
    [DebuggerDisplay("{ToString(),nq}")]
    public readonly struct Code
    {
        public string Sequence { get; }
        public int Length => Sequence.Length;

        public char this[int index] => Sequence[index];

        public Code(string seq) => Sequence = seq.ToUpper();

        public bool Contains(char c) => Sequence.Contains(c, StringComparison.OrdinalIgnoreCase);
        public bool Contains(string str) => Sequence.Contains(str, StringComparison.OrdinalIgnoreCase);
        public bool StartsWith(string str) => Sequence.StartsWith(str, StringComparison.OrdinalIgnoreCase);

        public static Code NewRandom(CodePattern pattern = null)
        {
            StringBuilder sb = new();
            pattern ??= LevelConfig.CodePattern;
            int length = pattern.Length;

            while (length > 0)
            {
                sb.Append(pattern.RandomChar());
                length--;
            }
            
            return new(sb.ToString());
        }

        public override bool Equals(object obj)
        {
            if (obj is not Code code)
                return false;

            return Sequence.Equals(code.Sequence);
        }
        public override string ToString() => Sequence;
        public override int GetHashCode() => HashCode.Combine(Sequence, Sequence.Length);

        public static bool operator ==(Code left, Code right) => left.Equals(right);
        public static bool operator !=(Code left, Code right) => !(left == right);
    }
}