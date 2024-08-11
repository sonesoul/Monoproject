namespace GlobalTypes.Events
{
    public static class GameEvents
    {
        public readonly static ListenerCollection Update = new();
        public readonly static ListenerCollection PostUpdate = new();
        public readonly static ListenerCollection FixedUpdate = new();

        public readonly static ListenerCollection PreDraw = new();
        public readonly static ListenerCollection PostDraw = new();
    }
}