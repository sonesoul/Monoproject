namespace GlobalTypes.InputManagement
{
    public readonly struct AxisCulture
    {
        public Key Up { get; init; }
        public Key Down { get; init; }
        public Key Left { get; init; }
        public Key Right { get; init; }

        public readonly void Deconstruct(out Key up, out Key down, out Key left, out Key right)
        {
            up = Up;
            down = Down;
            left = Left;
            right = Right;
        }

        public AxisCulture(Key up, Key down, Key left, Key right)
        {
            Up = up;
            Down = down;
            Left = left;
            Right = right;
        }

        public static AxisCulture WASD => new(Key.W, Key.S, Key.A, Key.D);
        public static AxisCulture Arrows => new(Key.Up, Key.Down, Key.Left, Key.Right);
    }
}