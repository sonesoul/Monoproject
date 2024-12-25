using InGame.GameObjects;

namespace InGame
{
    public static class LevelConfig
    {
        public static CodePattern CodePattern { get; set; }
        public static int MapIndex { get; set; }

        public static int CodeSize 
        {
            get => _codeSize; 
            set => _codeSize = value.ClampMin(1); 
        }
        public static int StorageCapacity 
        {
            get => _storageSize; 
            set => _storageSize = value.ClampMin(1);
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
        private static int _storageSize;
        private static int _codeSize;

        static LevelConfig() => Reset();

        public static void Reset()
        {
            CodePattern = new("QWER");
            CodeSize = 4;
            StorageCapacity = 3;
            MapIndex = 1;
            TimeSeconds = 3;
            TaskDifficulty = 2;
        }
    }
}