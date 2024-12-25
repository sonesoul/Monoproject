namespace InGame.GameObjects.SpecialObjects
{
    public class RandomCodeObject : PurchasableObject
    {
        public override int Price { get; protected set; } = 5;

        protected override string Sprite => "C?";

        public RandomCodeObject(Vector2 position) : base(position) { }

        public override void ApplyEffect(Player player)
        {
            player.Codes.Push(Code.NewRandom());
        }
    }
}