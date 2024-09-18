namespace InGame.Interfaces
{
    public interface ITaggable
    {
        public string Tag { get; }
        public bool IsTagEqual(ITaggable other) => IsTagEqual(other.Tag);
        public bool IsTagEqual(string otherTag) => Tag.Equals(otherTag, System.StringComparison.OrdinalIgnoreCase);
    }
}