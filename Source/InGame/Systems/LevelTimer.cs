using GlobalTypes;
using System;
using System.Collections;

namespace InGame
{
    public class LevelTimer
    {
        public int SecondsLeft { get; private set; }
        public int MillisecondsLeft { get; private set; } = 1000;
        public float TimeLeft => SecondsLeft + (MillisecondsLeft / 1000);

        public bool IsTimeHidden { get; set; } = false;

        public bool IsTimeOver { get; private set; } = false;
        public bool IsRunning { get; private set; } = true;

        public event Action<int> TickS, TickMs;
        public event Action TimeOver;

        private string _hiddenTimeString = "--";
        private StepTask _countTask = null;

        public LevelTimer(float seconds, bool start = true)
        {
            int s = (int)seconds;
            int ms = (int)((seconds - s) * 1000);

            SetTime(s, ms);

            IsRunning = start;

            if (start) 
            {
                Start();
            }
        }

        public void Start()
        {
            IsRunning = true;
            _countTask = StepTask.Run(CountTask());
        }
        public void Stop()
        {
            IsRunning = false;
            _countTask?.Break();
        }
        public void SetTime(int seconds, int milliseconds = 0)
        {
            if (!IsRunning)
                return;

            ClampTime(seconds, out seconds);
            
            SecondsLeft = seconds;
            
            MillisecondsLeft = milliseconds;
        }

        public void AddSeconds(int seconds)
        {
            if (!IsRunning)
                return;

            ClampTime(seconds, out seconds);

            SecondsLeft += seconds;
        }
        public void AddMilliseconds(int milliseconds)
        {
            if (!IsRunning)
                return;

            ClampTime(milliseconds, out milliseconds);

            MillisecondsLeft += milliseconds;

            while (MillisecondsLeft >= 1000)
            {
                SecondsLeft++;
                MillisecondsLeft -= 1000;
            }
        }

        private static void ClampTime(int value, out int clamped)
        {
            clamped = value.ClampMin(0);
        }

        private void Tick(int milliseconds)
        {
            MillisecondsLeft -= milliseconds;

            if (MillisecondsLeft < 0 && SecondsLeft > 0)
            {
                MillisecondsLeft = 1000;
                SecondsLeft--;
                TickS?.Invoke(SecondsLeft);
            }

            TickMs?.Invoke(milliseconds);
        }
        private IEnumerator CountTask()
        {
            while (SecondsLeft > 0 || MillisecondsLeft > 0)
            {
                if (IsRunning)
                {
                    Tick(16);

                    if (SecondsLeft <= 0)
                    {
                        SecondsLeft = 0;
                        MillisecondsLeft = 0;

                        break;
                    }
                }

                yield return null;
            }

            IsTimeOver = true;
            TimeOver?.Invoke();
        }

        public override string ToString() => IsTimeHidden ? _hiddenTimeString : $"{SecondsLeft:00}";
    }
}