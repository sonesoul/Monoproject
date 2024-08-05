namespace GlobalTypes.Events
{
    public static class GameEvents
    {
        public readonly static ListenerCollection OnUpdate = new();
        public readonly static ListenerCollection OnFixedUpdate = new();
        public readonly static ListenerCollection OnBeforeDraw = new();
        public readonly static ListenerCollection OnAfterDraw = new();
    }
}