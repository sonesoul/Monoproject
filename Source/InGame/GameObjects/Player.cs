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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace InGame.GameObjects
{
    public class Player : StringObject, ILevelObject
    {
        public string Tag => nameof(Player);
        public bool IsInitialized { get; private set; } = false;
        public bool IsDestructed { get; private set; } = true;

        public bool CanMove { get; set; } = true;
        public bool CanCombinate { get; set; } = false;
        
        public float JumpPower { get; set; } = 9f;
        public float MoveSpeed { get; set; } = 4;

        public static event Action<Combo> OnComboAdd, OnComboRemove;

        private readonly List<Key> pressedKeys = new();
        
        private Collider collider;
        private Rigidbody rigidbody;

        private OrderedAction _onUpdate;
        
        public Player() : base("#", UI.Silk, true) 
        {
            CharColor = Color.Green;
            Scale = new(2, 2);
            OriginOffset = new(-0.5f, -1.5f);
        }

        public void Init()
        {
            if (IsInitialized)
                return;

            IsInitialized = true;

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
            
            Input.Bind(Key.Up, KeyPhase.Press, () =>
            {
                rigidbody.velocity.Y = 0;
                rigidbody.velocity.Y -= JumpPower;
            });
        }

        public void Destruct() => Destroy();
        public void Reset()
        {
            if (rigidbody != null)
                rigidbody.velocity = Vector2.Zero;
        }

        private void Update()
        {
            rigidbody.velocity.X = Input.Axis.X * MoveSpeed;
        }

        private void OnKeyPressed(Key key)
        {
            char keyChar = (char)key;

            if (!Level.KeyPattern.Contains(keyChar))
                return;

            
        }

        protected override void PostDestroy()
        {
            base.PostDestroy();

            FrameEvents.Update.Remove(_onUpdate);
            Input.OnKeyPress -= OnKeyPressed;

            rigidbody = null;
            collider = null;
        }
    }
}