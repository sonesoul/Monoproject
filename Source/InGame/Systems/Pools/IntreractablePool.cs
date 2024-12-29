using InGame.GameObjects.SpecialObjects;
using InGame.Interfaces;
using System;
using System.Collections.Generic;

namespace InGame.Pools
{
    public static class IntreractablePool
    {
        private static List<Func<Vector2, IInteractable>> creators = new()
        {
            p => new RandomEffectObject(p)
        };

        private static IndexPool indexes = new(creators);


        public static IInteractable GetRandom(Vector2 position) => creators[indexes.Pop()](position); 
    }
}
