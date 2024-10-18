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
using System.Windows.Forms;

namespace InGame.GameObjects
{
    public class Player : StringObject, ILevelObject
    {
        public static bool DrawVisuals { get; set; } = true;

        public string Tag => nameof(Player);
        public bool IsInitialized { get; private set; } = false;
        public bool IsDestructed { get; private set; } = true;

        public bool CanMove { get; set; } = true;
        public bool CanCombinate { get; set; } = false;
        public bool CanRollCombos { get; set; } = true;

        public float JumpPower { get; set; } = 9.5f;
        public float MoveSpeed { get; set; } = 4;

        public IReadOnlyList<Combo> Combos => combos;

        public static event Action<Combo> OnComboAdd, OnComboRemove;

        private readonly List<Key> pressedKeys = new();
        
        private Collider collider;
        private Rigidbody rigidbody;

        private OrderedAction _onUpdate;
        
        private StepTask _comboRollTask = null;
        private Vector2 targetPosition;

        private readonly List<Combo> combos = new();
        private readonly int poolSize = 4;
        
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

            _comboRollTask ??= new(RollCombos, true);
            
            Input.OnKeyPress += OnKeyPressed;
            Input.Bind(Key.MouseRight, KeyPhase.Hold, Move);

            Drawer.Register(context =>
            {
                if ((targetPosition - Position).Length() > 5)
                    context.Line(
                        Position,
                        targetPosition,
                        Color.WhiteSmoke);
            });
            targetPosition = Position;
        }

        public void Destruct() => Destroy();
        public void Reset()
        {
            if (rigidbody != null)
                rigidbody.velocity = Vector2.Zero;
        }

        public void AddCombo(Combo combo)
        {
            combos.Add(combo);

            if (combos.Count > poolSize)
                RemoveCombo(combos[0]);

            OnComboAdd?.Invoke(combo);
        }
        public void RemoveCombo(Combo combo)
        {
            combos.Remove(combo);
            OnComboRemove?.Invoke(combo);
        }
        public void RemoveComboAt(int index) => RemoveCombo(combos[index]);

        private void Update()
        {
            if ((targetPosition - Position).Length() < 5)
            {
                rigidbody.velocity = Vector2.Zero;
            }
            else if (CanMove)
            {
                rigidbody.velocity = (targetPosition - Position).Normalized() * MoveSpeed;
            }
        }

        private IEnumerator RollCombos()
        {
            while (true) 
            {
                yield return StepTask.WaitWhile(() => !CanRollCombos);
                yield return StepTask.WaitForSeconds(1.5f);
                
                AddCombo(Combo.NewRandom());

                yield return StepTask.WaitForSeconds(1.5f);
            }
        }

        
        private void Move()
        {
            Vector2 mousePos = FrameInfo.MousePosition;

            if (!collider.ContainsPoint(mousePos))
            {
                targetPosition = mousePos;
            }
        }
        private void OnKeyPressed(Key key)
        {
            char keyChar = (char)key;

            if (!Level.KeyPattern.Contains(keyChar))
                return;

            var fillables = collider.Intersections.Select(c => c.Owner).OfType<IFillable>().ToList();
            
            if (!fillables.Any())
                return;

            foreach (var filler in fillables)
            {
                if (combos.Where(c => c.StartsWith(filler.CurrentCombo + keyChar)).Any())
                {
                    filler.Append(keyChar);
                     
                    if (filler.IsFilled)
                    {
                        Combo combo = new(filler.CurrentCombo);
                        filler.Push();

                        RemoveCombo(combo);
                        //AddCombo(Combo.NewRandom());
                    }
                }
            }
            
        }

        protected override void PostDestroy()
        {
            base.PostDestroy();

            FrameEvents.Update.Remove(_onUpdate);
            Input.OnKeyPress -= OnKeyPressed;

            rigidbody = null;
            collider = null;
            _comboRollTask.Break();
        }
    }
}