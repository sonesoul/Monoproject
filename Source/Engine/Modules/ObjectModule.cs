namespace Engine.Modules
{
    public abstract class ObjectModule
    {
        private GameObject _owner;
        public ObjectModule(GameObject owner) => Owner = owner;
        public event System.Action OnRemoved;

        public GameObject Owner { get => _owner; set => _owner = value ?? _owner; }
        
        protected virtual void OnRemove()
        {
            Destruct();
            OnRemoved?.Invoke();
            OnRemoved = null;
        }
        protected abstract void Destruct();
    }
}