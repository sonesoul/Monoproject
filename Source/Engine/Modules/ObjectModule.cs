using System;

namespace Engine.Modules
{
    public abstract class ObjectModule : IDisposable
    {
        private ModularObject _owner;
        public ObjectModule(ModularObject owner) => _owner = owner;
        public event Action OnDispose;
        public bool Disposed { get; private set; }  
        public ModularObject Owner 
        {
            get => _owner; set 
            {
                if (value == _owner) 
                    return;

                if(_owner != null && _owner.ContainsModule(this))
                    _owner.RemoveModule(this);

                if (value != null)
                    value.AddModule(this);
                else 
                    _owner = null;
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool isDisposing)
        {
            if(Disposed) 
                return;

            Disposed = true;

            if (isDisposing)
            {
                OnDispose?.Invoke();
                Owner = null;
                OnDispose = null;
            }

            Destruct();
        }
        ~ObjectModule() => Dispose(false);
        protected abstract void Destruct();
    }
}