using InGame.Interfaces;

namespace InGame.Difficulty.Modifiers
{
    public class ShorterCodesModifier : IDifficultyUp
    {
        public string Message => "Codes will be longer";

        private int Amount { get; } = 1;

        public void Apply() => LevelConfig.CodeLength -= Amount;
        public void Cancel() => LevelConfig.CodeLength += Amount;
    }
}