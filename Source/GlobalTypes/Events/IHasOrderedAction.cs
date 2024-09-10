namespace GlobalTypes.Events
{
    public interface IHasOrderedAction<TAction> : Interfaces.IOrderable
    {
        public TAction Action { get; set; }
    }
}