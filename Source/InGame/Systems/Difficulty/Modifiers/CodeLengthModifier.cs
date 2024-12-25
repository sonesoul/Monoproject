using InGame.Interfaces;

namespace InGame.Difficulty.Modifiers
{
    public class CodeLengthModifier : IDifficultyUp
    {
        public string Message => $"Codes length {(Amount >= 0 ? $"+{Amount}" : Amount)}";

        private int Amount { get; } = 1;

        public CodeLengthModifier(int amount)
        {
            Amount = amount;
        }

        public void Apply() => LevelConfig.CodeSize += Amount;
        public void Cancel() => LevelConfig.CodeSize -= Amount;
    }
}