namespace Engine.Modules
{
    public abstract class ObjectModule
    {
        private GameObject _owner;
        public ObjectModule(GameObject owner) => Owner = owner;
        public event System.Action OnDestruct;

        public GameObject Owner { get => _owner; set => _owner = value ?? _owner; }
        
        protected virtual void DestructInvoke()
        {
            Destruct();
            OnDestruct?.Invoke();
            OnDestruct = null;
        }
        protected abstract void Destruct();
    }
}