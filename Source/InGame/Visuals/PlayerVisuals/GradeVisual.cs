using Engine.Drawing;
using GlobalTypes;
using InGame.GameObjects;
using System.Collections;

namespace InGame.Visuals.PlayerVisuals
{
    public static class GradeVisual
    {
        private class GradeElement : VisualElement
        {
            public string rank;
            public float value;

            private StepTask scaleUpTask = null;

            public void SetScale(Vector2 end)
            {
                StepTask.Replace(ref scaleUpTask, ScaleUp(end));
                Scale = end;
            }

            private IEnumerator ScaleUp(Vector2 end)
            {
                yield return StepTask.Interpolate((ref float e) =>
                {
                    Scale = Vector2.Lerp(Scale, Vector2.One, e);
                    e += FrameState.DeltaTime;
                });
            }
        }

        private static GradeElement grade = new();
        private static Player player;
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

                player.Grade.Obj.RankChanged += r =>
                {
                    if (grade.value > player.Grade.Value)
                    {
                        grade.SetScale(new(0.5f));
                    }
                    else
                    {
                        grade.SetScale(new(1.2f));
                    }    
                };
            };
        }

        private static void Draw(DrawContext context)
        {
            return;
            if (player == null)
                return;

            grade.rank = player.Grade.Rank;
            grade.value = player.Grade.Value;

            Vector2 rankValueScale = new(MathHelper.Lerp(0.6f, 1, player.Grade.Obj.DistanceToPrevious() / Grade.RankStep));
            
            var rankFont = Fonts.SilkBold;
            context.String(
                rankFont, 
                grade.rank, 
                Window.Size.Where((x, y) => new(x - 30, 20)) + grade.Position, 
                Palette.White, 
                rankFont.MeasureString(grade.rank).TakeX(),
                rankValueScale * 2);

            DrawLine(context);
        }
        private static void DrawLine(DrawContext context)
        {
            int maxWidth = 100;
            int height = 5;

            int rectWidth = (int)MathHelper.Lerp(0, maxWidth, player.Grade.Obj.DistanceToPrevious() / Grade.RankStep);
            Rectangle rect = new(
                new(Window.Width - rectWidth, 2),
                new(rectWidth, height));

            context.Rectangle(rect, Palette.White);

            Rectangle boundRect = new(
                new(Window.Width - maxWidth, 2),
                new(maxWidth, height));

            context.HollowRect(boundRect, Palette.White);
        }
    }
}