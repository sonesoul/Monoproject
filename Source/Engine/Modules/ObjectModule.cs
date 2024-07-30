namespace Engine.Modules
{
    public abstract class ObjectModule
    {
        private ModularObject _owner;
        public ObjectModule(ModularObject owner) => Owner = owner;
        public event System.Action OnRemoved;

        public ModularObject Owner { get => _owner; set => _owner = value ?? _owner; }
        
        protected virtual void OnRemove()
        {
            Destruct();
            OnRemoved?.Invoke();
            OnRemoved = null;
        }
        protected abstract void Destruct();
    }
}