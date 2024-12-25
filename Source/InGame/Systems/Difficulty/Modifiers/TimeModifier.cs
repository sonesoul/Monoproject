using InGame.Interfaces;

namespace InGame.Difficulty.Modifiers
{
    public class TimeModifier : IDifficultyUp
    {
        public string Message => "Tick-tack...";

        private int Amount { get; } = 10;

        public TimeModifier(int amount)
        {
            Amount = amount;
        }

        public void Apply() => LevelConfig.TimeSeconds -= Amount;
        public void Cancel() => LevelConfig.TimeSeconds += Amount;
    }
}