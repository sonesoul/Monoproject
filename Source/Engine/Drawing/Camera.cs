using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Drawing
{
    public static class Camera
    {
        public static float Zoom
        {
            get => _zoom;
            set => _zoom = Math.Max(0, value);
        }
        public static Vector2 Position
        {
            get => _position;
            set => _position = value;
        }

        private static float _zoom = 1f;
        private static Vector2 _position = Vector2.Zero;

        public static Vector2 ScreenToWorld(Vector2 screenPosition) => Vector2.Transform(screenPosition, Matrix.Invert(GetViewMatrix()));
        public static Vector2 WorldToScreen(Vector2 worldPositon) => Vector2.Transform(worldPositon, GetViewMatrix());

        public static Matrix GetViewMatrix() => Matrix.CreateTranslation(new(-_position, 0)) * Matrix.CreateScale(_zoom, _zoom, 1);
    }
}
