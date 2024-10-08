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
        [Init(nameof(Init))]
        private static class PlayerVisual
        {
            private static readonly List<ComboVisual> comboDisplays = new();
            private static Vector2 comboStartPosition = new(10, 10);
            private static float comboSpacing = 30f;
            
            public class ComboVisual
            {
                public Combo Combo { get; set; }
                public Vector2 Position { get; set; }
                public Vector2 TargetPosition { get; set; }

                public float Alpha { get; set; } = 1f;
                public bool IsDisappearing { get; set; } = false;

                public ComboVisual(Combo combo, Vector2 position)
                {
                    Combo = combo;
                    Position = position;
                    TargetPosition = position;
                }
            }

            private static void Init()
            {
                InterfaceDrawer.Instance.AddDrawAction(Draw);
                FrameEvents.Update.Append(Update);
            }

            private static void Update()
            {
                for (int i = comboDisplays.Count - 1; i >= 0; i--)
                {
                    var display = comboDisplays[i];

                    if (display.IsDisappearing)
                    {
                        display.Alpha -= 0.10f;
                        if (display.Alpha <= 0)
                        {
                            comboDisplays.RemoveAt(i);
                            UpdateTargetPositions();
                            continue;
                        }
                    }

                    display.Position = Vector2.Lerp(display.Position, display.TargetPosition, 0.2f);
                }
            }
            private static void Draw()
            {
                if (!DrawVisuals)
                    return;

                foreach (var display in comboDisplays)
                {
                    Color color = Color.White * display.Alpha;
                    InstanceInfo.SpriteBatch.DrawString(UI.Silk, display.Combo.ToString(), display.Position, color);
                }
            }

            public static void AddComboVisual(Combo combo)
            {
                Vector2 position = comboStartPosition + new Vector2(0, comboDisplays.Count * comboSpacing);
                comboDisplays.Add(new ComboVisual(combo, position));
            }
            public static void RemoveComboVisual(Combo combo)
            {
                var display = comboDisplays.FirstOrDefault(c => c.Combo == combo);
                if (display != null)
                {
                    display.IsDisappearing = true;
                }

                UpdateTargetPositions();
            }

            private static void UpdateTargetPositions()
            {
                for (int i = 0; i < comboDisplays.Count; i++)
                {
                    comboDisplays[i].TargetPosition = comboStartPosition + new Vector2(0, i * comboSpacing);
                }
            }
        }

        public static bool DrawVisuals { get; set; } = false;

        public string Tag => nameof(Player);
        public bool IsInitialized { get; private set; } = false;
        public bool IsDestructed { get; private set; } = true;

        public bool CanMove { get; set; } = true;
        public bool CanCombinate { get; set; } = false;
        public bool CanRollCombos { get; set; } = true;

        public float JumpPower { get; set; } = 9.5f;
        public float MoveSpeed { get; set; } = 4;

        public IReadOnlyList<Combo> Combos => combos;

        private readonly List<Key> pressedKeys = new();
        
        private Collider collider;
        private Rigidbody rigidbody;

        private OrderedAction _onUpdate;
        
        private StepTask _comboRollTask = null;

        private readonly List<Combo> combos = new();
        private readonly int poolSize = 4;

        public Player() : base(IngameDrawer.Instance, "#", UI.Silk) 
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

            PlayerVisual.AddComboVisual(combo);
        }
        public void RemoveCombo(Combo combo)
        {
            combos.Remove(combo);
            PlayerVisual.RemoveComboVisual(combo);
        }
        public void RemoveComboAt(int index)
        {
            Combo combo = combos[index];
            RemoveCombo(combo);
        }

        private void Update()
        {
            Move();
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
            if (CanMove)
            {
                rigidbody.velocity = Input.Axis * MoveSpeed;
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