namespace GlobalTypes.Events
{
    public static class UpdateOrders
    {
        public static int InputManager => -2;
        public static int StepTaskManager => -1;
        public static int GameMain => 0;
    }
    public static class EndUpdateOrders
    {
        public static int ColliderUpdater => -3;
        public static int RigidbodyUpdater => -2;
    }
    public static class EndSingleOrders
    {
        public static int Destroy => -3;
        public static int Dispose => -2;
    }
}