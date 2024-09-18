using Microsoft.Xna.Framework;

namespace GlobalTypes.Events
{
    public static class FrameEvents
    {
        public static OrderedEvent<GameTime> Update { get; } = new();
        public static OrderedEvent<GameTime> EndUpdate { get; } = new();
        public static OrderedEvent<GameTime> FixedUpdate { get; } = new();

        public static SingleTriggerEvent<GameTime> EndSingle { get; } = new();
        
        public static OrderedEvent<GameTime> PreDraw { get; } = new();
        public static OrderedEvent<GameTime> PostDraw { get; } = new();
    }
}