using Engine;
using Engine.Modules;
using Engine.Types;
using GlobalTypes.Input;
using GlobalTypes.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace InGame.Scripts
{
    class PlayerScript : ObjectModule
    {
        private readonly float _jumpPower = 9.5f;
        private readonly float _moveSpeed = 6;

        private EventListener<GameTime> _updateListener;
        private Rigidbody _rigidbody;
        private KeyListener _jumpKey;

        public PlayerScript(TextObject owner = null) : base(owner) { }

        protected override void Initialize()
        {
            _updateListener = FrameEvents.Update.Append(Update);

            var owner = Owner as TextObject;
            owner.origin += new Vector2(0, -2.5f);

            _rigidbody = owner.AddModules(
            new Collider()
            {
                polygon = Polygon.Rectangle(30, 30)
            },
            new Rigidbody()
            {
                maxVelocity = new(50, 50),
                GravityScale = 3
            })[1] as Rigidbody;

            _jumpKey = InputManager.AddKey(Keys.Space, KeyEvent.Press, Jump);
        }
        private void Update(GameTime gameTime)
        {
            Owner.position.X += InputManager.Axis.X * _moveSpeed;
        }
        private void Jump()
        {
            _rigidbody.velocity = Vector2.Zero;
            _rigidbody.Windage = 0;
            _rigidbody.AddForce(new Vector2(0, -_jumpPower));
        }
        protected override void PostDispose()
        {
            FrameEvents.Update.Remove(_updateListener);
            InputManager.RemoveKey(_jumpKey);

            _rigidbody = null;
            _jumpKey = null;
        }
    }
}