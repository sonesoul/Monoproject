using Engine.Drawing;
using GlobalTypes;
using System;
using System.Collections;

namespace InGame.Overlays
{
    public static class ScreenFade
    {
        public static float FadeTime { get; set; } = 0.4f;
        public static Color FadeColor { get; set; } = Palette.Black;
        public static bool IsOnFade { get; private set; } = false;

        private static float alpha = 0;

        private static StepTask fadeTask = null;
        
        [Init]
        private static void Init()
        {
            Drawer.Register(Draw, false, 0);
        }

        private static void Draw(DrawContext context)
        {
            context.Rectangle(new(0, 0, Window.Width, Window.Height), new(FadeColor, (byte)alpha));
        }

        public static void FadeIn()
        {
            StepTask.Replace(ref fadeTask, FadeAlpha(255, FadeTime), false);
            IsOnFade = true;
        }
        public static void FadeOut()
        {
            StepTask.Replace(ref fadeTask, FadeAlpha(0, FadeTime), false);
            fadeTask.Completed += (t) => IsOnFade = false;
        }

        public static void FadeTo(Action callback)
        {
            FadeIn();
            fadeTask.Completed += (t) => 
            {
                callback();
                FadeOut();
            };
        }

        private static IEnumerator FadeAlpha(float newAlpha, float time)
        {
            yield return StepTask.Interpolate((ref float e) =>
            {
                alpha = MathHelper.Lerp(alpha, newAlpha, e /*0.2f - wth is this means*/);
                e += FrameState.DeltaTimeUnscaled / time;
            });

            alpha = newAlpha;

            yield return StepTask.DelayUnscaled(time);
        }
    }
}