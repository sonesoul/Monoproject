using Engine.Drawing;
using Engine.Modules;
using Engine.Types.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics;

namespace Engine
{
    [DebuggerDisplay("{ToString(),nq}")]
    public class StringObject : ModularObject, IRenderable
    {
        public string Text { get; set; }

        public Vector2 Origin { get; set; }
        public Vector2 OriginOffset { get; set; } = Vector2.Zero;
        public Color DrawColor { get; set; } = Palette.White;
        public SpriteFont UsedFont { get; set; }
        public bool MatrixDepend { get; private set; }

        public bool IsVisible { get; set; } = true;

        public StringObject(string content, SpriteFont font, bool matrixDepend, int layer = -1) : base()
        {
            MatrixDepend = matrixDepend;

            Origin = font.MeasureString(content) / 2;

            Drawer.Register(Draw, matrixDepend, layer);

            UsedFont = font;
            Text = content;
        }
        public StringObject(string content, SpriteFont font, bool matrixDepend, params ObjectModule[] modules) : this(content, font, matrixDepend)
        {
            foreach (var module in modules)
                AddModule(module);
        }

        public virtual void Draw(DrawContext context)
        {
            if (IsVisible)
            {
                DrawOptions options = new()
                {
                    color = DrawColor,
                    position = IntegerPosition,
                    font = UsedFont,
                    origin = Origin,
                    rotationDeg = RotationDeg,
                    scale = Scale,
                };

                context.String(Text, options);
            }
        }
        public void SetLayer(int layer)
        {
            Drawer.Unregister(Draw);
            Drawer.Register(Draw, MatrixDepend, layer);
        }
        
        public override void ForceDestroy()
        {
            base.ForceDestroy();
            Drawer.Unregister(Draw);
        }
        public override string ToString() => Text;
    }
}
