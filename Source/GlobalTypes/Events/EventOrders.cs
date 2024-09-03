namespace GlobalTypes.Events
{
    public static class EventOrders
    {
        public static class Update 
        {
            public static int FrameInfo => -3;
            public static int UI => -2;
            public static int InputManager => -1;
            public static int GameMain => 0;
        }
        public static class EndUpdate
        {
            public static int Rigidbody => -3;
            public static int Collider => -2;
        }
        public static class EndSingle
        {
            public static int Destroy => -3;
            public static int Dispose => -2;
        }
    }
}