using GlobalTypes.Events;
using GlobalTypes.Interfaces;
using System;

namespace Engine.Modules
{
    public abstract class ObjectModule : IDestroyable
    {
        public bool IsDestroyed { get; set; }
        public bool IsConstructed { get; private set; }  
        public ModularObject Owner => _owner;

        public event Action Destroyed;
        public event Action<ModularObject> OwnerChanged;
        
        private ModularObject _owner;
        
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

        public void Destroy()
        {
            if (IsDestroyed)
                return;

            IsDestroyed = true;
            
            FrameEvents.EndSingle.Add(ForceDestroy, EndSingleOrders.Dispose);
        }

        public virtual void ForceDestroy()
        {
            SetOwner(null);
            Destroyed?.Invoke();

            _owner = null;

            Destroyed = null;
            OwnerChanged = null;
        }
    }
}