namespace GlobalTypes.Interfaces
{
    public interface IHasOrderedAction<TAction> : Interfaces.IOrderable
    {
        public TAction Action { get; set; }
    }
}