using InGame.GameObjects;
using InGame.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace InGame.Difficulty.Modifiers
{
    public class OtherPatternModifier : IDifficultyUp
    {
        public string Message { get; private set; }

        private CodePattern pattern;
        private CodePattern previousPattern;

        private static List<CodePattern> Patterns { get; } = new()
        {
            new("1234"), 
            new("WASD"),
            new("ZXCV"),
            new("QWER"),
        };

        public OtherPatternModifier()
        {
            previousPattern = LevelConfig.CodePattern;

            pattern = Patterns.Where(p => p != previousPattern).RandomElement();
            Message = $"Codes will use {pattern} pattern!";
        }
        public void Apply() => LevelConfig.CodePattern = pattern;
        public void Cancel() => LevelConfig.CodePattern = previousPattern;
    }
}