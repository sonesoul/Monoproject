using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using Engine.Drawing;
using System.Collections.Generic;
using System.Linq;
using Engine.Modules;
using System.Diagnostics;
using Engine.Types;
using Microsoft.Xna.Framework.Input;
using GlobalTypes.Collections;

namespace Engine
{
    public interface IRenderable
    {
        public IDrawer Drawer { get; }
        public void Draw(GameTime gameTime);
    }

    [DebuggerDisplay("{ToString(),nq}")]
    public abstract class ModularObject
    {
        public Vector2 position = new(0, 0);
        public Vector2 IntegerPosition => position.Rounded();
        public float Rotation { get; set; } = 0;

        public static T New<T>(params object[] args) where T : ModularObject => (T)Activator.CreateInstance(typeof(T), args);
        public virtual void Destroy()
        {
            for (int i = Modules.Count - 1; i >= 0; i--)
                RemoveModule(modules[i]);
        }

        #region ModuleManagement

        private readonly LockList<ObjectModule> modules = new();
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
        public void AddModule<T>(T module) where T : ObjectModule
        {
            if (module == null)
                throw new ArgumentException($"Module cannot be null ({typeof(T).Name}).");

            if (ContainsModule<T>())
                throw new ArgumentException($"Module already exists ({typeof(T).Name}).");

            modules.Add(module);
            module.SetOwner(this);
        }
        
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

            return;
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
    public class TextObject : ModularObject, IRenderable
    {
        public string text;
        public SpriteFont font;
        public Vector2 size = Vector2.One;
        public Vector2 center;

        public Color color = Color.White;
        public Vector2 viewport;
        private SpriteBatch spriteBatch;
        public IDrawer Drawer { get; private set; }

        public TextObject(IDrawer drawer, string text, SpriteFont font) : base()
        {
            spriteBatch = drawer.SpriteBatch;

            Viewport view = spriteBatch.GraphicsDevice.Viewport;
            viewport = new(view.Width, view.Height);

            center = font.MeasureString(text) / 2;

            Drawer = drawer;
            drawer.AddDrawAction(Draw);

            this.text = text;
            this.font = font;
        }
       
        public void Draw(GameTime gameTime)
        {
            bool canDraw = position.Y >= 0 && position.Y <= viewport.Y && position.X >= 0 && position.X <= viewport.X;
            
            if (canDraw)
            {
                spriteBatch.DrawString(
                    font,
                    text,
                    IntegerPosition,
                    color,
                    Rotation.AsRad(),
                    center,
                    size,
                    SpriteEffects.None,
                    0);
            }
        }
        public override void Destroy()
        {
            Drawer.RemoveDrawAction(Draw);
            base.Destroy();
        }


        public override string ToString() => text;
    }

    public class InputZone : TextObject
    {
        private readonly Queue<char> charQueue = new();
  
        public InputZone(IDrawer drawer, string text, SpriteFont font) : base(drawer, text, font)
        {
            Collider collider = new(this)
            {
                Mode = ColliderMode.Trigger,
                polygon = Polygon.Rectangle(center.X * 2, 30)
            };
            collider.TriggerStay += ObjectStay;

            AddModule(collider);

            foreach (var item in text.ToLower())
            {
                charQueue.Enqueue(item);
            }
        }
        private void ObjectStay(Collider obj)
        {
            if(obj.Owner is TextObject textobj && textobj.text == "#")
            {
                Keys[] keys = Keyboard.GetState().GetPressedKeys();

                for (int i = 0; i < keys.Length; i++)
                {
                    string keyString = keys[i].ToString().ToLower();

                    if (keyString.Length > 1)
                        continue;

                    if (keyString[0] == charQueue.Peek())
                    {
                        charQueue.Dequeue();
                        if (text.Length > 1)
                        {
                            text = text[1..];
                            center = font.MeasureString(text) / 2;
                        }
                        else if (text.Length == 1)
                            Destroy();

                        break;
                    }
                }
            }            
        }
    }
}