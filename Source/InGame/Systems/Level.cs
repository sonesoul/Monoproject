using GlobalTypes;
using GlobalTypes.Events;
using InGame.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System;
using InGame.TileProcessing;
using InGame.Pools;
using System.Collections;
using Monoproject;

namespace InGame
{
    public static class Level
    {
        public static ILevelTask CurrentTask { get; private set; }

        public static TileSet Tiles { get; private set; }
        public static List<Vector2> TopZones { get; private set; } = new();
        public static List<Vector2> JumpZones { get; private set; } = new();
        public static Vector2 TileSize { get; private set; } = new(37, 37);

        public static IndexPool RegularLevels { get; private set; } = new(1); 

        public static event Action Created, Cleared, Completed, Failed;
        public static float TimePlayed { get; private set; } = 0;

        private readonly static List<ILevelObject> levelObjects = new();
        private static StepTask countTask = null;

        public static void Load(int index = -1)
        {
            FrameEvents.UpdateUnscaled.AddSingle(() =>
            {
                Clear();

                Vector2 center = Window.Center;

                Tiles = TileExtractor.Load(index < 0 ? LevelConfig.MapIndex : index);

                TileBuilder.Build(Tiles);

                TopZones = TileBuilder.GetTopZones(Tiles);
                JumpZones = TileBuilder.GetJumpZones(Tiles);

                CurrentTask = LevelTaskPool.GetRandom();
                CurrentTask.Start();

                StepTask.Replace(ref countTask, CountPlayingTime);

                Created?.Invoke();
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
                    item.ForceDestroy();
                    
                    toRemove.Add(item);
                }
            }

            foreach (var item in toRemove)
            {
                levelObjects.Remove(item);
            }
            
            Tiles = null;

            TopZones.Clear();
            JumpZones.Clear();

            Cleared?.Invoke();

            Main.Instance.ForceGC();
        }

        public static void Complete() => Completed?.Invoke();
        public static void Fail() => Failed?.Invoke();


        public static void AddObject(ILevelObject levelObject)
        {
            levelObjects.Add(levelObject);
        }
        public static void RemoveObject(ILevelObject levelObject)
        {
            levelObjects.Remove(levelObject);
            levelObject.Destroy();
        }
        public static void ContainsObject(ILevelObject levelObject) => levelObjects.Contains(levelObject);

        public static T GetObject<T>() where T : class, ILevelObject => levelObjects.OfType<T>().FirstOrDefault();
        public static List<T> GetObjects<T>() where T : class, ILevelObject => levelObjects.OfType<T>().ToList();
        
        private static IEnumerator CountPlayingTime()
        {
            TimePlayed = 0;
            while (true)
            {
                TimePlayed += FrameState.DeltaTime;
                yield return null;
            }
        }
    }
}