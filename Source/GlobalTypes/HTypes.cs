using Microsoft.Xna.Framework;
using System;
using GlobalTypes.Events;
using GlobalTypes.Interfaces;

namespace GlobalTypes
{
    public class HTime : IInitable
    {
        public const float FixedDelta = 1.0f / 240.0f;
        public static float DeltaTime { get; private set; }
        public static float DeltaTimeMs { get; private set; }

        private static float updateTimeBuffer = 0.0f;
        public void Init() => GameEvents.OnUpdate.AddListener(UpdateValues);

        public static void UpdateValues(GameTime gameTime)
        {
            DeltaTimeMs = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            updateTimeBuffer += DeltaTime;
            while (updateTimeBuffer >= FixedDelta)
            {
                GameEvents.OnFixedUpdate.Trigger(gameTime);

                updateTimeBuffer -= FixedDelta;
            }
        }
    }
}