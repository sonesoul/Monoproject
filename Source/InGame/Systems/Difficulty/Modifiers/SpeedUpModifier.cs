using InGame.Interfaces;

namespace InGame.Difficulty.Modifiers
{
    public class SpeedUpModifier : IDifficultyUp
    {
        public string Message => $"{(LevelConfig.SpeedFactor < 5 ? "Move fast, baby" : "How unfortunate...")}";
        public bool IsForceApply => true;

        private float Amount => 1f;

        public void Apply()
        {
            LevelConfig.SpeedFactor += Amount;
        }

        public void Cancel()
        {
            LevelConfig.SpeedFactor -= Amount;
        }
    }
}
