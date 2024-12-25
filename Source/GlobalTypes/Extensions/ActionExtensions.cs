using System;

namespace GlobalTypes.Extensions
{
    public static class ActionExtensions
    {
        public static void Invoke(this Action action, Action<Action> calling, Action<Action> since)
        {
            void Callback()
            {
                action();
                since(Callback);
            }

            calling(Callback);
        }

        public static void Invoke<T>(this Action<T> action, Action<Action<T>> calling, Action<Action<T>> since)
        {
            void Callback(T arg)
            {
                action(arg);
                since(Callback);
            }

            calling(Callback);
        }
    }
}