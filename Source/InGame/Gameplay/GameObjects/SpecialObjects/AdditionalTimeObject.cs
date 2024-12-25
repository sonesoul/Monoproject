namespace InGame.GameObjects.SpecialObjects
{
    public class AdditionalTimeObject : PurchasableObject
    {
        public override int Price { get; protected set; } = 10;
        protected override string Sprite => "T+";

        public AdditionalTimeObject(Vector2 position) : base(position) { }

        public override void ApplyEffect(Player player)
        {
            Level.GetObject<CodeStorage>()?.Timer.AddSeconds(15); 
        }
    }
}