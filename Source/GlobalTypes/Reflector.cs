using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GlobalTypes
{
    public static class Reflector
    {
        public static T CreateInstance<T>(object[] args = null) where T : class
        {
            var type = typeof(T);
            if (CreateInstance(type, args) is T instance)
                return instance;

            throw new InvalidOperationException($"Unable to create an instance of {type.FullName}");
        }
        public static object CreateInstance(Type t, object[] args = null) => Activator.CreateInstance(t.GetType(), args);

        public static void CallMethod<T>(string methodName, T instance, object[] parameters = null)
        {
            CallMethod(typeof(T), methodName, instance, parameters);
        }
        public static void CallMethod(Type type, string methodName, object instance, object[] parameters = null)
        {
            BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;

            if (instance == null)
            {
                bindingFlags |= BindingFlags.Static;

                MethodInfo[] methods = type
                    .GetMethods(bindingFlags)
                    .Where(m => m.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase))
                    .ToArray();

                MethodInfo method = methods.Where(m => m.GetParameters().Length == (parameters?.Length ?? 0)).FirstOrDefault()
                    ?? throw new InvalidOperationException($"Method {methodName} with {parameters?.Length ?? 0} arguments not found.");
                method.Invoke(null, parameters);
            }
            else
            {
                MethodInfo method = type.GetMethod(methodName, bindingFlags)
                    ?? throw new InvalidOperationException($"Method {methodName} with {parameters?.Length ?? 0} arguments not found.");
                method.Invoke(instance, parameters);
            }
        }


        public static object GetFieldValue<T>(T instance, string fieldName)
        {
            BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;

            if (instance == null)
            {
                bindingFlags |= BindingFlags.Static;
                var type = typeof(T);
                FieldInfo fieldInfo = type.GetField(fieldName, bindingFlags)
                    ?? throw new InvalidOperationException($"Static field [{fieldName}] not found.");
                return fieldInfo.GetValue(null);
            }

            var instanceType = typeof(T);
            FieldInfo field = instanceType.GetField(fieldName, bindingFlags)
                ?? throw new InvalidOperationException($"Field [{fieldName}] not found.");
            return field.GetValue(instance);
        }
    }
}