using Engine.Modules;
using Engine.Types;
using Engine;
using InGame.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace InGame.GameObjects
{
    public class StaticPlatform : ILevelObject
    {
        public Vector2 Position { get; private set; }

        public StringObject Object { get; private set; }

        public StaticPlatform(Vector2 position)
        {
            Position = position;
        }

        public void OnAdd()
        {
            if (Object == null)
            {
                Rigidbody rigidbody = new() 
                { 
                    BodyType = BodyType.Static 
                };
                Collider collider = new()
                {
                    Shape = Polygon.Rectangle(Level.TileSize)
                };

                Object = new("", UI.Silk, true, collider, rigidbody)
                {
                    Position = this.Position
                };
            }

        }

        public void OnRemove() 
        {
            Object?.Destroy();
            Object = null;
        }
    }
}