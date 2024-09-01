using Engine;
using Engine.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Text;
using System.Threading.Tasks;
using Engine.Drawing;
using InGame;
using Engine.Modules;

namespace InGame.Generators
{
    class MapGenerator
    {
        private readonly Dictionary<char, Action<Vector2>> mapPattern = new();
        
        public static Dictionary<char, Action<Vector2>> DefaultPattern => new() 
        {
            { '#', (pos) => 
                {
                    _ =
                    new TextObject(IngameDrawer.Instance, "", UI.Font, new Collider()
                    {
                        Mode = ColliderMode.Static,
                        polygon = Polygon.Rectangle(37, 37)
                    })
                    {
                        position = pos
                    };
                }
            }
        };
        

        public MapGenerator(Dictionary<char, Action<Vector2>> mapPattern)
        {
            this.mapPattern = mapPattern;
        }
        public MapGenerator()
        {
            mapPattern = DefaultPattern;
        }

        public void Generate(Grid<Vector2> positions, string input)
        {
            int stringIndex = 0;
            char current;

            positions.ForEach(v =>
            {
                if(stringIndex == input.Length)
                    current = ' ';
                else
                    current = input[stringIndex++];

                if (mapPattern.TryGetValue(current, out var value))
                    value(v);
            });
        }
        public static Grid<Vector2> SliceRect(Point rowsCols, Rectangle source, Point offset = default)
        {
            Point cellSize = GetCellSize(source.Size, rowsCols);
            source.Location -= (cellSize.ToVector2() / 2).ToPoint();

            Grid<Vector2> grid = new(rowsCols.X, rowsCols.Y);

            for (int i = 0; i < rowsCols.X; i++)
            {
                for (int j = 0; j < rowsCols.Y; j++)
                {
                    grid.SetCell(((new Point(i + 1, j + 1) * cellSize) + source.Location + offset).ToVector2(), i, j);
                }
            }

            return grid;
        }

        public static Point GetCellSize(Point size, Point rowsCols) => size / rowsCols;

    }
}