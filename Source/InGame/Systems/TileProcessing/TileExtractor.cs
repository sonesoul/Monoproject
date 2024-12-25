using GlobalTypes;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace InGame.TileProcessing
{
    public static class TileExtractor
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

        public static TileSet Load(int levelIndex)
        {
            Texture2D levelPic = Asset.LoadLevelPicture(levelIndex);
            Point tileSize = Window.Size.ToPoint() / levelPic.Bounds.Size;

            Color[,] colors = GetPixels(levelPic);
            Vector2[,] positions = Slice(levelPic.Bounds.Size, tileSize);

            TileSet tileSet = new(new Tile[levelPic.Width, levelPic.Height], tileSize);

            for (int y = 0; y < levelPic.Height; y++)
            {
                for (int x = 0; x < levelPic.Width; x++)
                {
                    Color pixelColor = colors[x, y];
                    Vector2 position = positions[x, y];

                    tileSet.SetTile(x, y, new(position, pixelColor, null));
                }
            }

            return tileSet;
        }
    }
    public struct Tile
    {
        public object Data { get; set; }
        public Vector2 Position { get; set; }
        public Color Color { get; set; }

        public Tile(Vector2 position, Color color, object data)
        {
            Data = data;

            Position = position;
            Color = color;
        }
    }
    public class TileSet
    {
        public Point TileSize { get; init; }

        public int XLength => tiles.GetLength(0);
        public int YLength => tiles.GetLength(1);

        public Tile this[int x, int y]
        {
            get => tiles[x, y];
            set => tiles[x, y] = value;
        }
        
        private readonly Tile[,] tiles;

        public TileSet(Tile[,] tiles, Point tileSize)
        {
            TileSize = tileSize;
            this.tiles = tiles;
        }

        public void SetTile(int x, int y, Tile value) => tiles[x, y] = value;
        public Tile GetTile(int x, int y) => tiles[x, y];

        public void ForEach(Action<Tile> action) 
        {
            for (int x = 0; x < XLength; x++)
            {
                for (int y = 0; y < YLength; y++)
                {
                    action(tiles[x, y]);
                }
            }
        }
        public void ForEach(Action<int, int, Tile> action)
        {
            for (int x = 0; x < XLength; x++)
            {
                for (int y = 0; y < YLength; y++)
                {
                    action(x, y, tiles[x, y]);
                }
            }
        }
        public void ForEach(Func<Tile, Tile> func)
        {
            for (int x = 0; x < XLength; x++)
            {
                for (int y = 0; y < YLength; y++)
                {
                    SetTile(x, y, func(tiles[x, y]));
                }
            }
        }

        public List<Tile> GetCollection()
        {
            List<Tile> tiles = new();

            ForEach(item => tiles.Add(item));

            return tiles;
        }
    }
}