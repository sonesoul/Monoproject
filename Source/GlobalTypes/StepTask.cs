using System.Collections;
using GlobalTypes.Events;
using System;
using GlobalTypes.Collections;

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
                FrameEvents.Update.Add(gt => Update(), UpdateOrders.CoroutineManager);
            }

            private static void Update()
            {
                for (int i = tasks.Count - 1; i >= 0; i--)
                {
                    if (!tasks[i].Action.MoveNext())
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

        public bool IsRunning => item != null;
        public int Order { get; set; }
        public IEnumerator Action { get; set; }

        private OrderedItem<StepTask>? item = null;

        public StepTask(IEnumerator action, bool run = false) 
        {
            Action = action ?? throw new ArgumentNullException(nameof(action), "Action can't be null.");

            if (run)
                Run();
        }
        public void Run() => item = StepTaskManager.Append(this);
        public void RunNew()
        {
            if (IsRunning)
                Break();

            Run();
        }
        public void Break()
        {
            if (item.HasValue)
            {
                StepTaskManager.Remove(item.Value);
                item = null;
            }
        }

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
    }
}