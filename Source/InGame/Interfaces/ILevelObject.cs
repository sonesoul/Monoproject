using System;

namespace InGame.Interfaces
{
    public interface ILevelObject : IDisposable
    {
        void OnAdd() { }
        void OnRemove() { }

        void IDisposable.Dispose() => GC.SuppressFinalize(this);
    }
}