using Engine;
using Engine.Drawing;
using InGame.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace InGame.GameObjects
{
    public class ComboStorage : ModularObject, ITaggable
    {
        public string Tag => "storage";

        public static readonly char[] AlphabetUpper =
        {
            'A', 'B', 'C', 
            'D', 'E', 'F',
            'G', 'H', 'I',
            'J', 'K', 'L',
            'M', 'N', 'O',
            'P', 'Q', 'R',
            'S', 'T', 'U', 
            'V', 'W', 'X',
            'Y', 'Z'
        };

        public readonly char[] Pattern = { 'Q', 'W', 'E', 'R' };

        private readonly Random random = new();
        public char Requirement { get; private set; }
        private string StringReq => Requirement.ToString();

        private Vector2 letterOrigin;
        private readonly static string Brackets = "[    ]";
        private Vector2 bracketsOrigin;

        private static SpriteBatch SpriteBatch => Drawer.SpriteBatch;
        private static IngameDrawer Drawer => IngameDrawer.Instance;
        private static SpriteFont Font => UI.Silk;

        public ComboStorage()
        {
            Drawer.AddDrawAction(Draw);
            RollRequirement();
            bracketsOrigin = Font.MeasureString(Brackets) / 2;
        }

        public bool Push(string combo)
        {
            if (combo.Contains(Requirement, StringComparison.OrdinalIgnoreCase))
            {
                RollRequirement();
                return true;
            }
            return false;
        }
        public void RollRequirement()
        {
            Requirement = Pattern[random.Next(Pattern.Length)];
            letterOrigin = Font.MeasureString(StringReq) / 2;
        }

        public void Draw(GameTime gameTime)
        {
            SpriteBatch.DrawString(
                Font, 
                Brackets, 
                position, 
                Color.White, 
                0, 
                bracketsOrigin,
                new Vector2(1f, 1.5f), 
                SpriteEffects.None,
                0);

            SpriteBatch.DrawString(
                Font,
                StringReq,
                position,
                Color.White,
                0,
                letterOrigin,
                1,
                SpriteEffects.None,
                0);
        }

        protected override void PostDestroy()
        {
            Drawer.RemoveDrawAction(Draw);
        }
    }
}