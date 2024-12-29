using GlobalTypes.InputManagement;
using System;
using System.Linq;

namespace InGame.GameObjects
{
    public class CodePattern
    {
        public string CharSet { get; set; }
        public int CharCount => CharSet.Length;
        public int Length { get; set; }

        public CodePattern(string set) : this(set, set.Length) { }
        public CodePattern(string set, int length)
        {
            if (!set.HasContent())
                throw new ArgumentException("Char set doesn't have any content.");

            CharSet = new(set.ToUpper().Where(c => char.IsLetterOrDigit(c)).Distinct().ToArray());
            Length = length;
        }

        public char RandomChar() => CharSet.RandomElement();
        public Key RandomKey() => Enum.Parse<Key>(RandomChar().ToString());

        public bool Contains(Key key)
        {
            return CharSet.Any(c => Enum.Parse<Key>(c.ToString(), true) == key);
        }
        public bool Contains(char c) => CharSet.Contains(c);

        public override string ToString()
        {
            return string.Join("", CharSet);
        }

        public override bool Equals(object obj)
        {
            if (obj is not CodePattern)
                return false;

            return Equals((CodePattern)obj);
        }
        private bool Equals(CodePattern other) => Equals(other.CharSet) && other.Length == Length;
        private bool Equals(string otherChars) => CharSet == otherChars;
        public override int GetHashCode() => HashCode.Combine(CharSet, Length);

        public static bool operator ==(CodePattern left, CodePattern right) => left.Equals(right);
        public static bool operator !=(CodePattern left, CodePattern right) => !(left == right);

        public static bool operator ==(CodePattern left, string right) => left.Equals(right);
        public static bool operator !=(CodePattern left, string right) => !(left == right);
    }
}