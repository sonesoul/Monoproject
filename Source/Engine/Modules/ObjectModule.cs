using System;
using System.Linq;

namespace Engine.Modules
{
    public abstract class ObjectModule : IDisposable
    {
        private ModularObject _owner;
        public event Action PreDispose;

        public bool IsDisposed { get; private set; }  
        public ModularObject Owner => _owner;

        public void SetOwner(ModularObject newOwner)
        {
            if (newOwner == _owner)
                return;

            if (_owner != null && _owner.ContainsModule(this))
                _owner.RemoveModule(this);

            _owner = newOwner;

            if (_owner != null && !_owner.ContainsModule(this))
                _owner.AddModule(this);
        }

        public ObjectModule(ModularObject owner) => _owner = owner;
        ~ObjectModule() => Dispose(false);

        public static T New<T>(ModularObject owner, params object[] args) where T : ObjectModule
            => (T)Activator.CreateInstance(typeof(T), new object[] { owner }.Concat(args).ToArray());

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
                SetOwner(null);
                PreDispose = null;
            }

            Destruct();
        }

        protected abstract void Destruct();
    }
}