﻿using System;

namespace GlobalTypes.Extensions
{
    public static class ActionExtensions
    {
        public static void Wrap(this Action action, Action<Action> before, Action<Action> after)
        {
            void Callback()
            {
                action();
                after(Callback);
            }

            before(Callback);
        }

        public static void Wrap<T>(this Action<T> action, Action<Action<T>> before, Action<Action<T>> after)
        {
            void Callback(T arg)
            {
                action(arg);
                after(Callback);
            }

            before(Callback);
        }
    }
}