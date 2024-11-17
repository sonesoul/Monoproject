using Engine;
using Engine.Modules;
using Engine.Types;
using InGame.Interfaces;
using Microsoft.Xna.Framework;

namespace InGame.GameObjects
{
    public class JumpPad : ILevelObject
    {
        public virtual Vector2 Force { get; protected set; }

        public Vector2 Positon { get; private set; }
        public StringObject Object { get; private set; }
        protected string Sprite { get; set; }

        public JumpPad(Vector2 positon) 
        {
            Sprite = ".^.";
            Force = new(0, -10);
            Positon = positon;
        }

        public void OnAdd()
        {
            if (Object == null) 
            {
                Collider collider = new()
                {
                    Shape = Polygon.Rectangle(Level.TileSize / 2),
                    IsShapeVisible = false,
                };

                collider.OnOverlapEnter += OnColliderEnter;
                collider.OnOverlapExit += OnColliderExit;

                Object = new(Sprite, UI.SilkBold, true, collider)
                {
                    Position = Positon
                };

                
            }
        }
        public void OnRemove() 
        {
            Object?.Destroy();
            Object = null;
        }

        private void OnColliderEnter(Collider other)
        {
            if (other.Owner is Player player)
            {
                player.Movement.EnableJump();
                player.Movement.JumpPower = -Force.Y;
            }
        }
        private void OnColliderExit(Collider other)
        {
            if (other.Owner is Player player)
            {
                player.Movement.JumpPower = player.Movement.BaseJumpPower;
            }
        }
    }
}