using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using Engine.Drawing;
using System.Collections.Generic;
using System.Linq;
using Engine.Modules;
using System.Diagnostics;
using GlobalTypes.Collections;
using GlobalTypes.Events;

namespace Engine
{
    [DebuggerDisplay("{ToString(),nq}")]
    public abstract class ModularObject
    {
        public Vector2 position = new(0, 0);
        public Vector2 IntegerPosition => position.Rounded();
        public float RotationDeg { get; set; } = 0;
        public float RotationRad => RotationDeg.AsRad();
        public bool IsDestroyed { get; private set; } = false;


        public void Destroy() => FrameEvents.EndSingle.Add(gt => DestroyAction(), EndSingleOrders.Destroy);
        public void ForceDestroy() => DestroyAction();
        protected void DestroyAction() 
        {
            if (IsDestroyed) 
                return;
            
            IsDestroyed = true;

            PreDestroy();

            for (int i = Modules.Count - 1; i >= 0; i--)
                RemoveModule(modules[i]);
            
            PostDestroy();
        }

        protected virtual void PreDestroy() { }
        protected virtual void PostDestroy() { }

        #region ModuleManagement

        private readonly LockList<ObjectModule> modules = new();
        public event Action<ObjectModule> OnModuleRemove;
        public IReadOnlyList<ObjectModule> Modules => modules;
        
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

        public void RemoveModule<T>() where T : ObjectModule 
            => RemoveModule(Modules.OfType<T>().FirstOrDefault());
        public void RemoveModule<T>(T module) where T : ObjectModule
        {
            if (module == null || !ContainsModule(module))
                return;

            modules.Remove(module);
            OnModuleRemove?.Invoke(module);

            if (!module.IsDisposed)
                module.Dispose();
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

        public override string ToString() => $"{position} ({modules.Count})";
    }
    
    [DebuggerDisplay("{ToString(),nq}")]
    public class TextObject : ModularObject, Types.IRenderable
    {
        public string text;
        public SpriteFont font;
        public Vector2 size = Vector2.One;
        public Vector2 origin;
        public static Vector2 OriginOffset { get; set; } = new Vector2(-0.5f, 1);
        public Color Color { get; set; } = Color.White;
        public Vector2 viewport;
        private SpriteBatch spriteBatch;
        public IDrawer Drawer { get; private set; }

        public TextObject(IDrawer drawer, string text, SpriteFont font) : base()
        {
            spriteBatch = drawer.SpriteBatch;

            Viewport view = spriteBatch.GraphicsDevice.Viewport;
            viewport = new(view.Width, view.Height);

            origin = font.MeasureString(text) / 2;

            Drawer = drawer;
            drawer.AddDrawAction(Draw);

            this.text = text;
            this.font = font;
        }
        public TextObject(IDrawer drawer, string text, SpriteFont font, params ObjectModule[] modules) : this(drawer, text, font)
        {
            foreach (var module in modules) 
                AddModule(module);
        }
        public virtual void Draw(GameTime gameTime)
        {
            spriteBatch.DrawString(
                font,
                text,
                IntegerPosition,
                Color,
                RotationRad,
                origin + OriginOffset,
                size,
                SpriteEffects.None,
                0);
        }
        protected override void PostDestroy()
        {
            Drawer.RemoveDrawAction(Draw);
        }

        public override string ToString() => text;
    }
}