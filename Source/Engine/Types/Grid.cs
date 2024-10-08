using System;

namespace Engine.Types
{
    public readonly struct Grid<T>
    {
        public int Rows { get; init; }
        public int Columns { get; init; }

        private readonly T[,] cells;

        public Grid(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
            cells = new T[rows, columns];
        }
        public Grid(int rowsCols)
        {
            Rows = rowsCols;
            Columns = rowsCols;
            cells = new T[rowsCols, rowsCols];
        }
        public readonly T GetCell(int row, int column) => cells[row, column];

        public readonly void ForEach(Action<T> action)
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    action(cells[j, i]);
                }
            }
        }

        public readonly void SetCell(T value, int row, int column) => cells[row, column] = value;
    }
}