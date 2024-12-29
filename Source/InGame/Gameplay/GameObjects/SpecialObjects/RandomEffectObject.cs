using Engine;
using GlobalTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InGame.GameObjects.SpecialObjects
{
    public class RandomEffectObject : PurchasableObject
    {
        public override int Price { get; protected set; } = 1;

        protected override string Sprite => "?";

        private List<Action> effectsCreators; 
        private Action<Player> effect = null;

        private StringObject descriptionObject = null;
        private StepTask descriptionTask = null;

        public RandomEffectObject(Vector2 position) : base(position) 
        {
            effectsCreators = new()
            {
                () =>
                {
                    Price = 5;
                    effect = RandomCode;
                },
                () =>
                {
                    Price = 5;
                    effect = RollRequirement;
                },
                () =>
                {
                    Price = 10;
                    effect = AddSeconds;
                }
            };

            RollEffect();
        }

        public override void ApplyEffect(Player player)
        {
            effect(player);
            RollEffect();
        }

        private IEnumerator ShowEffect(string description)
        {
            descriptionObject?.Destroy();
            descriptionObject = new(description, Fonts.Silk, true)
            {
                Position = this.Position + new Vector2(0, -40),
                DrawColor = new(Palette.White, 0)
            };

            Vector2 newPosition = descriptionObject.Position + new Vector2(0, 0);
            descriptionObject.Origin = Fonts.Silk.MeasureString(description) / 2;
            descriptionObject.Scale = new(0.8f);

            yield return StepTask.Interpolate((ref float e) =>
            {
                descriptionObject.DrawColor = Color.Lerp(descriptionObject.DrawColor, Palette.White, e);
                e += FrameState.DeltaTime / 0.5f;
            });

            yield return StepTask.Delay(1);
            yield return StepTask.Interpolate((ref float e) =>
            {
                descriptionObject.DrawColor = Color.Lerp(descriptionObject.DrawColor, new(Palette.White, 0), e);

                e += FrameState.DeltaTime / 2f;
            });
        }

        private void RollEffect() => effectsCreators.RandomElement()();
        private void RandomCode(Player p)
        {
            Code c = Code.NewRandom();
            p.Codes.Push(c);
            StepTask.Replace(ref descriptionTask, ShowEffect($"{c}"));
        }
        private void RollRequirement(Player p)
        {
            Level.GetObject<CodeStorage>()?.RollRequirement();

            StepTask.Replace(ref descriptionTask, ShowEffect($"[{Level.GetObject<CodeStorage>()?.Requirement}]"));
        }
        private void AddSeconds(Player p)
        {
            Level.GetObject<CodeStorage>()?.Timer.AddSeconds(10);

            StepTask.Replace(ref descriptionTask, ShowEffect($"+{10}s"));
        }
    }
}