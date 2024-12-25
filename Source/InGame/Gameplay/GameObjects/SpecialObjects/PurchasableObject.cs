using Engine.Modules;
using Engine;
using InGame.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using System;
using Engine.Types;
using GlobalTypes.Interfaces;

namespace InGame.GameObjects.SpecialObjects
{
    public abstract class PurchasableObject : ModularObject, ILevelObject, IInteractable
    {
        public event Action<Collider> InteractEntered, InteractStayed, InteractExited;

        public abstract int Price { get; protected set; }
        protected Collider collider;
        protected StringObject stringObj;

        protected abstract string Sprite { get; }

        protected PurchasableObject(Vector2 position)
        {
            Position = position;
            SpriteFont font = Fonts.SilkBold;

            stringObj = new(Sprite, font, true, 0)
            {
                Position = this.Position,
            };

            collider = new()
            {
                Shape = Polygon.Rectangle(font.MeasureString(Sprite)),
                IsShapeVisible = false
            };

            collider.ColliderEnter += OnColliderEnter;
            collider.ColliderStay += OnColliderStay;
            collider.ColliderExit += OnColliderExit;

            AddModule(collider);
        }
       
        public override void ForceDestroy()
        {
            base.ForceDestroy();

            collider.ForceDestroy();
            stringObj.ForceDestroy();
        }

        public abstract void ApplyEffect(Player player);
        public void Interact(Player player)
        {
            if (player.BitWallet.TrySpend(Price))
            {
                ApplyEffect(player);
            }
        }

        private void OnColliderEnter(Collider collider) => InteractEntered?.Invoke(collider);
        private void OnColliderStay(Collider collider) => InteractStayed?.Invoke(collider);
        private void OnColliderExit(Collider collider) => InteractExited?.Invoke(collider);
    }
}