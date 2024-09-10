namespace InGame.Interfaces
{
    public interface ITaggable
    {
        public string Tag { get; }
        public bool CompareTag(ITaggable other) => CompareTag(other.Tag);
        public bool CompareTag(string otherTag) => Tag.ToLower() == otherTag;
    }
}