using InGame.GameObjects;

namespace InGame
{
    public static class LevelConfig
    {
        public static CodePattern CodePattern { get; set; }
        public static int MapIndex { get; set; }

        public static int CodeLength 
        {
            get => CodePattern.Length; 
            set => CodePattern.Length = value.ClampMin(1); 
        }
        public static int StorageCapacity 
        {
            get => _storageCapacity; 
            set => _storageCapacity = value.ClampMin(1);
        }

        public static int TaskDifficulty 
        {
            get => _taskDifficulty; 
            set => _taskDifficulty = value.ClampMin(1);
        }
        public static float TimeSeconds 
        {
            get => _timeSeconds; 
            set => _timeSeconds = value.Clamp(1, 61);
        }

        private static float _timeSeconds;
        private static int _taskDifficulty;
        private static int _storageCapacity;

        static LevelConfig() => Reset();

        public static void Reset()
        {
            CodePattern = new("QWER", 5);
            StorageCapacity = CodeLength * 3;
            MapIndex = 1;
            TimeSeconds = 30;
            TaskDifficulty = 2;
        }
    }
}