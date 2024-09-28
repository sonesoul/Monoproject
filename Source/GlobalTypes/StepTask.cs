using System.Collections;
using GlobalTypes.Events;
using System;
using GlobalTypes.Collections;
using System.Threading.Tasks;

namespace GlobalTypes
{
    public class StepTask
    {
        [Init(nameof(Init), InitOrders.StepTaskManager)]
        private static class StepTaskManager
        {
            private readonly static OrderedList<StepTask> tasks = new();
            private static void Init()
            {
                FrameEvents.Update.Add(() => Update(), UpdateOrders.CoroutineManager);
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
                        tasks.RemoveAt(i);
                    }
                }
            }

            public static OrderedItem<StepTask> Append(StepTask task) => tasks.Append(task);
            public static void Add(OrderedItem<StepTask> orderedTask) => tasks.Add(orderedTask);
            public static void RemoveFirst(StepTask task) => tasks.RemoveFirst(task);
            public static void Remove(OrderedItem<StepTask> orderedTask) => tasks.Remove(orderedTask);
            public static bool Contains(StepTask coroutine) => tasks.Contains(coroutine);
        }

        public bool IsRunning => item != null && !IsPaused;
        public bool IsPaused { get; set; } = false;
        public int Order { get; set; }
        public Func<IEnumerator> Task { get; set; }

        public event Action Completed;

        private IEnumerator workingTask;
        private OrderedItem<StepTask>? item = null;

        public StepTask(Func<IEnumerator> task, bool run = false)
        {
            Task = task;

            if (run)
                Start();
        }
        public void Start()
        {
            workingTask = Task() ?? throw new ArgumentNullException(nameof(Task), "Action can't be null.");
            item = StepTaskManager.Append(this);
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
            if (item.HasValue)
            {
                StepTaskManager.Remove(item.Value);
                item = null;
            }
        }

        public static StepTask Run(Func<IEnumerator> action) => new(action, true);

        #region WaitMethods
        
        public static IEnumerator WaitForSeconds(float seconds) => WaitForFrames((int)(seconds / FrameInfo.DeltaTime));
        public static IEnumerator WaitForRealSeconds(float seconds)
        {
            TimeSpan start = FrameInfo.GameTime.TotalGameTime;

            while ((FrameInfo.GameTime.TotalGameTime - start).TotalSeconds < seconds)
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