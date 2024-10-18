using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Engine.Drawing
{
    [Obsolete]
    public interface IDrawer
    {
        public void DrawAll();
        public void AddDrawAction(params Action[] drawActions);

        public void RemoveDrawAction(Action drawAction);

        public SpriteBatch SpriteBatch { get; }
        public GraphicsDevice GraphicsDevice { get; }
        public int Layer { get; }
    }
    [Obsolete]
    public abstract class OLDDrawer : IDrawer
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

    [Obsolete]
    public class OLDInterfaceDrawer : OLDDrawer
    {
        private static OLDInterfaceDrawer _instance;
        private OLDInterfaceDrawer(SpriteBatch batch, GraphicsDevice device) => Init(batch, device);

        public static OLDInterfaceDrawer CreateInstance(SpriteBatch batch = null, GraphicsDevice device = null)
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
        public static OLDInterfaceDrawer Instance => _instance;
        public override int Layer => 0;
    }
    [Obsolete]
    public class OLDIngameDrawer : OLDDrawer
    {
        private static OLDIngameDrawer _instance;
        private OLDIngameDrawer(SpriteBatch batch, GraphicsDevice device) => Init(batch, device);
        public static OLDIngameDrawer CreateInstance(SpriteBatch batch = null, GraphicsDevice device = null)
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

        public static OLDIngameDrawer Instance => _instance;
        public override int Layer => 1;
    }
}