using InGame.GameObjects;
using InGame.GameObjects.SpecialObjects;
using InGame.Interfaces;
using InGame.Pools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InGame.TileProcessing
{
    public static class TileBuilder
    {
        #region Colors
        private static Color PlatformColor { get; set; } = new(255, 255, 255); //white
        private static Color JumpPadColor { get; set; } = new(128, 128, 255); //light blue
       
        private static Color StorageColor { get; set; } = new(0, 0, 255); //blue
        
        private static Color TopZoneColor { get; set; } = new(255, 128, 128); //coral
        private static Color JumpZoneColor { get; set; } = new(255, 128, 0); //orange
        #endregion

        private static Dictionary<Color, Func<Vector2, object>> BuildPattern { get; set; } = new()
        {
            { PlatformColor, PlacePlatform },
            { JumpPadColor, PlaceJumpPad },
            
            { StorageColor, PlaceStorage },
        };

        public static void Build(TileSet tiles)
        {
            tiles.ForEach(tile =>
            {
                if (BuildPattern.TryGetValue(tile.Color, out var func))
                {
                    tile.Data = func?.Invoke(tile.Position);
                }
                return tile;
            });

            
            PlaceRandomly(tiles, PlaceFiller, new(17, tiles.YLength));
            PlaceRandomly(tiles, PlaceSpecialObject, new(tiles.XLength, tiles.YLength));
        }

        public static List<Vector2> GetTopZones(TileSet tiles)
        {
            List<Vector2> result = new();

            tiles.ForEach(tile =>
            {
                if (tile.Color == TopZoneColor && tile.Data == null)
                {
                    result.Add(tile.Position);
                }
            });

            return result;
        }
        public static List<Vector2> GetJumpZones(TileSet tiles)
        {
            List<Vector2> result = new();

            tiles.ForEach(tile =>
            {
                if (tile.Color == JumpZoneColor && tile.Data == null)
                {
                    result.Add(tile.Position);
                }
            });

            return result;
        }

        private static void PlaceRandomly(TileSet tiles, Func<Vector2, object> callback, Point maxIndex)
        {
            static bool TryGetZone(Point indexPosition, TileSet tiles, out List<Point> indexes)
            {
                indexes = new();

                for (int x = -1; x < 2; x++)
                {
                    for (int y = 0; y < 3; y++)
                    {
                        Tile item = tiles[indexPosition.X + x, indexPosition.Y - y];

                        if (item.Data != null)
                        {
                            return false;
                        }

                        indexes.Add(new(indexPosition.X + x, indexPosition.Y - y));
                    }
                }

                return true;
            }

            List<Point> indexes = new();

            tiles.ForEach((x, y, t) =>
            {
                if (t.Color != TopZoneColor || t.Data != null)
                    return;

                if (x == tiles.XLength - 1 || x == 0 || x > maxIndex.X || y > maxIndex.Y)
                    return;

                indexes.Add(new(x, y));
            });

            Random random = new();
            
            do
            {                
                Point index = indexes[random.Next(indexes.Count)];

                if (!TryGetZone(index, tiles, out var zone))
                {
                    indexes.Remove(index);

                    if (indexes.Count < 1)
                    {
                        throw new InvalidOperationException("There are no free zones to place objects.");
                    }

                    continue;
                }

                object placedObject = callback(tiles[index.X, index.Y].Position);

                foreach (var item in zone)
                {
                    var zoneTile = tiles[item.X, item.Y];
                    zoneTile.Data = placedObject;

                    tiles.SetTile(item.X, item.Y, zoneTile);
                }

                break;

            } while (true);
        }

        private static object PlacePlatform(Vector2 position)
        {
            StaticPlatform platform = new(position);
            Level.AddObject(platform);

            return platform;
        }
        private static object PlaceJumpPad(Vector2 position)
        {
            JumpPad jumpPad = new(position);
            Level.AddObject(jumpPad);

            return jumpPad;
        }

        private static object PlaceFiller(Vector2 position)
        {
            StorageFiller filler = new()
            {
                Position = position
            };
            Level.AddObject(filler);

            return filler;
        }
        private static object PlaceStorage(Vector2 position)
        {
            CodeStorage storage = new()
            {
                Position = position
            };

            Level.AddObject(storage);

            return storage;
        }

        private static object PlaceSpecialObject(Vector2 position)
        {
            var interactable = IntreractablePool.GetRandom(position);

            Level.AddObject(interactable);

            return interactable;
        }
    }
}