namespace InGame.Interfaces
{
    public interface IDifficultyModifier
    {
        public string Message { get; }

        public void Apply();
        public void Cancel();
    }
}