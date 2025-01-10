using System.Collections;
using GlobalTypes.Events;
using System;
using System.Collections.Generic;
using System.Linq;

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
                FrameEvents.UpdateUnscaled.Add(() => UpdateUnscaled(), UpdateUnscaledOrders.StepTaskManager);
            }

            private static void Update()
            {
                UpdateTasks(tasks.Where(t => t.IsTimeScaled).ToList());
            }
            private static void UpdateUnscaled()
            {
                UpdateTasks(tasks.Where(t => !t.IsTimeScaled).ToList());
            }

            private static void UpdateTasks(List<StepTask> tasks)
            {
                for (int i = tasks.Count - 1; i >= 0; i--)
                {
                    if (tasks[i].IsPaused)
                        continue;

                    StepTask task = tasks[i];
                    IEnumerator iterator = task.Iterator;

                    Stack<IEnumerator> nestedTasks = GetNestedTasks(iterator);

                    if (NextNested(nestedTasks))
                        continue;

                    try
                    {
                        if (!iterator.MoveNext())
                        {
                            task.Complete();
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new AggregateException($"StepTask inner exception: {ex.Message} ({task.Action.Method.Name})");
                    }
                    
                }
            }

            private static Stack<IEnumerator> GetNestedTasks(IEnumerator iterator)
            {
                Stack<IEnumerator> nestedTasks = new();

                while (iterator.Current is IEnumerator subTask)
                {
                    iterator = subTask;

                    if (subTask != null)
                        nestedTasks.Push(iterator);
                }

                return nestedTasks;
            }
            private static bool NextNested(Stack<IEnumerator> nestedTasks)
            {
                while (nestedTasks.Count > 0)
                {
                    if (nestedTasks.Peek().MoveNext())
                        break;

                    nestedTasks.Pop();
                }

                return nestedTasks.Count > 0;
            }

            public static void Register(StepTask orderedTask) => tasks.Add(orderedTask);
            public static void Unregister(StepTask orderedTask) => tasks.Remove(orderedTask);
        }

        public Func<IEnumerator> Action { get; set; }
        public IEnumerator Iterator { get; private set; }

        public bool IsPaused { get; set; } = false;
        public bool IsTimeScaled { get; set; } = true;
        public bool IsRunning => updateItem != null && !IsPaused;

        public event Action<StepTask> Completed;

        private StepTask updateItem = null;

        public StepTask(Func<IEnumerator> action)
        {
            Action = action;
        }
        public StepTask(Func<IEnumerator> action, bool isTimeScaled) : this(action) 
        {
            IsTimeScaled = isTimeScaled;
        }
        public StepTask(Func<IEnumerator> action, bool isTimeScaled, bool run) : this(action, isTimeScaled)
        {
            if (run)
                Start();
        }
        
        public void Start()
        {
            Iterator = Action() ?? throw new ArgumentNullException(nameof(Action), "Action can't be null.");
            updateItem = this;
            StepTaskManager.Register(updateItem);
        }
        public void Restart()
        {
            if (IsRunning)
                Break();

            Start();
        }
        
        public void Pause() => IsPaused = true;
        public void Resume() => IsPaused = false;

        public void Break()
        {
            if (updateItem != null)
            {
                StepTaskManager.Unregister(updateItem);
                updateItem = null;
            }
        }
        public void Complete()
        {
            Break();
            Completed?.Invoke(this);
        }

        #region Static
        public static StepTask Run(Func<IEnumerator> action, bool isTimeScaled = true)
        {
            return new(action, isTimeScaled, true);
        }
        public static StepTask Run(IEnumerator iterator, bool isTimeScaled = true)
        {
            return Run(() => iterator, isTimeScaled);
        }

        public static void Replace(ref StepTask task, Func<IEnumerator> iterator, bool isTimeScaled = true)
        {
            task?.Break();
            task = Run(iterator, isTimeScaled);
        }
        public static void Replace(ref StepTask task, IEnumerator iterator, bool isTimeScaled = true)
        {
            task?.Break();
            task = Run(iterator, isTimeScaled);
        }

        public static StepTask RunDelayed(Action action, Func<IEnumerator> waitUntil, bool isTimeScaled = true)
        {
            IEnumerator Task()
            {
                yield return waitUntil();
                action();
            }

            return Run(Task(), isTimeScaled);
        }
        #endregion

        #region WaitMethods

        public delegate void RefFloatInterpolation(ref float e);

        public static IEnumerator Delay(float seconds) => WaitForFrames((int)(seconds / FrameState.DeltaTime));
        public static IEnumerator DelayUnscaled(float seconds) => WaitForFrames((int)(seconds / FrameState.DeltaTimeUnscaled));
        public static IEnumerator RealTimeDelay(float seconds)
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

        public static IEnumerator Interpolate(RefFloatInterpolation action)
        {
            float elapsed = 0;

            while (elapsed >= 0 && elapsed <= 1)
            {
                action(ref elapsed);

                if (elapsed < 0 || elapsed > 1)
                {
                    yield return null;

                    elapsed = elapsed.Clamp01();
                    action(ref elapsed);

                    break;
                }

                yield return null;
            }
        }

        #endregion
    }
}