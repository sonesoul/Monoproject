using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Modules;
using System.Diagnostics;
using GlobalTypes.Events;
using GlobalTypes.Interfaces;

namespace Engine
{
    [DebuggerDisplay("{ToString(),nq}")]
    public abstract class ModularObject : IDestroyable
    {
        public Vector2 Position { get; set; } = new(0, 0);
        public Vector2 IntegerPosition => Position.IntCast();

        public Vector2 Scale { get; set; } = Vector2.One;

        public float RotationDeg { get; set; } = 0;
        public float RotationRad => RotationDeg.Deg2Rad();
        
        public bool IsDestroyed { get; set; } = false;

        public event Action<ModularObject> Destroyed;

        public void Destroy()
        {
            if (IsDestroyed)
                return;

            IsDestroyed = true;

            FrameEvents.EndSingle.Add(ForceDestroy, EndSingleOrders.Destroy);
        }
        public virtual void ForceDestroy()
        {
            for (int i = Modules.Count - 1; i >= 0; i--)
                RemoveModule(modules[i], true);

            ModuleRemoved = null;

            Destroyed?.Invoke(this);
            Destroyed = null;
        }

        #region ModuleManagement

        public IReadOnlyList<ObjectModule> Modules => modules;
        public event Action<ObjectModule> ModuleRemoved;

        private readonly List<ObjectModule> modules = new();
        
        private static ObjectModule InitModule(Type type, params object[] args) => (ObjectModule)Activator.CreateInstance(type, args: args);

        public T AddModule<T>() where T : ObjectModule
        {
            if (typeof(T).IsAbstract)
                throw new ArgumentException($"Module can't be abstract ({typeof(T).Name}).");

            return AddModule((T)InitModule(typeof(T), new object[] { this }));
        }
        public T AddModule<T>(T module) where T : ObjectModule
        {
            if (module == null)
                throw new ArgumentException($"Module cannot be null ({typeof(T).Name}).");

            if (ContainsModule(module))
                throw new ArgumentException($"Module already exists ({typeof(T).Name}).");

            modules.Add(module);

            if (!module.IsConstructed)
                module.Construct(this);
            else if (module.Owner != this)
                module.SetOwner(this);

            return module;
        }
        public List<ObjectModule> AddModules(params ObjectModule[] modules)
        {
            List<ObjectModule> createdModules = new();

            foreach (var module in modules)
                createdModules.Add(AddModule(module));

            return createdModules;
        }
        public List<ObjectModule> AddModule<T1, T2>() where T1 : ObjectModule where T2 : ObjectModule
            => new()
            {
                AddModule<T1>(),
                AddModule<T2>()
            };
        public List<ObjectModule> AddModule<T1, T2, T3>() where T1 : ObjectModule where T2 : ObjectModule where T3 : ObjectModule
            => AddModule<T1, T2>().Append(AddModule<T3>()).ToList();
        public List<ObjectModule> AddModule<T1, T2, T3, T4>() where T1 : ObjectModule where T2 : ObjectModule where T3 : ObjectModule where T4 : ObjectModule
            => AddModule<T1, T2, T3>().Append(AddModule<T4>()).ToList();

        public void RemoveModule<T>(bool forced = false) where T : ObjectModule 
            => RemoveModule(Modules.OfType<T>().FirstOrDefault(), forced);
        public void RemoveModule<T>(T module, bool forced = false) where T : ObjectModule
        {
            if (module == null || !ContainsModule(module))
                return;

            modules.Remove(module);
            ModuleRemoved?.Invoke(module);

            if (!module.IsDestroyed)
            {
                if (forced)
                    module.ForceDestroy();
                else
                    module.Destroy();
            }
        }
        
        public T ReplaceModule<T>() where T : ObjectModule
        {
            if(typeof(T).IsAbstract)
                throw new ArgumentException("Module to replace is abstract.");

            if (TryGetModule(out T oldModule))
                RemoveModule(oldModule);

            return AddModule<T>();
        }
        public void ReplaceModule<T>(T newModule) where T : ObjectModule
        {
            if (ContainsModule<T>())
                RemoveModule<T>();

            AddModule(newModule);
        }
        
        public T GetModule<T>() where T : ObjectModule  
            => Modules.OfType<T>().FirstOrDefault();
        public bool TryGetModule<T>(out T module) where T : ObjectModule  
            => (module = GetModule<T>()) != null;
        public ObjectModule[] GetModulesOf<T>() where T : ObjectModule  
            => Modules.OfType<T>().ToArray();

        public bool ContainsModule<T>() where T : ObjectModule  
            => Modules.OfType<T>().Any();
        public bool ContainsModule<T>(T module) where T : ObjectModule  
            => Modules.Contains(module);
        #endregion

        public override string ToString() => $"{Position} ({modules.Count})";
    }
}