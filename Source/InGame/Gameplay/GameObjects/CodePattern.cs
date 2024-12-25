using GlobalTypes.InputManagement;
using System;
using System.Linq;

namespace InGame.GameObjects
{
    public class CodePattern
    {
        public char[] Chars { get; set; }

        public CodePattern(string pattern)
        {
            Chars = pattern.ToCharArray();
        }
        public CodePattern(char[] pattern)
        {
            Chars = pattern;
        }

        public char RandomChar() => Chars.RandomElement();
        public Key RandomKey() => Enum.Parse<Key>(Chars.RandomElement().ToString());

        public bool Contains(Key key)
        {
            return Chars.Any(c => Enum.Parse<Key>(c.ToString(), true) == key);
        }
        public bool Contains(char c) => Chars.Contains(c);

        public override string ToString()
        {
            return string.Join("", Chars);
        }

        public override bool Equals(object obj)
        {
            if (obj is not CodePattern)
                return false;

            return Equals((CodePattern)obj);
        }
        private bool Equals(CodePattern other)
        {
            return Equals(other.Chars);
        }
        private bool Equals(char[] otherChars)
        {
            if (otherChars.Length != Chars.Length)
                return false;

            for (int i = 0; i < Chars.Length; i++)
            {
                if (Chars[i] != otherChars[i])
                    return false;
            }

            return true;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Chars);
        }

        public static bool operator ==(CodePattern left, CodePattern right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(CodePattern left, CodePattern right)
        {
            return !(left == right);
        }

        public static bool operator ==(CodePattern left, char[] right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(CodePattern left, char[] right)
        {
            return !(left == right);
        }
    }
}