using Engine;
using Engine.Drawing;
using Engine.Modules;
using Engine.Types;
using GlobalTypes.Events;
using GlobalTypes.InputManagement;
using InGame.Interfaces;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InGame.GameObjects
{
    public class Player : TextObject, ILevelObject
    {
        public string Tag => nameof(Player);
        public bool IsInitialized { get; private set; } = false;

        public bool CanMove { get; set; } = true;
        public bool CanJump { get; set; } = true;

        public float JumpPower { get; set; } = 9.5f;
        public float MoveSpeed { get; set; } = 6;
        public IReadOnlyList<Combo> Combos => combos;

        public Key JumpKey { get; set; } = Input.AxisCulture.Up;


        private readonly List<Key> pressedKeys = new();
        
        private Collider collider;
        private Rigidbody rigidbody;

        private OrderedAction<GameTime> _onUpdate;
        private KeyBinding _onJumpPress;

        private readonly List<Combo> combos = new();

        public Player() : base(IngameDrawer.Instance, "#", UI.Silk) 
        {
            color = Color.Green;
            size = new(2, 2);
            originOffset = new(-0.5f, -1.5f);
        }

        public void Init()
        {
            if (IsInitialized)
                throw new InvalidOperationException("Can't be initialized twice.");

            IsInitialized = true;

            List<ObjectModule> modules = AddModules(
            new Collider()
            {
                polygon = Polygon.Rectangle(30, 30)
            },
            new Rigidbody()
            {
                maxVelocity = new(50, 50),
                GravityScale = 3
            });

            collider = modules[0].To<Collider>();
            rigidbody = modules[1].To<Rigidbody>();
            
            _onUpdate = FrameEvents.Update.Append(Update);
            _onJumpPress = Input.Bind(JumpKey, KeyPhase.Press, Jump);
        }
        public void Terminate() => Destroy();

        public void AddCombo(Combo combo) => combos.Add(combo);
        public void RemoveCombo(Combo combo) => combos.Remove(combo);

        private void Update(GameTime time)
        {
            Move();

            UpdateTyping();
        }

        private void Move()
        {
            if (CanMove)
            {
                position.X += Input.Axis.X * MoveSpeed;
            }
        }
        private void Jump()
        {
            if (CanJump)
            {
                rigidbody.velocity.Y = 0;
                rigidbody.AddForce(new Vector2(0, -JumpPower));
            }
        }

        private void UpdateTyping()
        {
            StorageFiller filler = Level.GetObject<StorageFiller>();

            Key[] keys = Input.GetPressedKeys().Where(k => k.ToString().Length == 1).ToArray();

            if (keys.Length > 0)
            {
                int count = 0;
                while (count < keys.Length && pressedKeys.Contains(keys[count]))
                    count++;

                if (count >= keys.Length)
                    return;

                Key key = keys[count];

                char keyChar = (char)key;
                
                if (!Level.KeyPattern.Contains(keyChar))
                    return;

                pressedKeys.Add(key);
                Input.Bind(key, KeyPhase.Release, () => pressedKeys.Remove(key));

                filler.Append(keyChar);
            }
        }

        protected override void PostDestroy()
        {
            base.PostDestroy();

            FrameEvents.Update.Remove(_onUpdate);
            Input.Unbind(_onJumpPress);

            rigidbody = null;
            collider = null;
            _onJumpPress = null;
        }
    }
}