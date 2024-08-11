using System;

namespace Engine.Modules
{
    public abstract class ObjectModule : IDisposable
    {
        private ModularObject _owner;
        public event Action PreDispose;

        public bool IsDisposed { get; private set; }  
        public ModularObject Owner 
        {
            get => _owner;
            set 
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
        
        public ObjectModule(ModularObject owner) => _owner = owner;
        ~ObjectModule() => Dispose(false);
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if(IsDisposed) 
                return;

            IsDisposed = true;

            if (disposing)
            {
                PreDispose?.Invoke();
                Owner = null;
                PreDispose = null;
            }

            Destruct();
        }

        protected abstract void Destruct();
    }
}