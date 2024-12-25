namespace InGame.Interfaces
{
    public interface ICodeReader
    {
        public int Size { get; }
        public bool IsFilled { get; }
        public string Sequence { get; }
        public Code? TargetCode { get; }
        
        public bool Push();
        public void Append(char c);
        public void Clear();

        public void SetCode(Code? code);

        public void Activate();
        public void Deactivate();
    }
}
