using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Engine.Drawing
{
    public interface IDrawer
    {
        public void DrawAll();
        public void AddDrawAction(params Action[] drawActions);

        public void RemoveDrawAction(Action drawAction);

        public SpriteBatch SpriteBatch { get; }
        public GraphicsDevice GraphicsDevice { get; }
        public int Layer { get; }
    }
    public abstract class Drawer : IDrawer
    {
        protected SpriteBatch _spriteBatch;
        protected GraphicsDevice _graphicsDevice;
        protected List<Action> _drawActions = new();

        protected void Init(SpriteBatch batch, GraphicsDevice device)
        {
            _graphicsDevice = device;
            _spriteBatch = batch;
        }

        public virtual void DrawAll()
        {
            foreach (var item in _drawActions)
            {
                item.Invoke();
            }
        }
        public virtual void AddDrawAction(Action drawAction)
        {
            if (drawAction != null)
                _drawActions.Add(drawAction);
        }
        public virtual void AddDrawAction(params Action[] drawActions)
        {
            foreach (var item in drawActions)
            {
                if (item != null)
                    _drawActions.Add(item);
            }
        }

        public virtual void RemoveDrawAction(Action drawAction)
        {
            if (drawAction != null)
                _drawActions.Remove(drawAction);
        }

        public SpriteBatch SpriteBatch => _spriteBatch;
        public GraphicsDevice GraphicsDevice => _graphicsDevice;

        public abstract int Layer { get; }
    }

    public class InterfaceDrawer : Drawer
    {
        private static InterfaceDrawer _instance;
        private InterfaceDrawer(SpriteBatch batch, GraphicsDevice device) => Init(batch, device);

        public static InterfaceDrawer CreateInstance(SpriteBatch batch = null, GraphicsDevice device = null)
        {
            if (_instance == null)
            {
                if (batch == null)
                    throw new ArgumentNullException(nameof(batch));
                else if (device == null)
                    throw new ArgumentNullException(nameof(device));
                
                _instance = new(batch, device);
            }
            return _instance;
        }
        public static InterfaceDrawer Instance => _instance;
        public override int Layer => 0;
    }
    public class IngameDrawer : Drawer
    {
        private static IngameDrawer _instance;
        private IngameDrawer(SpriteBatch batch, GraphicsDevice device) => Init(batch, device);
        public static IngameDrawer CreateInstance(SpriteBatch batch = null, GraphicsDevice device = null)
        {
            if (_instance == null)
            {
                if (batch == null)
                    throw new ArgumentNullException(nameof(batch));
                else if (device == null)
                    throw new ArgumentNullException(nameof(device));

                _instance = new(batch, device);
            }
            return _instance;
        }

        public static IngameDrawer Instance => _instance;
        public override int Layer => 1;
    }
}