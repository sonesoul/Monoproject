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
        public int Size { get; private set; }

        public char Requirement { get; private set; }
        public int CombosLeft => Size - pushedCombos.Count;

        private static SpriteFont BracketsFont => UI.Silk;
        private static SpriteFont LetterFont => UI.SilkBold;

        private Vector2 letterOrigin;
        private readonly List<Combo> pushedCombos = new();

        public ComboStorage(int storageSize)
        {
            Size = storageSize.Clamp(1, 99);

            Drawer.Register(Draw);

            RollRequirement();
        }
        public void OnRemove() => Destroy();

        public bool Push(Combo combo)
        {
            if (combo.Contains(Requirement))
            {
                RollRequirement();
                pushedCombos.Add(combo);

                if (pushedCombos.Count >= Size)
                    Level.Load();

                return true;
            }

            return false;
        }

        private void Draw(DrawContext context)
        {
            //letter
            context.String(
                LetterFont,
                Requirement.ToString(),
                Position,
                Color.White,
                0,
                letterOrigin,
                1);

            Vector2 origin = BracketsFont.MeasureString("[") / 2;
            
            context.String(
                BracketsFont,
                "[",
                Position.WhereX(x => x - 15),
                Color.White,
                0,
                origin,
                new Vector2(1.4f, 2));

            origin = BracketsFont.MeasureString("]") / 2;

            context.String(
                BracketsFont,
                "]",
                Position.WhereX(x => x + 14),
                Color.White,
                0,
                origin,
                new Vector2(1.4f, 2));
        }
        private void RollRequirement()
        {
            Requirement = Level.KeyPattern.RandomElement();
            letterOrigin = LetterFont.MeasureString(Requirement.ToString()) / 2;
        }

        protected override void PostDestroy()
        {
            Drawer.Unregister(Draw);
        }
    }
}