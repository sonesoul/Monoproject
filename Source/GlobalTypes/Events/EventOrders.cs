namespace GlobalTypes.Events
{
    public static class UpdateOrders
    {
        public static int FrameInfo => -4;
        public static int UI => -3;
        public static int InputManager => -2;
        public static int CoroutineManager => -1;
        public static int GameMain => 0;
    }
    public static class EndUpdateOrders
    {
        public static int Rigidbody => -3;
        public static int Collider => -2;
    }
    public static class EndSingleOrders
    {
        public static int Destroy => -3;
        public static int Dispose => -2;
    }
}