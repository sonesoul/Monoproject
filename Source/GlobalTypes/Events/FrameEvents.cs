namespace GlobalTypes.Events
{
    public static class FrameEvents
    {
        public static OrderedEvent Update { get; } = new();
        public static OrderedEvent EndUpdate { get; } = new();
        public static OrderedEvent FixedUpdate { get; } = new();

        public static SingleTriggerEvent EndSingle { get; } = new();
        
        public static OrderedEvent PreDraw { get; } = new();
        public static OrderedEvent PostDraw { get; } = new();
    }
}