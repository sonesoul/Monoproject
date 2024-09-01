using Engine;
using Engine.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace InGame.GameObjects
{
    public class WordStorage : ModularObject
    {
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

        private Random random = new();
        public char CurrentLetter { get; private set; }
        private string StringLetter => CurrentLetter.ToString();

        private Vector2 letterOrigin;
        private readonly static string Brackets = "[    ]";
        private Vector2 bracketsOrigin;

        private static SpriteBatch SpriteBatch => Drawer.SpriteBatch;
        private static IngameDrawer Drawer => IngameDrawer.Instance;
        private static SpriteFont Font => UI.Font;

        public WordStorage()
        {
            Drawer.AddDrawAction(Draw);
            RollLetter();
            bracketsOrigin = Font.MeasureString(Brackets) / 2;
        }

        public bool TryPushWord(string word)
        {
            if (word.Contains(CurrentLetter, StringComparison.OrdinalIgnoreCase))
            {
                RollLetter();
                return true;
            }
            return false;
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
                StringLetter,
                position,
                Color.White,
                0,
                letterOrigin,
                1,
                SpriteEffects.None,
                0);
        }

        private void RollLetter()
        {
            CurrentLetter = AlphabetUpper[random.Next(AlphabetUpper.Length)];
            letterOrigin = Font.MeasureString(StringLetter) / 2;
        }

        public override void Destroy()
        {
            base.Destroy();

            Drawer.RemoveDrawAction(Draw);
        }
    }
}