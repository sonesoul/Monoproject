using InGame.GameObjects;

namespace InGame
{
    public static class LevelConfig
    {
        public static CodePattern CodePattern { get; set; }
        public static int MapIndex { get; set; }

        public static int CodeLength { get => CodePattern.Length; set => CodePattern.Length = value.ClampMin(1); }
        public static int StorageCapacity { get => _storageCapacity; set => _storageCapacity = value.ClampMin(1); }

        public static int TaskDifficulty { get => _taskDifficulty; set => _taskDifficulty = value.ClampMin(1); }
        public static float SpeedFactor { get => _speedFactor; set => _speedFactor = value.Clamp(1, 5); }

        private static float _speedFactor;
        private static int _taskDifficulty;
        private static int _storageCapacity;

        static LevelConfig() => Reset();

        public static void Reset()
        {
            CodePattern = new("QWER", 5);
            StorageCapacity = CodeLength * 3;
            MapIndex = 1;
            SpeedFactor = 1f;
            TaskDifficulty = 2;
        }
    }
}