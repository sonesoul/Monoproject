namespace InGame.GameObjects.SpecialObjects
{
    public class RequirementRollObject : PurchasableObject
    {
        public override int Price { get; protected set; } = 5;
        protected override string Sprite => "[?]";

        public RequirementRollObject(Vector2 position) : base(position) { }

        public override void ApplyEffect(Player player)
        {
            Level.GetObject<CodeStorage>()?.RollRequirement();
        }
    }
}