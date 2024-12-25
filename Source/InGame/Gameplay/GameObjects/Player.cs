using Engine;
using Engine.Modules;
using Engine.Types;
using GlobalTypes;
using GlobalTypes.Events;
using GlobalTypes.InputManagement;
using GlobalTypes.Interfaces;
using InGame.Interfaces;
using InGame.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace InGame.GameObjects
{
    public class Player : StringObject, IPersistentObject
    {
        public class MovementManager : IDestroyable
        {
            public Vector2 Velocity => rigidbody.velocity;

            public bool IsDestroyed { get; set; } = false;

            public bool CanMove { get; set; } = true;
            public bool CanJump { get; set; } = true;
            public bool IsJumpHoldEnabled { get; set; } = true;

            public float JumpPower { get; set; } = 8f;
            public float BaseJumpPower { get; private set; } = 8f;
            public float MoveSpeed { get; set; } = 5;

            public Key JumpKey { get; set; } = Key.Up;
            public Key LeftKey { get; set; } = Key.Left;
            public Key RightKey { get; set; } = Key.Right;

            private readonly float coyoteJumpTime = 0.1f;
            private StepTask coyoteJumpTask;

            private Collider collider;
            private Rigidbody rigidbody;

            private bool hasJump = true;

            private List<Collider> grounds = new();
            private List<Collider> previousGrounds = new();
            private OrderedAction onUpdate;

            private KeyBinding jumpBinding;

            public MovementManager(Player player)
            {
                rigidbody = player.rigidbody;
                collider = player.collider;

                onUpdate = FrameEvents.Update.Append(Update);

                coyoteJumpTask = new(() => DisableJump(coyoteJumpTime), false);

                jumpBinding = new(JumpKey, GetBindType(IsJumpHoldEnabled), Jump);
                Input.AddBind(jumpBinding);
            }

            public void ResetJump()
            {
                coyoteJumpTask?.Break();
                hasJump = true;
            }
            public void DisableJump()
            {
                hasJump = false;
            }

            public void SetHoldJumping(bool enabled)
            {
                IsJumpHoldEnabled = enabled;
                jumpBinding.TriggerPhase = GetBindType(enabled);
            }

            private void Update()
            {
                if (!CanMove)
                    return;

                float dir = (Input.IsKeyDown(LeftKey) ? -1 : 0) + (Input.IsKeyDown(RightKey) ? 1 : 0);
                
                Move(dir);
                UpdateGrounds();
            }

            private void Move(float xDir)
            {
                rigidbody.velocity.X = xDir.Clamp(-1, 1) * MoveSpeed;
            }
            private void Jump()
            {
                if (hasJump && CanJump)
                {
                    rigidbody.velocity.Y = 0;
                    rigidbody.velocity.Y -= JumpPower;

                    hasJump = false;
                }
            }

            private void UpdateGrounds()
            {
                previousGrounds = grounds;

                grounds =
                    collider.Intersections
                    .Where(c => c != null && c.Owner != null)
                    .Where(c => c.Owner.ContainsModule<Rigidbody>())
                    .Where(c => collider.GetMTV(c).Y > 0).ToList();

                bool isOnGround = IsOnGround();

                if (!isOnGround && previousGrounds.Any())
                {
                    coyoteJumpTask.Restart();
                }
                else if (isOnGround)
                {
                    hasJump = true;
                }
            }

            private IEnumerator DisableJump(float delaySeconds)
            {
                yield return StepTask.Delay(delaySeconds);
                hasJump = false;
            }

            private bool IsOnGround() => grounds.Any();
            private static KeyPhase GetBindType(bool autoJumpEnabled) => autoJumpEnabled ? KeyPhase.Hold : KeyPhase.Press;

            public void Destroy() => IDestroyable.Destroy(this);
            public void ForceDestroy()
            {
                FrameEvents.Update.Remove(onUpdate);
                Input.Unbind(jumpBinding);

                coyoteJumpTask?.Break();
            }

            ~MovementManager() => Monoconsole.WriteLine("Movement dector");
        }
        public class CodeManager : IDestroyable
        {
            public bool IsDestroyed { get; set; } = false;

            public int StackSize { get; set; } = 3;
            
            public bool CanCombinate { get; set; } = true;
            public bool CanManuallyPop { get; set; } = true;

            public event Action<Code> Pushed, Popped, ManuallyPopped;

            private ICodeReader currentReader = null;
            
            private Collider collider;

            private Stack<Code> stack = new();
            private StepTask delayedPush;

            private KeyBinding popBind = null;

            public CodeManager(Player player)
            {
                collider = player.collider;

                collider.ColliderEnter += OnColliderEnter;
                collider.ColliderExit += OnColliderExit;
                
                Input.KeyPressed += OnKeyPress;

                popBind = Input.Bind(Key.Space, KeyPhase.Press, () =>
                {
                    if (stack.Count < 1 || !CanManuallyPop)
                        return;

                    Code c = Pop();
                   
                    ManuallyPopped?.Invoke(c);
                });
            }

            public void Push(Code code)
            {
                void PushCode()
                {
                    stack.Push(code);
                    TrySetCode();

                    Pushed?.Invoke(code);
                }

                if (stack.Count >= StackSize)
                {
                    Pop();

                    delayedPush?.Dispose();
                    delayedPush = StepTask.RunDelayed(PushCode, () => StepTask.Delay(0.5f));
                }
                else
                {
                    delayedPush?.Dispose();
                    PushCode();
                }
            }
            public Code Pop()
            {
                Code code = stack.Pop();
                TrySetCode();
                Popped?.Invoke(code);

                return code;
            }
            public Code Peek() => stack.Peek();
            public bool HasAny() => stack.Count > 0;
            public void Clear()
            {
                delayedPush?.Break();
                
                while (stack.Count > 0)
                {
                    Pop();
                }
            }

            public void UnregisterReader() => currentReader = null;

            private void OnKeyPress(Key key)
            {
                string keyStr = key.ToString();
                if (keyStr.Length > 2 || (keyStr.Length == 2 && keyStr.First() != 'D'))
                    return;
                
                if (currentReader == null || stack.Count < 1 || !CanCombinate)
                    return;

                char keyChar = (char)key;

                currentReader.Append(keyChar);

                if (currentReader.IsFilled)
                {
                    currentReader.Push();
                    Pop();
                }
            }

            private void OnColliderEnter(Collider other)
            {
                if (other.Owner is ICodeReader reader)
                {
                    currentReader = reader;
                    reader.Activate();

                    if (stack.Count > 0)
                        reader.SetCode(Peek());
                }
            }
            private void OnColliderExit(Collider other)
            {
                if (other.Owner is ICodeReader reader)
                {
                    currentReader = null;
                    reader.Deactivate();
                }
            }

            private void TrySetCode()
            {
                if (stack.Count > 0)
                {
                    currentReader?.SetCode(Peek());
                }
                else
                {
                    currentReader?.SetCode(null);
                }
            }

            public void Destroy() => IDestroyable.Destroy(this);
            public void ForceDestroy()
            {
                Input.KeyPressed -= OnKeyPress;
                Input.Unbind(popBind);
            }

            ~CodeManager() => Monoconsole.WriteLine($"Code dector");
        }
        public class BitWalletManager
        {
            public int BitCount 
            { 
                get => _bitCount; 
                private set 
                {
                    _bitCount = value;
                    ValueChanged?.Invoke(_bitCount);
                }
            }

            public event Action<int> ValueChanged;
            public event Action<int> PayDeclined, PaySuccessful, Deposited;

            private int _bitCount = 0;

            public bool HasEnoughBits(int price) => BitCount >= price;
            
            public bool TrySpend(int amount)
            {
                if (amount < 0)
                    throw new ArgumentOutOfRangeException(nameof(amount), "Spend amount must be greater than or equal to zero.");

                if (HasEnoughBits(amount))
                {
                    BitCount -= amount;

                    PaySuccessful?.Invoke(amount);
                    return true;
                }
                else
                {
                    PayDeclined?.Invoke(amount);
                    return false;
                }
            }

            public void Deposit(int amount)
            {
                if (amount < 0)
                    throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than or equal to zero.");

                BitCount += amount;
                Deposited?.Invoke(amount);
            }

            ~BitWalletManager()
            {
                //Monoconsole.WriteLine($"BitWallet dector");
            }
        }

        public static Key ShowUIKey => Key.Tab;

        public static event Action<Player> Created;
        public static event Action ShowUIRequested, HideUIRequested;

        private static KeyBinding showUIBind, hideUIBind;

        public MovementManager Movement { get; private set; }
        public CodeManager Codes { get; private set; }
        public BitWalletManager BitWallet { get; private set; }

        private Rigidbody rigidbody;
        private Collider collider;

        private KeyBinding interactBind = null;

        public Player() : base("0", Fonts.SilkBold, true, 1) 
        {
            Scale = new(1.9f, 2);

            Origin -= new Vector2(0f, 1f);

            List<ObjectModule> modules = AddModules(
            new Collider()
            {
                Shape = Polygon.Rectangle(29f, 30)
            },
            new Rigidbody()
            {
                MaxVelocity = new(50, 50),
            });

            collider = modules[0] as Collider;
            rigidbody = modules[1] as Rigidbody;

            Movement = new(this);
            Codes = new(this);
            BitWallet = new();

            collider.ColliderEnter += OnColliderEnter;
            collider.ColliderExit += OnColliderExit;
            collider.IsShapeVisible = false;

            Level.Created += OnLevelCreate;
            Created?.Invoke(this);
        }

        [Init]
        private static void Init()
        {
            showUIBind = Input.Bind(ShowUIKey, KeyPhase.Press, () => ShowUIRequested?.Invoke());
            hideUIBind = Input.Bind(ShowUIKey, KeyPhase.Release, () => HideUIRequested?.Invoke());

            Input.Bind(Key.Escape, KeyPhase.Press, () => SessionManager.Freeze());
            Input.Bind(Key.Escape, KeyPhase.Release, () => SessionManager.Unfreeze());
        }

        private void OnColliderEnter(Collider other)
        {
            if (other.Owner is IInteractable interactable)
            {
                if (interactBind != null)
                {
                    Input.Unbind(interactBind);
                }

                interactBind = Input.Bind(Key.Down, KeyPhase.Press, () => interactable.Interact(this));
            }
        }
        private void OnColliderExit(Collider other)
        {
            if (other.Owner is IInteractable && interactBind != null)
            {
                Input.Unbind(interactBind);
            }
        }

        public void OnLevelCreate()
        {
            if (rigidbody != null)
                rigidbody.velocity = Vector2.Zero;

            Position = Level.TopZones.RandomElement();

            Codes.UnregisterReader();
            Codes.Clear();
        }
        
        public override void ForceDestroy()
        {
            base.ForceDestroy();

            Level.Created -= OnLevelCreate;
            
            Movement.ForceDestroy();
            Codes.ForceDestroy();

            BitWallet = null;
            Movement = null;
            Codes = null;

            if (interactBind != null)
                Input.Unbind(interactBind);
        }

        ~Player() => Monoconsole.WriteLine($"Player dector");
    }
}