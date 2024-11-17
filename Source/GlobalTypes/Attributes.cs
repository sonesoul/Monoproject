using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GlobalTypes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public abstract class BaseInitAttribute : Attribute
    {
        public int Order { get; }

        protected BaseInitAttribute(int order = 0)
        {
            Order = order;
        }

        protected static void Invoke<TAttribute>() where TAttribute : BaseInitAttribute
        {
            List<MethodInfo> methods =
                Assembly.GetExecutingAssembly()
            .GetTypes()
            .SelectMany(t => t.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
            .Where(m => m.GetCustomAttributes<TAttribute>().Any()), (type, method) => new { Method = method, Attribute = method.GetCustomAttribute<TAttribute>() })
            .OrderBy(item => item.Attribute.Order)
            .Select(item => item.Method)
            .ToList();

            foreach (var item in methods)
            {
                if (!item.IsStatic)
                {
                    throw new InvalidOperationException(
                        $"Method {item.Name} in {item.DeclaringType.Name} class must be static to use {typeof(TAttribute).Name} attribute.");
                }

                item.Invoke(null, null);
            }
        }
    }

    public class InitAttribute : BaseInitAttribute
    {
        public InitAttribute(int order = 0) : base(order) { }
        public static void Invoke() => Invoke<InitAttribute>();
    }
    public class LoadAttribute : BaseInitAttribute
    {
        public LoadAttribute(int order = 0) : base(order) { }
        public static void Invoke() => Invoke<LoadAttribute>();
    }
}