using Engine;
using Engine.Modules;
using Engine.Types;
using GlobalTypes.Input;
using GlobalTypes.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using GlobalTypes;
using System.Linq;
using System.Collections.Generic;
using InGame.GameObjects;
using InGame.Interfaces;

namespace InGame.Scripts
{
    public class PlayerScript : ObjectModule, ITaggable
    {
        public string Tag => "player";

        private readonly float _jumpPower = 9.5f;
        private readonly float _moveSpeed = 6;
        private readonly float _holdThreshold = 0.3f;
        
        private readonly Keys _jumpKey = Keys.Space;
        private readonly Keys _typingKey = Keys.LeftShift;
        
        private KeyListener _onJumpPress;

        private KeyListener _onTypingPress;
        private KeyListener _onTypingHold;
        private KeyListener _onTypingRelease;

        private OrderedAction<GameTime> _onUpdate;
        
        private bool isTyping = false;
        private float holdTime = 0;
        private bool isKeyHeld = false;

        //will be changed
        private bool CanMove => !isTyping; 
        private bool CanJump => !isTyping;

        private readonly List<Keys> pressedKeys = new();

        private Rigidbody _rigidbody;
        private Collider _collider;
        private Collider _fillerCollider;
        private static Collider FillerCollider { get; set; }

        public PlayerScript(StorageFiller filler, ModularObject owner = null) : base(owner) 
        {
            _fillerCollider = filler.GetModule<Collider>();

            InputManager.AddKey(Keys.Back, KeyEvent.Press, () =>
            {
                filler.Backspace();
            });
            InputManager.AddKey(Keys.Enter, KeyEvent.Press, () =>
            {
                filler.Push();
            });
        }

        protected override void PostConstruct()
        {
            _onUpdate = FrameEvents.Update.Append(Update);

            var owner = Owner as TextObject;
            owner.origin += new Vector2(0, -2.5f);

            List<ObjectModule> added = owner.AddModules(
            new Collider()
            {
                polygon = Polygon.Rectangle(30, 30)
            },
            new Rigidbody()
            {
                maxVelocity = new(50, 50),
                GravityScale = 3
            });

            (_collider, _rigidbody) = (added[0] as Collider, added[1] as Rigidbody);
            

            _onJumpPress = InputManager.AddKey(_jumpKey, KeyEvent.Press, Jump);

            _onTypingPress = InputManager.AddKey(_typingKey, KeyEvent.Press, () => ToggleTyping(KeyEvent.Press));
            _onTypingHold = InputManager.AddKey(_typingKey, KeyEvent.Hold, () => ToggleTyping(KeyEvent.Hold));
            _onTypingRelease = InputManager.AddKey(_typingKey, KeyEvent.Release, () => ToggleTyping(KeyEvent.Release));
        }
        private void Update(GameTime gameTime)
        {
            if (CanMove)
                Owner.position.X += InputManager.Axis.X * _moveSpeed;

            if (isTyping)
                UpdateTyping();
        }
        private void UpdateTyping()
        {
            if (_fillerCollider.IntersectsWith(_collider))
            {
                StorageFiller filler = Level.StorageFiller;
                Keys[] keys = FrameState.KeyState.GetPressedKeys().Where(k => k.ToString().Length == 1).ToArray();

                if (keys.Length > 0)
                {
                    int count = 0;
                    while (count < keys.Length && pressedKeys.Contains(keys[count]))
                        count++;

                    if (count >= keys.Length)
                        return;

                    Keys key = keys[count];

                    pressedKeys.Add(key);
                    InputManager.AddKey(key, KeyEvent.Release, () => pressedKeys.Remove(key));

                    Level.StorageFiller.Append((char)key);
                }
            }
        }
        
        private void ToggleTyping(KeyEvent keyEvent)
        {
            switch (keyEvent)
            {
                case KeyEvent.Press:

                    if (!isTyping)
                    {
                        isTyping = true;
                    }
                    else if (!isKeyHeld)
                    {
                        isTyping = false;
                        holdTime = 0;
                    }

                    break;
                case KeyEvent.Hold:

                    holdTime += FrameState.DeltaTime;
                    isKeyHeld = holdTime > _holdThreshold;

                    break;
                case KeyEvent.Release:

                    if (isKeyHeld)
                    {
                        isTyping = false;
                        holdTime = 0;
                    }

                    break;
            }
        }
        private void Jump()
        {
            if (!CanJump)
                return;
            
            _rigidbody.velocity = Vector2.Zero;
            _rigidbody.Windage = 0;
            _rigidbody.AddForce(new Vector2(0, -_jumpPower));
        }

        protected override void PostDispose()
        {
            FrameEvents.Update.Remove(_onUpdate);
            InputManager.RemoveKey(_onJumpPress);
            
            InputManager.RemoveKey(_onTypingPress);
            InputManager.RemoveKey(_onTypingHold);
            InputManager.RemoveKey(_onTypingRelease);

            _rigidbody = null;
            _onJumpPress = null;
        }
    }
}