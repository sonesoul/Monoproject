using System.Collections.Generic;

namespace GlobalTypes.Collections
{
    public interface ILockCollection<T> : ICollection<T>
    {
        public bool IsLocked { get; }
        public int ChangeCount { get; }

        public void Lock();
        public void Unlock();
        protected void ApplyChanges() { }
    }
}