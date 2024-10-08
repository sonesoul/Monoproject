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
using Engine.Types;
using GlobalTypes;

namespace Engine
{
    [DebuggerDisplay("{ToString(),nq}")]
    public abstract class ModularObject
    {
        public Vector2 Position { get; set; } = new(0, 0);
        public Vector2 IntegerPosition => Position.IntCast();

        public float RotationDeg { get; set; } = 0;
        public float RotationRad => RotationDeg.AsRad();
        
        public bool IsDestroyed { get; private set; } = false;

        public void Destroy() => FrameEvents.EndSingle.Add(DestroyAction, EndSingleOrders.Destroy);
        public void ForceDestroy() => DestroyAction();
        protected void DestroyAction() 
        {
            if (IsDestroyed) 
                return;
            
            IsDestroyed = true;

            PreDestroy();

            for (int i = Modules.Count - 1; i >= 0; i--)
                RemoveModule(modules[i], true);
            
            PostDestroy();
        }

        protected virtual void PreDestroy() { }
        protected virtual void PostDestroy() { }

        #region ModuleManagement

        public IReadOnlyList<ObjectModule> Modules => modules;
        public event Action<ObjectModule> OnModuleRemove;

        private readonly LockList<ObjectModule> modules = new();
        
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
            OnModuleRemove?.Invoke(module);

            if (!module.IsDisposed)
            {
                if (forced)
                    module.ForceDispose();
                else
                    module.Dispose();
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

    [DebuggerDisplay("{ToString(),nq}")]
    public class CharObject : ModularObject, IRenderable
    {
        public char Character { get; set; }
        public Vector2 Offset { get; set; } = Vector2.Zero;
        public Vector2 Origin { get; set; } = Vector2.Zero;
        public Vector2 Scale { get; set; } = Vector2.One;
        public Color Color { get; set; } = Color.White;
        public SpriteEffects SpriteEffects { get; set; }
        public SpriteFont Font { get; set; }

        public IDrawer Drawer { get; private set; }
        public bool CanDraw { get; set; } = true;

        public CharObject(IDrawer drawer, char character, SpriteFont font)
        {
            drawer.AddDrawAction(Draw);
            Font = font;

            Character = character;
            Origin = Font.MeasureString(character.ToString()) / 2;
        }
        public CharObject(IDrawer drawer, char character, SpriteFont font, params ObjectModule[] modules) : this(drawer, character, font)
        {
            foreach (var module in modules)
                AddModule(module);
        }

        public void Draw()
        {
            if (CanDraw)
            {
                Drawer.SpriteBatch.DrawString(
                    Font,
                    Character.ToString(),
                    Position,
                    Color,
                    RotationRad,
                    Origin,
                    Scale,
                    SpriteEffects,
                    0);
            }
        }

        protected override void PostDestroy()
        {
            Drawer.RemoveDrawAction(Draw);
        }
        public override string ToString() => Character.ToString();
    }

    [DebuggerDisplay("{ToString(),nq}")]
    public class StringObject : ModularObject, IRenderable
    {
        public string Content
        {
            get => content;
            set
            {
                content = value;
                SliceString();
            }
        }
        public Vector2 Scale { get; set; } = Vector2.One;
        public Vector2 Origin { get; set; }
        public Vector2 OriginOffset { get; set; } = Vector2.Zero;//new(-0.5f, 1);
        public Color CharColor
        {
            get => charColor; set
            {
                charColor = value;
                SliceString();
            }
        }
        public SpriteFont Font { get; set; }

        public float Spacing { get => Font.Spacing; set => Font.Spacing = value; }
        public bool CanDraw { get; set; } = true;
        public int Length => characters.Count;
        public IDrawer Drawer { get; private set; }

        private string content;
        private Color charColor = Color.White;
        private RenderTarget2D textTexture;
        private readonly SpriteBatch spriteBatch;
        private readonly List<CharObject> characters = new();

        public StringObject(IDrawer drawer, string content, SpriteFont font) : base()
        {
            spriteBatch = drawer.SpriteBatch;
            
            Origin = font.MeasureString(content) / 2;
            
            Drawer = drawer;
            drawer.AddDrawAction(Draw);

            Font = font;
            Content = content;
        }
        public StringObject(IDrawer drawer, string text, SpriteFont font, params ObjectModule[] modules) : this(drawer, text, font)
        {
            foreach (var module in modules) 
                AddModule(module);
        }
        
        public CharObject SetChar(int index) => characters[index];
        public void SwapChars(int index1, int index2)
        {
            (characters[index1], characters[index2]) = (characters[index2], characters[index1]);
        }

        public virtual void Draw()
        {
            if (!CanDraw) 
                return;

            foreach (var charObj in characters)
            {
                spriteBatch.DrawString(
                    charObj.Font,
                    charObj.Character.ToString(),
                    IntegerPosition + charObj.Position,
                    charObj.Color,
                    charObj.RotationRad,
                    Origin + OriginOffset,
                    Scale * charObj.Scale,
                    SpriteEffects.None,
                    0);
            }
        }

        private void SliceString()
        {
            characters.Clear();
            Vector2 position = Vector2.Zero;

            Vector2 startPosition = new(0, 0);

            foreach (char character in Content)
            {
                CharObject c = new(Drawer, character, Font)
                {
                    Position = startPosition + position,
                    Color = CharColor,
                    CanDraw = false
                };
                characters.Add(c);

                Vector2 size = Font.MeasureString(character.ToString());
                position.X += size.X + Spacing;
            }
        }

        private void BuildTexture()
        {
            Vector2 textSize = Font.MeasureString(Content);

            if (textSize.Both() <= 0)
                return;

            textTexture = new RenderTarget2D(spriteBatch.GraphicsDevice, (int)textSize.X, (int)textSize.Y);

            spriteBatch.GraphicsDevice.SetRenderTarget(textTexture);
            spriteBatch.GraphicsDevice.Clear(Color.Transparent);

            spriteBatch.Begin();
            spriteBatch.DrawString(Font, Content, Vector2.Zero, CharColor);
            spriteBatch.End();

            spriteBatch.GraphicsDevice.SetRenderTarget(null);

            Origin = textSize / 2;
        }

        protected override void PostDestroy()
        {
            Drawer.RemoveDrawAction(Draw);
        }
        public override string ToString() => Content;
    }
}