using GlobalTypes.Events;
using System;

namespace Engine.Modules
{
    public abstract class ObjectModule : IDisposable
    {
        private ModularObject _owner;
        public event Action Disposing;
        public event Action<ModularObject> OwnerChanged;
        public bool IsDisposed { get; private set; }  
        public bool IsConstructed { get; private set; }  
        public ModularObject Owner => _owner;

        public ObjectModule(ModularObject owner = null) 
        {
            if (owner == null)
                return;

            Construct(owner);
        }
        public void Construct(ModularObject owner)
        {
            if (IsConstructed)
                return;

            IsConstructed = true;

            _owner = owner ?? throw new ArgumentNullException(nameof(owner));

            PostConstruct();
        }
        protected abstract void PostConstruct();
        ~ObjectModule() => DisposeAction(false);

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
            FrameEvents.EndSingle.Add(() => DisposeAction(true), EndSingleOrders.Dispose); 
            GC.SuppressFinalize(this);
        }
        public void ForceDispose() => DisposeAction(true);
        protected virtual void DisposeAction(bool disposing)
        {
            if (IsDisposed)
                return;

            IsDisposed = true;

            PreDispose();

            if (disposing)
            {
                Disposing?.Invoke();
                SetOwner(null);
            }
            else
                _owner = null;
            
            Disposing = null;
            OwnerChanged = null;

            PostDispose();
        }

        protected virtual void PostDispose() { }
        protected virtual void PreDispose() { }
    }
}