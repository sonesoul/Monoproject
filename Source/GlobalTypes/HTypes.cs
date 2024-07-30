using Microsoft.Xna.Framework;
using System;
using Monoproject;
using GlobalTypes.Events;
using GlobalTypes.Interfaces;

namespace GlobalTypes
{
    public static class HMath
    {
        public static float DegToRad(float degrees) => degrees * (float)Math.PI / 180;
        public static float RadToDeg(float radians) => radians * 180f / (float)Math.PI;
    }
    public class HTime : IInitable
    {
        public void Init() => GameEvents.OnUpdate.AddListener(UpdateValues);

        public static void UpdateValues(GameTime gameTime)
        {
            DeltaTimeMs = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public static float DeltaTimeMs { get; private set; }
        public static float DeltaTime { get; private set; }
    }
    public static class HCoords
    {
        public const int OneUnitPixels = 200;
        public static float ToUnits(float amount) => amount / OneUnitPixels;
        public static float ToPixels(float units) => units * OneUnitPixels;

        public static Vector2 ToUnits(Vector2 pxPos)
        {
            return new Vector2
            {
                X = ToUnits(pxPos.X),
                Y = ToUnits(pxPos.Y),
            };
        }
        public static Vector2 ToPixels(Vector2 unitPos)
        {
            return new Vector2
            {
                X = ToPixels(unitPos.X),
                Y = ToPixels(unitPos.Y),
            };
        }
        public static float UnitsPerSec(float speed) => HCoords.ToPixels(speed) * HTime.DeltaTime;
    }
}