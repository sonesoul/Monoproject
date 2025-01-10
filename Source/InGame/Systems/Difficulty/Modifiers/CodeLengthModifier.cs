using InGame.Interfaces;

namespace InGame.Difficulty.Modifiers
{
    public class CodeLengthModifier : IDifficultyUp
    {
        public string Message => $"Codes length {Amount.AsDifference(0)}";
        public bool IsForceApply => false;

        private int Amount { get; } = 1;

        public CodeLengthModifier(int amount)
        {
            Amount = amount;
        }

        public void Apply() => LevelConfig.CodeLength += Amount;
        public void Cancel() => LevelConfig.CodeLength -= Amount;
    }
}