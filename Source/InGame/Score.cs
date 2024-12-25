using GlobalTypes.Interfaces;

namespace InGame
{
    public class Score : IDestroyable
    {
        public bool IsDestroyed { get; set; } = false;
        public int Total { get; }
        public int StagesCompleted { get; private set; } = 0;
       
        public Score()
        {
            Level.Completed += OnLevelCompleted;
        }

        public void OnLevelCompleted()
        {
            StagesCompleted++;
        }

        public void Destroy() => IDestroyable.Destroy(this); 
        public void ForceDestroy()
        {
            Level.Completed -= OnLevelCompleted;    
        }
    }
}