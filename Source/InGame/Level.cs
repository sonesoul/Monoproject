using GlobalTypes;
using GlobalTypes.Events;
using InGame.Interfaces;
using InGame.GameObjects;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Engine;
using System;
using Engine.Modules;
using Engine.Types;
using InGame.Generators;
using InGame.TaskScripts;

namespace InGame
{
    public static class Level
    {
        public static char[] KeyPattern { get; private set; } = new char[] { 'Q', 'W', 'E', 'R' };
        public static int StorageSize { get; private set; } = 5;
        public static int FillerSize { get; private set; } = 4;

        public static TileSet Tiles { get; private set; }
        public static List<StringObject> Platforms { get; private set; } = new();
        public static List<Vector2> AbovePlatformTiles { get; private set; } = new();
        public static List<Vector2> ReachableTiles { get; private set; } = new();
        
        private readonly static List<ILevelObject> levelObjects = new();
        
        private static Color PlatformColor = new(255, 255, 255); //white
        private static Color StorageColor = new(0, 0, 255); //blue
        private static Color FillerColor = new(0, 255, 0); //green

        private static Color AbovePlatformTileColor = new(255, 128, 128); //light pink
        private static Color ReachableTileColor = new(255, 128, 0); //orange


        public static void Load(int index = 0)
        {
            FrameEvents.EndSingle.Append(() =>
            {
                Clear();

                Vector2 center = InstanceInfo.WindowSize / 2;

                Build(index);
                
                foreach (var item in levelObjects)
                {
                    if (item is IPersistentObject persistent)
                    {
                        persistent.OnLoad();
                    }
                }

                PointTouchTask task = new(5);
                task.Start();
            });
        }
        public static void Clear()
        {
            List<ILevelObject> toRemove = new();

            foreach (var item in levelObjects) 
            {
                if (item is not IPersistentObject)
                {
                    item.OnRemove();
                    item.Dispose();

                    toRemove.Add(item);
                }
            }

            foreach (var item in toRemove)
            {
                levelObjects.Remove(item);
            }
            
            foreach (var item in Platforms)
            {
                item.ForceDestroy();
            }
            
            Platforms.Clear();
            Tiles = null;

            AbovePlatformTiles.Clear();
            ReachableTiles.Clear();
        }

        public static void Build(int index)
        {
            Vector2 storagePosition = new(-1);
            int storagePosCount = 0;

            Vector2 fillerPosition = new(-1);
            int fillerPosCount = 0;
            
            Tiles = LevelGenerator.Load(index, new()
            {
                { PlatformColor, PlacePlatform },

                { StorageColor, pos => 
                {
                    storagePosition += pos;
                    storagePosCount++;
                    return null;
                } },
                { FillerColor, pos => 
                {
                    fillerPosition += pos;
                    fillerPosCount++;
                    return null;
                } },
                
                { AbovePlatformTileColor, pos => 
                {
                    AbovePlatformTiles.Add(pos);
                    return null;
                } },
                { ReachableTileColor, pos => 
                {
                    ReachableTiles.Add(pos);
                    return null;
                } }
            });
             
            if (storagePosition.Any() < 0)
                throw new InvalidOperationException($"There must be a position for {nameof(ComboStorage)}.");
            if (fillerPosition.Any() < 0)
                throw new InvalidOperationException($"There must be a position for {nameof(StorageFiller)}.");

            ComboStorage storage = new(StorageSize)
            {
                Position = storagePosition / storagePosCount
            };
            StorageFiller filler = new(storage, 4)
            {
                Position = fillerPosition / fillerPosCount
            };

            AddObjects(storage, filler);
        }

        private static object PlacePlatform(Vector2 position)
        {
            Collider collider = new()
            {
                Shape = Polygon.Rectangle(37, 37)
            };
            Rigidbody rigidbody = new()
            {
                BodyType = BodyType.Static
            };

            StringObject obj = new("", UI.Silk, true, collider, rigidbody)
            {
                Position = position
            };

            Platforms.Add(obj);

            return obj;
        }

        public static void AddObject(ILevelObject levelObject)
        {
            levelObjects.Add(levelObject);
            levelObject.OnAdd();
        }
        public static void AddObjects(params ILevelObject[] levelObjects)
        {
            foreach (var item in levelObjects)
                AddObject(item);
        }

        public static void RemoveObject(ILevelObject levelObject)
        {
            levelObjects.Remove(levelObject);
            levelObject.OnRemove();
        }

        public static void ContainsObject(ILevelObject levelObject) => levelObjects.Contains(levelObject);

        public static T GetObject<T>() where T : class => levelObjects.Where(i => i is T).FirstOrDefault() as T;
    }
}