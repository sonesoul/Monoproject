using Engine.Drawing;
using GlobalTypes;
using InGame.GameObjects;
using InGame.Managers;
using SharpDX.XAudio2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InGame.Visuals.PlayerVisuals
{
    public static class ScoreVisual
    {

        private static Player player = null;
        [Init]
        private static void Init()
        {
            Drawer.Register(Draw);

            Player.Created += (p) =>
            {
                player = p;

                player.Destroyed += (p) =>
                {
                    if (p == player)
                    {
                        player = null;
                    }
                };
            };
        }

        private static void Draw(DrawContext context)
        {
            if (player == null)
                return;

            string scoreStr = $"{(int)SessionManager.Score.Total}";

            var font = Fonts.PicoMono;
            Vector2 scoreSize = font.MeasureString(scoreStr);

            context.String(
                font, 
                scoreStr, 
                Window.Size.Where((x, y) => new(x - 5, 5)),
                Palette.White, 
                scoreSize.TakeX(),
                Vector2.LerpPrecise(new(0), new(1.4f), player.Grade.Value).Rounded(2));
        }
    }
}