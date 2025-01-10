using InGame.Interfaces;

namespace InGame.Difficulty.Modifiers
{
    public class StorageCapacityModifier : IDifficultyUp
    {
        public string Message => $"Storage capacity {Amount.AsDifference(0)}";
        public bool IsForceApply => false;

        private int Amount { get; } = 1;

        public StorageCapacityModifier(int amount)
        {
            Amount = amount;
        }

        public void Apply() => LevelConfig.StorageCapacity += Amount;
        public void Cancel() => LevelConfig.StorageCapacity -= Amount;
    }
}