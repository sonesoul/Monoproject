using Engine;
using Engine.Modules;
using Engine.Types;
using GlobalTypes.Interfaces;
using InGame.Interfaces;

namespace InGame.GameObjects
{
    public class JumpPad : ILevelObject
    {
        public bool IsDestroyed { get; set; } = false;

        public virtual Vector2 Force { get; protected set; }

        public Vector2 Position { get; private set; }
        public StringObject Object { get; private set; }
        protected string Sprite { get; set; }

        public JumpPad(Vector2 position) 
        {
            Sprite = @"|^|";
            Force = new(0, -11);
            Position = position;

            Collider collider = new()
            {
                Shape = Polygon.Rectangle(new(Level.TileSize.X / 2, 3)),
                IsShapeVisible = false,
            };

            collider.ColliderEnter += OnColliderEnter;
            collider.ColliderExit += OnColliderExit;

            Object = new(Sprite, Fonts.SilkBold, true, collider)
            {
                Position = Position
            };
        }

        public void Destroy() => IDestroyable.Destroy(this);
        public void ForceDestroy() 
        {
            Object?.Destroy();
            Object = null;
        }

        private void OnColliderEnter(Collider other)
        {
            if (other.Owner is Player player)
            {
                player.Movement.ResetJump();
                player.Movement.JumpPower = -Force.Y;
            }
        }
        private void OnColliderExit(Collider other)
        {
            if (other.Owner is Player player)
            {
                player.Movement.JumpPower = player.Movement.BaseJumpPower;
                player.Movement.DisableJump();
            }
        }
    }
}