namespace GlobalTypes.Events
{
    public static class UpdateOrders
    {
        public static int InputManager => -3;
        public static int StepTaskManager => -2;
        public static int GameMain => -1;
    }
    public static class EndUpdateOrders
    {
        public static int ColliderUpdater => -2;
        public static int RigidbodyUpdater => -1;
    }
    public static class EndSingleOrders
    {
        public static int Destroy => -2;
        public static int Dispose => -1;
    }
}