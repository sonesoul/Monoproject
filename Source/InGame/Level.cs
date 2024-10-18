using GlobalTypes;
using GlobalTypes.Events;
using InGame.Interfaces;
using InGame.GameObjects;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Engine;
using System;
using Engine.Drawing;
using Engine.Modules;
using Engine.Types;
using InGame.Generators;

namespace InGame
{
    public static class Level
    {
        public static char[] KeyPattern { get; private set; } = new char[] { 'Q', 'W', 'E', 'R' };
        public static int StorageSize { get; private set; } = 5;
        public static int FillerSize { get; private set; } = 4;

        public static Player PlayerInstance { get; private set; } = null;

        private readonly static List<ILevelObject> levelObjects = new();

        private readonly static List<StringObject> _platforms = new();
        private readonly static Dictionary<char, Action<Vector2>> mapPattern = new()
        {
            { '#', static (pos) =>
                {
                   _platforms.Add(
                       new StringObject("", UI.Silk, true,
                           new Collider()
                           {
                               Shape = Polygon.Rectangle(37, 37)
                           },
                           new Rigidbody()
                           {
                               BodyType = BodyType.Static
                           })
                       {
                            Position = pos
                       });
                }
            }
        };
        
        private readonly static Random random = new();
        
        public static void New()
        {
            Clear();

            FrameEvents.EndSingle.Append(() =>
            {
                Vector2 center = InstanceInfo.WindowSize / 2;

                ComboStorage storage = new(StorageSize)
                {
                    Position = center.WhereY(y => y = 200)
                };
                StorageFiller filler = new(storage, 4)
                {
                    Position = center.WhereY(y => y = 700)
                };

                PlayerInstance ??= new();
                PlayerInstance.Position = center;
                PlayerInstance.Reset();
                AddObjects(storage, filler, PlayerInstance);

                NewPlatforms();
            });
        }
        public static void Clear()
        {
            foreach (var item in levelObjects) 
            {
                if (item is not Player)
                    item.Destruct();
            }

            levelObjects.Clear();
            
            foreach (var item in _platforms)
            {
                item.Destroy();
            }
            _platforms.Clear();
        }

        public static StorageFiller NewFiller(ComboStorage storage)
        {
            if (TryGetObject<StorageFiller>(out var old))
            {
                old.Destruct();
            }
            StorageFiller newFiller = new(storage, 4);
            AddObject(newFiller);

            return newFiller;
        }
        public static void NewPlatforms()
        {
            MapGenerator generator = new(mapPattern);
            Vector2 windowSize = InstanceInfo.WindowSize;

            Rectangle screenSquare = new(Point.Zero, windowSize.MinSquare().ToPoint());

            Grid<Vector2> grid = MapGenerator.SliceRect(new Point(20, 20), screenSquare, new(0, 0));

            generator.Generate(
                grid,
                "                    " +
                "                    " +
                "                    " +
                "                    " +
                "                    " +
                "                    " +
                "                    " +
                "                    " +
                "                    " +
                "                    " +
                "                    " +
                "                    " +
                "                    " +
                "                    " +
                "                    " +
                "                    " +
                "       ##           " +
                "                    " +
                "####                " +
                "                    "
            );

        }
        public static ComboStorage NewStorage()
        {
            if (TryGetObject<StorageFiller>(out var old))
            {
                old.Destruct();
            }

            ComboStorage newStorage = new(StorageSize);
            AddObject(newStorage);

            return newStorage;
        }

        public static void AddObject(ILevelObject levelObject)
        {
            levelObjects.Add(levelObject);
            levelObject.Init();
        }
        public static void AddObjects(params ILevelObject[] levelObjects)
        {
            foreach (var item in levelObjects)
                AddObject(item);
        }

        public static void RemoveObject(ILevelObject levelObject)
        {
            levelObjects.Remove(levelObject);
            levelObject.Destruct();
        }
        public static void ContainsObject(ILevelObject levelObject) => levelObjects.Contains(levelObject);

        public static object GetObject(string tag) => GetObject<object>(tag);
        public static T GetObject<T>(string tag) where T : class
        {
            return levelObjects.Where(i => i.IsTagEqual(tag))
                .FirstOrDefault() as T;

        }
        public static T GetObject<T>() where T : class
        {
            return levelObjects.Where(i => i is T)
                .FirstOrDefault() as T;
        }
        
        public static bool TryGetObject<T>(string tag, out T obj) where T : class
        {
            obj = null;

            if (levelObjects.Where(i => i.IsTagEqual(tag)).FirstOrDefault() is T found)
            {
                obj = found;
                return true;
            }

            return false;
        }
        public static bool TryGetObject<T>(out T obj) where T : class
        {
            obj = null;

            if (levelObjects.Where(i => i is T).FirstOrDefault() is T found)
            {
                obj = found;
                return true;
            }

            return false;
        }
    }
}