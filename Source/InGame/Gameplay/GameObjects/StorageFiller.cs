using Engine;
using Engine.Modules;
using Engine.Types;
using GlobalTypes;
using GlobalTypes.Interfaces;
using InGame.Interfaces;
using InGame.Overlays;
using System;

namespace InGame.GameObjects
{
    public class StorageFiller : ModularObject, ILevelObject, ICodeReader
    {
        public CodeStorage Storage { get; set; }

        public string Sequence { get; set; } = "";
        public bool IsInputEnabled { get; set; } = true;
        public int Size { get; private set; }

        public bool IsFilled => Sequence.Length >= Size;
        public bool IsActive { get; private set; }


        public event Action Activated, Deactivated, Cleared;
        public event Action<char> CharAdded;
        public event Action<Code> Pushed;

        public event Action InputFailed, InputEnabled, InputDisabled;
        public event Action<Code?> TargetCodeChanged;

        public Code? TargetCode { get; private set; } = null;
        public char? TargetChar => (IsFilled || !TargetCode.HasValue) ? null : TargetCode.Value[Sequence.Length];

        public StorageFiller()
        {
            Size = LevelConfig.CodeSize;

            Level.Created += GetStorage;
            
            AddModule(new Collider()
            {
                Shape = Polygon.Rectangle(50, 50),
                IsShapeVisible = false
            });
        }

        public bool Push()
        {
            if (IsFilled)
            {
                Code code = new(Sequence);
                bool isPushed = Storage.Push(code);
                SetCode(null);
                
                Pushed?.Invoke(code);

                return isPushed;
            }

            return false;
        }
        public void Append(char c)
        {
            c = char.ToUpper(c);

            if (!IsFilled && TargetCode.TryGetValue(out var targetCode) && IsInputEnabled)
            {
                if (TargetChar.Value != c)
                {
                    LockInput();
                    return;
                }

                Sequence += c;
                CharAdded?.Invoke(c);
            }
        }
        public void Clear()
        {
            Sequence = "";
            Cleared?.Invoke();
        }

        public void SetCode(Code? code)
        {
            if (Storage.IsFilled)
                code = null;

            TargetCode = code;
            Clear();

            TargetCodeChanged?.Invoke(code);
        }

        public void Activate()
        {
            if (IsActive)
                return;

            IsActive = true;
            Activated?.Invoke();
        }
        public void Deactivate() 
        {
            if (!IsActive)
                return;

            IsActive = false;
            Deactivated?.Invoke();

            SetCode(null);
        }

        public void LockInput()
        {
            Clear();
            IsInputEnabled = false;
            InputDisabled?.Invoke();

            StepTask.RunDelayed(() =>
            {
                IsInputEnabled = true;
                InputEnabled?.Invoke();
            }, () => StepTask.Delay(0.5f));


            InputFailed?.Invoke();
        }

        private void GetStorage()
        {
            Storage = Level.GetObject<CodeStorage>();
        }

        public override void ForceDestroy()
        {
            base.ForceDestroy();

            Activated = null;
            Deactivated = null;
            Cleared = null;
            CharAdded = null;
            Pushed = null;

            Level.Created -= GetStorage;
        }


        ~StorageFiller()
        {
            //Monoconsole.WriteLine("Filler dector");
        }
    }
}