using Engine;
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
    public class Player : StringObject, IPersistentObject
    {
        public class MovementManager
        {
            public bool CanMove { get; private set; } = true;
            public bool CanJump { get; private set; } = true;

            public float JumpPower { get; set; } = 9f;
            public float BaseJumpPower { get; private set; } = 9f;
            public float MoveSpeed { get; set; } = 4;

            public Key JumpKey { get; set; } = Key.Up;
            public Key LeftKey { get; set; } = Key.Left;
            public Key RightKey { get; set; } = Key.Right;

            private readonly float coyoteJumpTime = 0.1f;
            private StepTask coyoteJumpTask;

            private Collider collider;
            private Rigidbody rigidbody;

            private List<Collider> grounds = new();

            private OrderedAction onUpdate;

            public MovementManager(Player player)
            {
                rigidbody = player.rigidbody;
                collider = player.collider;

                Input.OnKeyHold += OnKeyHold;
                onUpdate = FrameEvents.Update.Append(Update);

                collider.OnOverlapEnter += OnColliderEnter;
                collider.OnOverlapExit += OnColliderExit;
            }

            public bool IsOnGround() =>
                collider.Intersections.Any(c => collider.GetMTV(c).Y > 0 && c.Owner.ContainsModule<Rigidbody>());

            public void EnableMovement()
            {
                CanMove = true;
            }
            public void DisableMovement()
            {
                CanMove = false;
            }

            public void EnableJump()
            {
                coyoteJumpTask?.Break();
                CanJump = true;
            }
            public void DisableJump()
            {
                CanJump = false;
            }

            private void Update()
            {
                Move(Input.Axis.X);
            }
            private void Move(float xDir)
            {
                rigidbody.velocity.X = xDir.Clamp(-1, 1) * MoveSpeed;
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
            
            private void OnKeyHold(Key key)
            {
                if (key == JumpKey)
                {
                    Jump();
                }
            }

            private void OnColliderEnter(Collider other)
            {
                if (collider.GetMTV(other).Y > 0 && other.Owner.ContainsModule<Rigidbody>())
                {
                    EnableJump();
                    grounds.Add(other);
                }
            }
            private void OnColliderExit(Collider other)
            {
                if (grounds.Remove(other))
                {
                    if (!IsOnGround())
                    {
                        coyoteJumpTask = StepTask.Run(DisableJump(coyoteJumpTime));
                    }
                }
            }

            private IEnumerator DisableJump(float delaySeconds)
            {
                yield return StepTask.WaitForSeconds(delaySeconds);
                CanJump = false;
            }

            public void Destroy()
            {
                collider.OnOverlapEnter -= OnColliderEnter;
                collider.OnOverlapExit -= OnColliderExit;

                Input.OnKeyHold -= OnKeyHold;

                FrameEvents.Update.Remove(onUpdate);
                
                collider = null;
            }
        }
        public class ComboManager
        {
            public int ComboStackSize { get; set; } = 3;
            public int PopCooldown { get; set; } = 1;

            public event Action<Combo> OnPush, OnPop, OnManualPop;

            private IComboReader currentReader = null;
            private List<Combo> comboStack = new();
            private Collider collider;

            public ComboManager(Player player)
            {
                collider = player.collider;

                collider.OnOverlapEnter += OnColliderEnter;
                collider.OnOverlapExit += OnColliderExit;
                
                Input.OnKeyPress += OnKeyPress;

                Input.Bind(Key.Space, KeyPhase.Press, () =>
                {
                    if (comboStack.Count < 1 || comboStack.Count > ComboStackSize)
                        return;

                    var pop = Pop();
                    OnManualPop?.Invoke(pop);
                });
            }

            public void Push(Combo combo)
            {
                comboStack.Insert(0, combo);
                OnPush?.Invoke(combo);

                if (comboStack.Count > ComboStackSize)
                {
                    StepTask.Run(PopWithDelay(2f));
                    return;
                }
            }
            public Combo Pop()
            {
                var combo = comboStack.First();
                comboStack.RemoveAt(0);

                OnPop?.Invoke(combo);

                return combo;
            }
            public Combo Peek() => comboStack.First();
            public Combo PopAt(int index)
            {
                var combo = comboStack[index];
                comboStack.RemoveAt(index);

                OnPop?.Invoke(combo);

                return combo;
            }

            private void OnKeyPress(Key key)
            {
                char keyChar = (char)key;

                if (!Level.KeyPattern.Contains(keyChar) || currentReader == null || comboStack.Count < 1)
                    return;

                Combo last = Peek();

                if (last.StartsWith(currentReader.CurrentCombo + keyChar))
                {
                    currentReader.Append(keyChar);
                }

                if (currentReader.IsFilled)
                {
                    Pop();
                    currentReader.Push();
                }
            }

            private void OnColliderEnter(Collider other)
            {
                if (other.Owner is IComboReader comboReader)
                {
                    currentReader = comboReader;
                    comboReader.Activate();
                }
            }
            private void OnColliderExit(Collider other)
            {
                if (other.Owner is IComboReader comboReader)
                {
                    currentReader = null;
                    comboReader.Deactivate();
                    return;
                }
            }

            private IEnumerator PopWithDelay(float delaySeconds)
            {
                yield return StepTask.WaitForSeconds(delaySeconds);
                yield return StepTask.WaitWhile(() => currentReader != null);

                if (comboStack.Count > 0)
                    Pop();
            }

            public void Destroy()
            {
                Input.OnKeyPress -= OnKeyPress;

                collider.OnOverlapEnter -= OnColliderEnter;
                collider.OnOverlapExit -= OnColliderExit;
                collider = null;

                OnPush = null;
                OnPop = null;
                OnManualPop = null;
            }
        }
        public class HealthManager
        {
            public float HP { get; private set; }
            public float MaxHP { get; private set; }

            public event Action<float> OnDamageTaken, OnHealApplied;
            public event Action<float> OnHPChanged;

            public HealthManager(float maxHp = 5)
            {
                MaxHP = maxHp;
                HP = MaxHP;

                OnHPChanged += value =>
                {
                    if (value < 0)
                        OnDamageTaken?.Invoke(-value);
                    else 
                        OnHealApplied?.Invoke(value);
                };
            }

            public void TakeDamage(float damage)
            {
                if (damage < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(damage), "Damage can't be less than zero.");
                }

                HP -= damage;

                OnHPChanged?.Invoke(-damage);
            }
            public void Heal(float healAmount)
            {
                if (healAmount < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(healAmount), "Healing amount can't be less than zero.");
                }

                HP += healAmount;
                
                if (HP > MaxHP)
                    HP = MaxHP;

                OnHPChanged?.Invoke(healAmount);
            } 

            public void Destroy()
            {
                OnDamageTaken = null;
                OnHealApplied = null;
                OnHPChanged = null;
            }
        }

        public static event Action<Player> OnCreate;

        public MovementManager Movement { get; private set; }
        public ComboManager Combo { get; private set; }
        public HealthManager Health { get; private set; }

        private Rigidbody rigidbody;
        private Collider collider;

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

            Movement = new(this);
            Combo = new(this);
            Health = new();


            OnCreate?.Invoke(this);
        }

        public void OnLoad()
        {
            if (rigidbody != null)
                rigidbody.velocity = Vector2.Zero;

            Position = Level.AbovePlatformTiles.RandomElement();
        }
        public void OnRemove() => Destroy();

        protected override void PostDestroy()
        {
            base.PostDestroy();

            Movement.Destroy();
            Combo.Destroy();
            Health.Destroy();

            rigidbody = null;
            collider = null;
        }
    }
}