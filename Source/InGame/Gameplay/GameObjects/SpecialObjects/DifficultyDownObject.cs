using InGame.Managers;
using InGame.Pools;

namespace InGame.GameObjects.SpecialObjects
{
    public class DifficultyDownObject : PurchasableObject
    {
        public override int Price { get; protected set; } = 15;
        protected override string Sprite => "D-";

        public DifficultyDownObject(Vector2 position) : base(position) { }

        public override void ApplyEffect(Player player)
        {
            SessionManager.Difficulty.AddModifier(ModifierPool.GetRandomDown());
        }
    }
}