namespace InGame.Interfaces
{
    public interface IComboReader
    {
        public int Size { get; }
        public bool IsFilled { get; }
        public string CurrentCombo { get; }
        
        public bool Push();
        public void Append(char c);
        public void Backspace();
        public void Clear();

        public void Activate();
        public void Deactivate();
    }
}
