using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using Engine.FrameDrawing;
using System.Collections.Generic;
using System.Linq;
using Engine.Modules;
using System.Reflection;

namespace Engine
{
    public abstract class GameObject
    {
        public Vector2 position = new(0, 0);
        public float rotation = 0;
        public Color color = Color.White;

        public IDrawer drawer;
        public SpriteBatch spriteBatch;
        public Vector2 viewport;

        protected List<ObjectModule> modules = new();

        protected readonly Action<GameTime> drawAction;

        public Vector2 IntegerPosition => new((int)Math.Round(position.X), (int)Math.Round(position.Y));
        public IReadOnlyList<ObjectModule> Modules => modules.ToArray();

        public GameObject(IDrawer drawer)
        {
            this.drawer = drawer;
            spriteBatch = drawer.SpriteBatch;
            drawAction = Draw;

            drawer.AddDrawAction(drawAction);
            viewport = new(spriteBatch.GraphicsDevice.Viewport.Width, spriteBatch.GraphicsDevice.Viewport.Height);
        }
        public virtual void Destroy() => drawer.RemoveDrawAction(drawAction);

        protected abstract void Draw(GameTime gameTime);

        public T AddModule<T>() where T : ObjectModule
        {
            var module = (T)Activator.CreateInstance(typeof(T), args: this);

            if (!ContainsModule<T>())
                modules.Add(module);
            else
                modules[modules.IndexOf(module)] = module;

            return module;
        }
        public void AddModule<T>(T module)
            where T : ObjectModule => modules.Add(module);
        public bool RemoveModule<T>() 
            where T : ObjectModule => RemoveModule(modules.OfType<T>().FirstOrDefault());
        public bool RemoveModule<T>(T module) where T : ObjectModule 
        {
            bool res = modules.Remove(module);
            if (res)
            {
                var method = module.GetType().GetMethod("DestructInvoke", BindingFlags.NonPublic | BindingFlags.Instance);
                method?.Invoke(module, null);
            }            

            return res;
        }
        public T GetModule<T>() 
            where T : ObjectModule => Modules.OfType<T>().FirstOrDefault();
        public ObjectModule[] GetModulesOf<T>() 
            where T : ObjectModule => Modules.OfType<T>().ToArray();
        public bool TryGetModule<T>(out T module)
            where T : ObjectModule => (module = Modules.OfType<T>().FirstOrDefault()) != null;
        public bool ContainsModule<T>()
            where T : ObjectModule => Modules.OfType<T>().Any();
        public bool ContainsModule<T>(T module)
           where T : ObjectModule => Modules.Where(m => m == module).Any();
    }
    public class TextObject : GameObject
    {
        public string sprite;
        public SpriteFont font;
        public Vector2 size = Vector2.One;
        public Vector2 center;

        public TextObject(IDrawer drawer, string sprite, SpriteFont font) : base(drawer)
        {
            center = font.MeasureString(sprite) / 2;

            this.sprite = sprite;
            this.font = font;
        }
        protected override void Draw(GameTime gameTime)
        {
            bool canDraw = position.Y >= 0 && position.Y <= viewport.Y && position.X >= 0 && position.X <= viewport.X;
            
            if (canDraw)
                spriteBatch.DrawString(font, sprite, IntegerPosition, color, rotation.ToRad(), center, size, SpriteEffects.None, 0);
        }
    }
}