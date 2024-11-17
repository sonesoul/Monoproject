using GlobalTypes;
using GlobalTypes.Events;
using InGame.Interfaces;
using InGame.GameObjects;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System;
using InGame.Generators;
using InGame.TaskScripts;

namespace InGame
{
    public static class Level
    {
        public static char[] KeyPattern { get; private set; } = new char[] { 'Q', 'W', 'E', 'R' };
        public static int StorageSize { get; private set; } = 5;
        public static int FillerSize { get; private set; } = 4;

        public static ILevelTask CurrentTask { get; private set; }

        public static TileSet Tiles { get; private set; }
        public static List<Vector2> AbovePlatformTiles { get; private set; } = new();
        public static List<Vector2> ReachableTiles { get; private set; } = new();
        public static Vector2 TileSize { get; private set; } = new(37, 37);

        private readonly static List<ILevelObject> levelObjects = new();

        #region Colors
        private static Color PlatformColor = new(255, 255, 255); //white

        private static Color JumpPadColor = new(128, 128, 255); //light blue
        private static Color StrongJumpPadColor = new(128, 255, 255); //cyan
       
        private static Color SpecialObjectColor = new(255, 255, 128);

        private static Color StorageColor = new(0, 0, 255); //blue
        private static Color FillerColor = new(0, 255, 0); //green

        private static Color AbovePlatformZoneColor = new(255, 128, 128); //coral
        private static Color ReachableZoneColor = new(255, 128, 0); //orange
        #endregion

        public static void Load(int index = 1)
        {
            FrameEvents.EndSingle.Append(() =>
            {
                Clear();

                Vector2 center = MainContext.WindowSize / 2;

                Build(index);
                
                foreach (var item in levelObjects)
                {
                    if (item is IPersistentObject persistent)
                    {
                        persistent.OnLoad();
                    }
                }

                CurrentTask = new PointTouchTask(1);
                CurrentTask.Start();
            });
        }
        public static void Clear()
        {
            CurrentTask?.Finish();
            CurrentTask = null;

            List<ILevelObject> toRemove = new();

            foreach (var item in levelObjects) 
            {
                if (item is not IPersistentObject)
                {
                    item.OnRemove();
                    
                    toRemove.Add(item);
                }
            }

            foreach (var item in toRemove)
            {
                levelObjects.Remove(item);
            }
            
            Tiles = null;

            AbovePlatformTiles.Clear();
            ReachableTiles.Clear();

            GC.Collect();
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

                { JumpPadColor, PlaceJumpPad },
                { StrongJumpPadColor, PlaceStrongJumpPad },
                { SpecialObjectColor, PlaceSpecialObject },

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
                
                { AbovePlatformZoneColor, pos => 
                {
                    AbovePlatformTiles.Add(pos);
                    return null;
                } },
                { ReachableZoneColor, pos => 
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
            StaticPlatform platform = new(position);
            AddObject(platform);
            
            return platform;
        }
        private static object PlaceJumpPad(Vector2 position)
        {
            JumpPad jumpPad = new(position);
            AddObject(jumpPad);

            return jumpPad;
        }
        private static object PlaceStrongJumpPad(Vector2 position)
        {
            StrongJumpPad strongJumpPad = new(position);
            AddObject(strongJumpPad);
            return strongJumpPad;
        }
        private static object PlaceSpecialObject(Vector2 position)
        {
            return null;
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