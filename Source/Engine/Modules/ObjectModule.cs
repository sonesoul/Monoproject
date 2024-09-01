using System;

namespace Engine.Modules
{
    public abstract class ObjectModule : IDisposable
    {
        private ModularObject _owner;
        public event Action PreDispose;
        public event Action<ModularObject> OwnerChanged;
        public bool IsDisposed { get; private set; }  
        public bool IsInited { get; private set; }  
        public ModularObject Owner => _owner;

        public ObjectModule(ModularObject owner = null) 
        {
            if (owner == null)
                return;

            Construct(owner);
        }
        public void Construct(ModularObject owner)
        {
            if (IsInited)
                return;
            IsInited = true;

            _owner = owner ?? throw new ArgumentNullException(nameof(owner));

            Initialize();
        }
        protected abstract void Initialize();
        ~ObjectModule() => Dispose(false);

        public void SetOwner(ModularObject newOwner)
        {
            if (newOwner == _owner)
                return;

            if (_owner != null && _owner.ContainsModule(this))
                _owner.RemoveModule(this);

            _owner = newOwner;

            if (_owner != null && !_owner.ContainsModule(this))
                _owner.AddModule(this);

            OwnerChanged?.Invoke(_owner);
        }
        public void AssignOwner(ModularObject newOwner) => _owner = newOwner;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if(IsDisposed) 
                return;

            IsDisposed = true;

            if (disposing)
            {
                PreDispose?.Invoke();
                SetOwner(null);
                PreDispose = null;
            }
        }
    }
}