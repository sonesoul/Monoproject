namespace InGame.Interfaces
{
    public interface ILevelObject : ITaggable
    {
        public bool IsInitialized { get; }
        void Init() { }
        void Terminate() { }
    }
}