using InGame.GameObjects;
using InGame.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace InGame.Difficulty.Modifiers
{
    public class OtherCharSetModifier : IDifficultyUp
    {
        public string Message { get; private set; }

        private string set;
        private string previousSet;

        private static List<string> Sets { get; } = new()
        {
            "1234", 
            "WASD",
            "ZXCV",
            "QWER",
        };

        public OtherCharSetModifier()
        {
            previousSet = LevelConfig.CodePattern.CharSet;

            set = Sets.Where(p => p != previousSet).RandomElement();
            Message = $"New char set - {set}";
        }

        public void Apply() => LevelConfig.CodePattern.CharSet = set;
        public void Cancel() => LevelConfig.CodePattern.CharSet = previousSet;
    }
}