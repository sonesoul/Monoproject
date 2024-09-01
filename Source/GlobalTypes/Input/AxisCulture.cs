using Microsoft.Xna.Framework.Input;

namespace GlobalTypes.Input
{
    public readonly struct AxisCulture
    {
        public Keys Up { get; init; }
        public Keys Down { get; init; }
        public Keys Left { get; init; }
        public Keys Right { get; init; }

        public readonly void Deconstruct(out Keys up, out Keys down, out Keys left, out Keys right)
        {
            up = Up;
            down = Down;
            left = Left;
            right = Right;
        }

        public static AxisCulture WASD => new()
        {
            Up = Keys.W,
            Down = Keys.S,
            Left = Keys.A,
            Right = Keys.D,
        };
        public static AxisCulture Arrows => new()
        {
            Up = Keys.Up,
            Down = Keys.Down,
            Left = Keys.Left,
            Right = Keys.Right,
        };
    }
}