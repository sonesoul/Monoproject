namespace GlobalTypes.Interfaces
{
    public interface IDestroyable
    {
        bool IsDestroyed { get; set; }

        public void Destroy() => Destroy(this);
        public static void Destroy(IDestroyable destroyable)
        {
            if (destroyable == null || destroyable.IsDestroyed)
                return;

            destroyable.IsDestroyed = true;
            destroyable.ForceDestroy();
        }

        void ForceDestroy();
    }
}