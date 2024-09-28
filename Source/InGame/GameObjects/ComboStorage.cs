using Engine;
using Engine.Drawing;
using InGame.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace InGame.GameObjects
{
    public class ComboStorage : ModularObject, ILevelObject
    {
        public string Tag => nameof(ComboStorage);
        public bool IsInitialized { get; private set; } = true;
        
        public int Size { get; private set; }

        public char Requirement { get; private set; }
        public int CombosLeft => Size - pushedCombos.Count;

        private static SpriteFont BracketsFont => UI.Silk;
        private static SpriteFont LetterFont => UI.SilkBold;

        private static IngameDrawer Drawer => IngameDrawer.Instance;
        private static SpriteBatch SpriteBatch => Drawer.SpriteBatch;

        private Vector2 letterOrigin;
        private readonly List<Combo> pushedCombos = new();

        public ComboStorage(int storageSize)
        {
            Size = storageSize.Clamp(1, 99);

            Drawer.AddDrawAction(Draw);

            RollRequirement();
        }
        public void Destruct() => Destroy();

        public bool Push(Combo combo)
        {
            if (combo.Contains(Requirement))
            {
                RollRequirement();
                pushedCombos.Add(combo);

                if (pushedCombos.Count >= Size)
                    Level.New();

                return true;
            }

            return false;
        }

        private void Draw()
        {
            //letter
            SpriteBatch.DrawString(
                LetterFont,
                Requirement.ToString(),
                Position,
                Color.White,
                0,
                letterOrigin,
                1,
                SpriteEffects.None,
                0);

            Vector2 origin = BracketsFont.MeasureString("[") / 2;

            SpriteBatch.DrawString(
                BracketsFont,
                "[",
                Position.WhereX(x => x - 15),
                Color.White,
                0,
                origin,
                new Vector2(1.4f, 2),
                SpriteEffects.None,
                0);

            origin = BracketsFont.MeasureString("]") / 2;

            SpriteBatch.DrawString(
                BracketsFont,
                "]",
                Position.WhereX(x => x + 14),
                Color.White,
                0,
                origin,
                new Vector2(1.4f, 2),
                SpriteEffects.None,
                0);
        }
        private void RollRequirement()
        {
            Requirement = Level.KeyPattern.RandomElement();
            letterOrigin = LetterFont.MeasureString(Requirement.ToString()) / 2;
        }

        protected override void PostDestroy()
        {
            Drawer.RemoveDrawAction(Draw);
        }
    }
}