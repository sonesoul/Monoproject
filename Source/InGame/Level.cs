using GlobalTypes;
using GlobalTypes.Events;
using InGame.Interfaces;
using InGame.GameObjects;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace InGame
{
    public static class Level
    {
        public static char[] KeyPattern { get; private set; } = new char[] { 'Q', 'W', 'E', 'R' };
        public static int StorageSize { get; private set; } = 5;

        private readonly static List<ILevelObject> levelObjects = new();

        public static void New()
        {
            Clear();

            FrameEvents.PostDraw.AppendSingle(gt =>
            {
                Vector2 center = InstanceInfo.WindowSize / 2;

                ComboStorage storage = new(StorageSize)
                {
                    position = center.WhereY(y => y = 200)
                };
                StorageFiller filler = new(storage, 5)
                {
                    position = center.WhereY(y => y = 700)
                };

                Player player = new()
                {
                    position = center
                };
                AddObjects(storage, filler, player);

                levelObjects.ForEach(i => i.Init());
            });
        }
        public static void Clear()
        {
            foreach(var item in levelObjects)
                item.Terminate();

            levelObjects.Clear();
        }

        public static void AddObject(ILevelObject levelObject) => levelObjects.Add(levelObject);
        public static void AddObjects(params ILevelObject[] levelObjects)
        {
            foreach (var item in levelObjects)
                AddObject(item);
        }

        public static void RemoveObject(ILevelObject levelObject) => levelObjects.Remove(levelObject);
        public static void ContainsObject(ILevelObject levelObject) => levelObjects.Contains(levelObject);

        public static T GetObject<T>(string tag) where T : class
        {
            return levelObjects.Where(i => i.IsTagEqual(tag)).FirstOrDefault().As<T>();

        }
        public static T GetObject<T>() where T : class
        {
            return levelObjects.Where(i => i.IsTagEqual(typeof(T).Name)).FirstOrDefault().As<T>();
        }
        public static object GetObject(string tag) => GetObject<object>(tag);
    }
}