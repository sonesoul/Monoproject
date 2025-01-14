﻿using Engine;
using Engine.Modules;
using Engine.Types;
using GlobalTypes;
using InGame.Interfaces;
using System;
using System.Collections;

namespace InGame.GameObjects
{
    public class StorageFiller : ModularObject, ILevelObject, ICodeReader
    {
        public CodeStorage Storage { get; set; }

        public string Sequence { get; set; } = "";
        public bool IsInputEnabled { get; set; } = true;

        public int Size { get; private set; }
        public bool IsActive { get; private set; }
        public int MistakeCount { get; private set; }
        public bool IsFilled => Sequence.Length >= Size;

        public Code? TargetCode { get; private set; } = null;
        public char? TargetChar => (IsFilled || !TargetCode.HasValue) ? null : TargetCode.Value[Sequence.Length];

        public float TotalInputTime { get; private set; } = 0;

        public float LastInputTime { get; private set; } = 0;

        public event Action Activated, Deactivated, Cleared;
        public event Action<char> CharAdded;
        public event Action<Code> Pushed;

        public event Action MistakeOccured, InputEnabled;
        public event Action<Code?> TargetCodeChanged;

        private StepTask inputTimeCounting = null;
        private Player player = null;
        private float timeBuffer = 0;

        public StorageFiller()
        {
            Size = LevelConfig.CodeLength;

            Action setStorage = () => Storage = Level.GetObject<CodeStorage>();
            setStorage.Wrap(w => Level.Created += w, w => Level.Created -= w);
            
            AddModule(new Collider()
            {
                Shape = Polygon.Rectangle(50, 50),
                IsShapeVisible = false
            });
        }

        public void Push()
        {
            if (!IsFilled || !IsActive)
                return;

            FixateCount();

            Code code = new(Sequence);
            
            Storage.Push(code);
            SetCode(null);
            Pushed?.Invoke(code);
        }
        public void Append(char c)
        {
            if (!IsActive)
                return;

            c = char.ToUpper(c);

            if (!IsFilled && TargetCode.TryGetValue(out var targetCode) && IsInputEnabled)
            {
                if (!inputTimeCounting?.IsRunning ?? true)
                    StartCount();

                if (TargetChar.Value != c)
                {
                    HandleMistake();
                    return;
                }

                player.Grade.AddPoints(0.01f);
                Sequence += c;
                Sfx.Play(Sounds.CodeInput);
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

        public void Activate(Player player)
        {
            if (IsActive)
                return;
            this.player = player;
            IsActive = true;
            
            Activated?.Invoke();
        }
        public void Deactivate() 
        {
            if (!IsActive)
                return;

            IsActive = false;
            SetCode(null);

            BreakCount();

            Deactivated?.Invoke();
        }

        public void HandleMistake()
        {
            Clear();
            
            IsInputEnabled = false;
            MistakeCount++;

            BreakCount();

            StepTask.RunDelayed(() =>
            {
                IsInputEnabled = true;
                InputEnabled?.Invoke();
                StartCount();
            }, () => StepTask.Delay(0.5f));
            
            MistakeOccured?.Invoke();
        }

        public override void ForceDestroy()
        {
            base.ForceDestroy();

            Activated = null;
            Deactivated = null;
            Cleared = null;
            CharAdded = null;
            Pushed = null;
        }

        private void StartCount() => StepTask.Replace(ref inputTimeCounting, CountTask);
        private void FixateCount()
        {
            LastInputTime = timeBuffer;
            BreakCount();
        }
        private void BreakCount()
        {
            inputTimeCounting?.Break();
            inputTimeCounting = null;
            timeBuffer = 0;
        }

        private IEnumerator CountTask()
        {
            while (true)
            {
                timeBuffer += FrameState.DeltaTime;
                yield return null;
            }
        }
        //~StorageFiller() => Monoconsole.WriteLine("Filler dector");
    }
}