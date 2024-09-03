using Engine;
using Engine.Drawing;
using Engine.Modules;
using Engine.Types;
using GlobalTypes;
using InGame.GameObjects;
using InGame.Generators;
using InGame.Scripts;
using Microsoft.Xna.Framework;
using Monoproject;
using System;
using System.Collections.Generic;

namespace InGame
{
    public static class Level
    {
        public class LevelInfo
        {
            public TextObject Player { get; private set; }
            public WordStorage WordStorage { get; private set; }
            public StorageFiller StorageFiller { get; private set; }
            public IReadOnlyList<TextObject> Platforms { get; private set; }
            
            public LevelInfo(IReadOnlyList<TextObject> platforms, WordStorage storage, StorageFiller filler, TextObject player)
            {
                WordStorage = storage;
                Platforms = platforms;
                StorageFiller = filler;
                Player = player;
            }
        }

        public static LevelInfo Current { get; private set; } = null;
        public static TextObject Player => Current?.Player;
        public static IReadOnlyList<TextObject> Platforms => Current?.Platforms; 
        public static WordStorage WordStorage => Current?.WordStorage;
        public static StorageFiller StorageFiller => Current?.StorageFiller;

        private readonly static List<TextObject> _platforms = new();
        private readonly static Dictionary<char, Action<Vector2>> mapPattern = new()
        {
            { '#', static (pos) => 
                {
                   _platforms.Add(
                       new TextObject(IngameDrawer.Instance, "", UI.Font, 
                           new Collider() 
                           {
                                Mode = ColliderMode.Static,
                                polygon = Polygon.Rectangle(37, 37)
                           })
                       {
                            position = pos
                       });
                } 
            }
        };

        public static void Clear()
        {
            if (Current != null)
            {
                foreach (TextObject p in Platforms)
                    p.Destroy();

                WordStorage.Destroy();
                Player.Destroy();
                StorageFiller.Destroy();
            }
        }
        public static void New()
        {
            Clear();

            MapGenerator generator = new(mapPattern);
            Vector2 windowSize = InstanceInfo.WindowSize;
            Vector2 windowCenter = windowSize / 2;

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


            TextObject player = new(IngameDrawer.Instance, "#", UI.Font, new PlayerScript())
            {
                position = windowCenter,
                Color = Color.Green,
                size = new(2, 2),
            };
            WordStorage storage = new() { position = windowCenter };
            StorageFiller filler = new(storage, 5) { position = new(windowCenter.X, 200) };

            Current = new(
                new List<TextObject>(_platforms),
                storage, filler, player);
                
            
            _platforms.Clear();
        }
    }

    public static class OldLevelSystem
    {
        public static WordStorage WordStorage { get; private set; }
        public static IReadOnlyList<TextObject> Platforms => _platforms;
        public static TextObject Player { get; private set; }

        private static List<TextObject> _platforms = new();

        private readonly static Dictionary<char, Action<Vector2>> mapPattern = new()
        {
            { '#', static (pos) =>
                {
                   _platforms.Add(
                       new TextObject(IngameDrawer.Instance, "", UI.Font,
                           new Collider()
                           {
                                Mode = ColliderMode.Static,
                                polygon = Polygon.Rectangle(37, 37)
                           })
                       {
                            position = pos
                       });
                }
            }
        };

        public static void Clear()
        {
            foreach (var item in _platforms)
                item?.Destroy();

            WordStorage?.Destroy();
            Player?.Destroy();
        }
        public static void New()
        {
            Clear();

            MapGenerator generator = new(mapPattern);

            Rectangle screenSquare = new(Point.Zero, InstanceInfo.WindowSize.MinSquare().ToPoint());
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

            WordStorage = new() { position = InstanceInfo.WindowSize / 2 };
            Player = new(IngameDrawer.Instance, "#", UI.Font, new PlayerScript())
            {
                position = InstanceInfo.WindowSize / 2,
                Color = Color.Green,
                size = new(2, 2),
            };
        }
    }
}