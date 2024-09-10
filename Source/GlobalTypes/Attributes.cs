using System;
using System.Linq;
using System.Reflection;

namespace GlobalTypes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = true)]
    public abstract class BaseInitAttribute : Attribute
    {
        public string MethodName { get; }
        public int Order { get; }

        protected BaseInitAttribute(string methodName, int order = 0)
        {
            MethodName = methodName;
            Order = order;
        }

        protected static void Invoke<TAttribute>() where TAttribute : BaseInitAttribute
        {
            var methodsWithAttributes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .SelectMany(t => t.GetCustomAttributes<TAttribute>(), (t, attr) => new { Type = t, Attribute = attr })
                .OrderBy(item => item.Attribute.Order)
                .ToList();

            foreach (var item in methodsWithAttributes)
            {
                var type = item.Type;
                var methodName = item.Attribute.MethodName;

                var method = 
                    type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
                    ?? throw new InvalidOperationException($"Method '{methodName}' not found in class '{type.FullName}'.");

                if (method.IsStatic)
                    method.Invoke(null, null);
                else
                {
                    var instance = Activator.CreateInstance(type);
                    method.Invoke(instance, null);
                }
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = true)]
    public class InitAttribute : BaseInitAttribute
    {
        public InitAttribute(string methodName, int order = 0) : base(methodName, order) { }

        public static void Invoke() => Invoke<InitAttribute>();
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = true)]
    public class LoadAttribute : BaseInitAttribute
    {
        public LoadAttribute(string methodName, int order = 0) : base(methodName, order) { }

        public static void Invoke() => Invoke<LoadAttribute>();
    }
}