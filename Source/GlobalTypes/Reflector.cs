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

        public static List<T> CreateInstances<T>() where T : class
        {
            var instances = new List<T>();

            var types = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => typeof(T).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var type in types)
            {
                if (Activator.CreateInstance(type) is T instance)
                    instances.Add(instance);
            }

            return instances;
        }
        public static List<Type> GetSubtypes<T>()
        {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => typeof(T).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToList();
        }

        public static void CallMethod<T>(T instance, string methodName, object[] parameters = null)
        {
            BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;

            if (instance == null)
            {
                bindingFlags |= BindingFlags.Static;
                var type = typeof(T);
                MethodInfo method = type.GetMethod(methodName, bindingFlags)
                    ?? throw new InvalidOperationException($"Static method [{methodName}] not found.");
                method.Invoke(null, parameters);
            }
            else
            {
                var instanceType = typeof(T);
                MethodInfo method = instanceType.GetMethod(methodName, bindingFlags)
                    ?? throw new InvalidOperationException($"Instance method [{methodName}] not found.");
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