using InGame.Interfaces;
using InGame.Difficulty.Modifiers;
using System.Collections.Generic;
using System.Linq;
using System;

namespace InGame.Pools
{
    public static class ModifierPool
    {
        private static List<Func<IDifficultyModifier>> upCreators = new()
        {
            () => new OtherCharSetModifier(),

            () => new StorageCapacityModifier(LevelConfig.CodeLength / 2),
            () => new CodeLengthModifier(1),
        };
        private static List<Func<IDifficultyModifier>> downCreators = new()
        {
            () => new StorageCapacityModifier(-1),
            () => new TimeModifier(-10),
            () => new CodeLengthModifier(-1),
        };

        private static IndexPool upIndexes = new(upCreators);
        private static IndexPool downIndexes = new(downCreators);

        public static List<T> GetModifiers<T>() where T : class, IDifficultyModifier
        {
            List<IDifficultyModifier> modifiers = new();
            
            for (int i = 0; i < upCreators.Count; i++)
            {
                modifiers.Add(upCreators[i]());
            }
            for (int i = 0; i < downCreators.Count; i++)
            {
                modifiers.Add(downCreators[i]());
            }

            return modifiers.OfType<T>().ToList();
        }

        public static IDifficultyModifier GetRandomUp() => upCreators[upIndexes.Pop()]();
        public static IDifficultyModifier GetRandomDown() => downCreators[downIndexes.Pop()]();
    }
}