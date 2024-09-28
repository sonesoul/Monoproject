namespace InGame.Interfaces
{
    public interface IFillable
    {
        public int Size { get; }
        public bool IsFilled { get; }
        public string CurrentCombo { get; }

        public bool Push();
        public void Append(char c);
        public void Backspace();
        public void Clear();
    }
}
