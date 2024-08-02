using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using Engine.FrameDrawing;
using System.Collections.Generic;
using System.Linq;
using Engine.Modules;
using System.Reflection;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Engine
{
    [DebuggerDisplay("{ToString(),nq}")]
    public abstract class ModularObject
    {
        public Vector2 position = new(0, 0);
        public Color color = Color.White;
        
        public IDrawer drawer;
        public Vector2 viewport;
        
        protected readonly Action<GameTime> drawAction;
        public Vector2 IntegerPosition => position.Rounded();
        public float Rotation { get; set; } = 0;

        public ModularObject(IDrawer drawer)
        {
            this.drawer = drawer;
            drawAction = Draw;
            drawer.AddDrawAction(drawAction);

            var spriteBatch = drawer.SpriteBatch;
            viewport = new(spriteBatch.GraphicsDevice.Viewport.Width, spriteBatch.GraphicsDevice.Viewport.Height);
        }

        public virtual void Destroy()
        {
            drawer.RemoveDrawAction(drawAction);
            modules.ForEach(m => RemoveModule(m));
        }
        protected abstract void Draw(GameTime gameTime);

        #region ModuleManagement

        private readonly List<ObjectModule> modules = new();
        public event Action<ObjectModule> OnModuleRemove;
        public IReadOnlyList<ObjectModule> Modules => modules;
        
        public T AddModule<T>() where T : ObjectModule
        {
            if (typeof(T).IsAbstract)
                throw new ArgumentException($"Module can't be abstract ({typeof(T).Name}).");

            var module = (T)Activator.CreateInstance(typeof(T), args: this);

            AddModule(module);

            return module;
        }
        public void AddModule<T>(T module)
            where T : ObjectModule
        {
            if (module == null)
                throw new ArgumentException($"Module cannot be null ({typeof(T).Name}).");

            if (ContainsModule<T>())
                throw new ArgumentException($"Module already exists ({typeof(T).Name}).");

            if (module.Owner != this)
                module.Owner?.RemoveModule(module);
            
            module.Owner = this;
            
            modules.Add(module);
        }
        public bool RemoveModule<T>() 
            where T : ObjectModule => RemoveModule(Modules.OfType<T>().FirstOrDefault());
        public bool RemoveModule<T>(T module) where T : ObjectModule
        {
            if (module == null)
                return false;

            bool isRemoved = modules.Remove(module);

            if (isRemoved)
            {
                OnModuleRemove?.Invoke(module);

                if(!module.Disposed)
                    module.Dispose();
            }

            return isRemoved;
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
        public T GetModule<T>()
            where T : ObjectModule => Modules.OfType<T>().FirstOrDefault();
        public ObjectModule[] GetModulesOf<T>() 
            where T : ObjectModule => Modules.OfType<T>().ToArray();
        public bool TryGetModule<T>(out T module)
            where T : ObjectModule => (module = GetModule<T>()) != null;
        public bool ContainsModule<T>()
            where T : ObjectModule => Modules.OfType<T>().Any();
        public bool ContainsModule<T>(T module)
           where T : ObjectModule => Modules.Contains(module);
        #endregion

        public override string ToString() => $"{position} ({modules.Count})";
    }
    
    [DebuggerDisplay("{ToString(),nq}")]
    public class TextObject : ModularObject
    {
        public string sprite;
        public SpriteFont font;
        public Vector2 size = Vector2.One;
        public Vector2 center;
        private SpriteBatch spriteBatch;

        public TextObject(IDrawer drawer, string sprite, SpriteFont font) : base(drawer)
        {
            center = font.MeasureString(sprite) / 2;
            spriteBatch = drawer.SpriteBatch;

            this.sprite = sprite;
            this.font = font;
        }
        protected override void Draw(GameTime gameTime)
        {
            bool canDraw = position.Y >= 0 && position.Y <= viewport.Y && position.X >= 0 && position.X <= viewport.X;
            
            if (canDraw)
            {
                spriteBatch.DrawString(
                    font,
                    sprite,
                    IntegerPosition,
                    color,
                    Rotation.AsRad(),
                    center,
                    size,
                    SpriteEffects.None,
                    0);
            }
        }

        public override string ToString() => sprite;
    }
}