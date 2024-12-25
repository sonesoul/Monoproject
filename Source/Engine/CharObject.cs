using Engine.Drawing;
using Engine.Modules;
using Engine.Types.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace Engine
{
    [DebuggerDisplay("{ToString(),nq}")]
    public class CharObject : ModularObject, IRenderable
    {
        public char Character { get; set; }
        public Vector2 Origin { get; set; } = Vector2.Zero;
        public Color Color { get; set; } = Palette.White;
        public SpriteFont Font { get; set; }

        public bool IsVisible { get; set; } = true;

        public CharObject(char character, SpriteFont font, bool matrixDepend)
        {
            Drawer.Register(Draw, matrixDepend: matrixDepend);
            Font = font;

            Character = character;
            Origin = Font.MeasureString(character.ToString()) / 2;
        }
        public CharObject(char character, SpriteFont font, bool matrixDepend, params ObjectModule[] modules) : this(character, font, matrixDepend)
        {
            foreach (var module in modules)
                AddModule(module);
        }

        public void Draw(DrawContext context)
        {
            if (IsVisible)
            {
                context.String(
                    Font,
                    Character.ToString(),
                    Position,
                    Color,
                    RotationRad,
                    Origin,
                    Scale);
            }
        }

        public override void ForceDestroy()
        {
            base.ForceDestroy();
            Drawer.Unregister(Draw);
        }
        public override string ToString() => Character.ToString();
    }
}
