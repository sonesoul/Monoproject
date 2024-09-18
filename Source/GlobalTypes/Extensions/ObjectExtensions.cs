using System;

namespace GlobalTypes.Extensions
{
    public static class ObjectExtensions
    {
        public static T As<T>(this object value)
        {
            if (value is T t)
                return t;

            return default;
        }
        public static bool Is<T>(this object value, out T newValue)
        {
            newValue = default;

            if (value is T changed)
            {
                newValue = changed;
                return true;
            }
            
            return false;
        }

        public static T To<T>(this object value)
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
    }
}