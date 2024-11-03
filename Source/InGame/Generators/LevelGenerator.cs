using Engine;
using GlobalTypes;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;

namespace InGame.Generators
{
    public static class LevelGenerator
    {
        public static Vector2[,] Slice(Point mapSize, Point cellSize)
        {
            Vector2[,] positions = new Vector2[mapSize.X, mapSize.Y];
            
            for (int y = 0; y < mapSize.Y; y++)
            {
                for (int x = 0; x < mapSize.X; x++)
                {
                    positions[x, y] = (new Vector2(x + 1, y + 1) * cellSize.ToVector2()) - cellSize.ToVector2() / 2;
                }
            }
            

            return positions;
        }
        public static Color[,] GetPixels(Texture2D texture)
        {
            Color[] pixelData = new Color[texture.Width * texture.Height];

            texture.GetData(pixelData);
            
            Color[,] pixelGrid = new Color[texture.Width, texture.Height];

            for (int y = 0; y < texture.Height; y++)
            {
                for (int x = 0; x < texture.Width; x++)
                {
                    pixelGrid[x, y] = pixelData[y * texture.Width + x]; 
                }
            }

            return pixelGrid;
        }

        public static Texture2D LoadPicture(int index) => InstanceInfo.Content.Load<Texture2D>($"Levels/level_{index}");
        
        public static TileSet Load(int levelIndex, Dictionary<Color, Func<Vector2, object>> callbacks)
        {
            Texture2D levelPic = LoadPicture(levelIndex);
            Point tileSize = InstanceInfo.WindowSize.ToPoint() / levelPic.Bounds.Size;

            Color[,] colors = GetPixels(levelPic);
            Vector2[,] positions = Slice(levelPic.Bounds.Size, tileSize);

            TileSet tileSet = new(new Tile[levelPic.Width, levelPic.Height], tileSize);

            for (int y = 0; y < levelPic.Height; y++)
            {
                for (int x = 0; x < levelPic.Width; x++)
                {
                    object obj = null;
                    Color pixelColor = colors[x, y];
                    Vector2 position = positions[x, y];

                    if (callbacks.TryGetValue(pixelColor, out var func))
                    {
                        obj = func(position);
                    }

                    tileSet.SetTile(x, y, new(position, pixelColor, obj));
                }
            }

            return tileSet;
        }
    }
    public struct Tile
    {
        public object Object { get; set; }
        public Vector2 Position { get; set; }
        public Color Color { get; set; }

        public Tile(Vector2 position, Color color, object obj)
        {
            Object = obj;

            Position = position;
            Color = color;
        }
    }
    public class TileSet 
    {
        public Point TileSize { get; init; }
        public int XLast => tiles.GetLength(0) - 1;
        public int YLast => tiles.GetLength(1) - 1;

        private readonly Tile[,] tiles;

        public TileSet(Tile[,] tiles, Point tileSize)
        {
            TileSize = tileSize;
            this.tiles = tiles;
        }

        public Tile GetTile(int x, int y) => tiles[x, y];
        public void SetTile(int x, int y, Tile value) => tiles[x, y] = value;
    }
}