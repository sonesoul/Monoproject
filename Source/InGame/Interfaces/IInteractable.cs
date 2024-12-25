using Engine.Modules;
using InGame.GameObjects;
using System;

namespace InGame.Interfaces
{
    public interface IInteractable : ILevelObject
    {
        public event Action<Collider> InteractEntered, InteractStayed, InteractExited;

        public void Interact(Player player);
    }
}