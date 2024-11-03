using Engine;
using Engine.Drawing;
using Engine.Modules;
using Engine.Types;
using GlobalTypes;
using GlobalTypes.Events;
using GlobalTypes.InputManagement;
using InGame.Interfaces;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InGame.GameObjects
{
    public class Player : StringObject, IPersistentObject
    {
        public bool CanMove { get; set; } = true;
        public bool CanJump { get; set; } = true;
        public bool CanCombinate { get; set; } = false;

        public float JumpPower { get; set; } = 9f;
        public float MoveSpeed { get; set; } = 4;

        public static event Action<Combo> OnComboPush, OnComboPop;

        private IComboReader currentReader = null;
        private Stack<Combo> comboStack = new();
        
        private Rigidbody rigidbody;
        private Collider collider;

        private OrderedAction _onUpdate;

        public Player() : base("#", UI.Silk, true) 
        {
            CharColor = Color.Green;
            Scale = new(2, 2);
            OriginOffset = new(-0.5f, -1.5f);

            List<ObjectModule> modules = AddModules(
            new Collider()
            {
                Shape = Polygon.Rectangle(30, 30)
            },
            new Rigidbody()
            {
                MaxVelocity = new(50, 50),
            });

            collider = modules[0] as Collider;
            rigidbody = modules[1] as Rigidbody;

            _onUpdate = FrameEvents.Update.Append(Update);

            Input.OnKeyPress += OnKeyPressed;

            Input.Bind(Key.Up, KeyPhase.Hold, Jump);

            collider.OnOverlapEnter += OnColliderEnter;
            collider.OnOverlapExit += OnColliderExit;

            
        }

        public void OnLoad()
        {
            if (rigidbody != null)
                rigidbody.velocity = Vector2.Zero;

            Position = Level.AbovePlatformTiles.RandomElement();
        }
        public void OnRemove() => Destroy();
        
        private void Update()
        {
            rigidbody.velocity.X = Input.Axis.X * MoveSpeed;
        }

        private void Jump()
        {
            if (CanJump)
            {
                rigidbody.velocity.Y = 0;
                rigidbody.velocity.Y -= JumpPower;

                CanJump = false;
            }
        }

        public void PushCombo(Combo combo)
        {
            comboStack.Push(combo);
            OnComboPush?.Invoke(combo);
        }
        public Combo PopCombo()
        {
            var combo = comboStack.Pop();
            OnComboPop?.Invoke(combo);

            return combo;
        }
        public Combo PeekCombo() => comboStack.Peek();


        private void OnColliderEnter(Collider other)
        {
            if (other.Owner is IComboReader comboReader)
            {
                currentReader = comboReader;
                comboReader.Activate();
            }
            else if (other.Owner.ContainsModule<Rigidbody>())
            {
                var mtv = collider.GetMTV(other);

                if (mtv.Y > 0)
                {
                    CanJump = true;
                }
            }
        }
        private void OnColliderExit(Collider other)
        {
            if (other.Owner is IComboReader comboReader)
            {
                currentReader = null;
                comboReader.Deactivate();
            }
        }

        private void OnKeyPressed(Key key)
        {
            char keyChar = (char)key;

            if (!Level.KeyPattern.Contains(keyChar) || currentReader == null || comboStack.Count < 1)
                return;

            Combo last = PeekCombo();

            if (last.StartsWith(currentReader.CurrentCombo + keyChar))
            {
                currentReader.Append(keyChar);
            }

            if (currentReader.IsFilled)
            {
                PopCombo();
                currentReader.Push();
            }
        }

        protected override void PostDestroy()
        {
            base.PostDestroy();

            FrameEvents.Update.Remove(_onUpdate);
            Input.OnKeyPress -= OnKeyPressed;
            
            comboStack.Clear();

            OnComboPush = null;
            OnComboPop = null;

            comboStack = null;
            rigidbody = null;
        }
    }
}