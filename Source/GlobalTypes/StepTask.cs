using System.Collections;
using GlobalTypes.Events;
using System;
using System.Collections.Generic;

namespace GlobalTypes
{
    public class StepTask
    {
        private static class StepTaskManager
        {
            private readonly static List<StepTask> tasks = new();

            [Init]
            private static void Init()
            {
                FrameEvents.Update.Add(() => Update(), UpdateOrders.StepTaskManager);
            }

            private static void Update()
            {
                for (int i = tasks.Count - 1; i >= 0; i--)
                {
                    if (tasks[i].IsPaused)
                        continue;

                    var action = tasks[i].workingTask;

                    if (action.Current is IEnumerator subCoroutine)
                    {
                        if (subCoroutine.MoveNext())
                            continue;
                    }
                    
                    if (!action.MoveNext())
                    {
                        tasks[i].OnComplete?.Invoke();
                        tasks.RemoveAt(i);
                    }
                }
            }

            public static void Register(StepTask orderedTask) => tasks.Add(orderedTask);
            public static void Unregister(StepTask orderedTask) => tasks.Remove(orderedTask);
        }

        public bool IsRunning => item != null && !IsPaused;
        public bool IsPaused { get; set; } = false;
        public int Order { get; set; }
        public Func<IEnumerator> Task { get; set; }

        public event Action OnComplete;

        private IEnumerator workingTask;
        private StepTask item = null;

        public StepTask(Func<IEnumerator> task, bool run = false)
        {
            Task = task;

            if (run)
                Start();
        }
        
        public void Start()
        {
            workingTask = Task() ?? throw new ArgumentNullException(nameof(Task), "Action can't be null.");
            item = this;
            StepTaskManager.Register(item);
        }
        public void Restart()
        {
            if (IsRunning)
                Break();

            Start();
        }
        
        public void Pause()
        {
            if (IsPaused)
                return;

            IsPaused = true;
        }
        public void Resume()
        {
            if (!IsPaused)
                return;

            IsPaused = false;
        }

        public void Break()
        {
            if (item != null)
            {
                StepTaskManager.Unregister(item);
                item = null;
            }
        }

        public static StepTask Run(Func<IEnumerator> action) => new(action, true);
        public static StepTask Run(IEnumerator task) => Run(() => task);

        #region WaitMethods
        
        public static IEnumerator WaitForSeconds(float seconds) => WaitForFrames((int)(seconds / FrameState.DeltaTime));
        public static IEnumerator WaitForRealSeconds(float seconds)
        {
            TimeSpan start = FrameState.GameTime.TotalGameTime;

            while ((FrameState.GameTime.TotalGameTime - start).TotalSeconds < seconds)
            {
                yield return null;
            }
        }
        public static IEnumerator WaitForFrames(int frames)
        {
            while (frames > 0)
            {
                frames--;
                yield return null;
            }
        }
        public static IEnumerator WaitWhile(Func<bool> condition)
        {
            while (condition())
            {
                yield return null;
            }
        }
        public static IEnumerator WaitUntil(Func<bool> condition)
        {
            while (!condition())
            {
                yield return null;
            }
        }

        #endregion
    }
}