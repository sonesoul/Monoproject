namespace InGame.Interfaces
{
    public interface IDifficultyModifier
    {
        public string Message { get; }
        public bool IsForceApply { get; }

        public void Apply();
        public void Cancel();
    }
}