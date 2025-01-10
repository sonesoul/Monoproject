using GlobalTypes.Interfaces;
using InGame.GameObjects;
using System.Collections.Generic;

namespace InGame
{
    public class Score : IDestroyable
    {
        public bool IsDestroyed { get; set; } = false;

        public float Total { get; private set; }

        private List<Grade> grades = new();

        private Player player;


        public Score(Player player)
        {
            this.player = player;
            player.Grade.Obj.ValueChanged += v =>
            {
                if (v > 0)
                {
                    Total += (v * 10) + 1;
                }
            };

            Level.Completed += OnLevelCompleted;
        }

        private void OnLevelCompleted()
        {
            player.Grade.AddPoints(0.3f / (Level.TimePlayed / 15));
            grades.Add(new(Level.GetObject<Player>().Grade.Value));
        }
        public float GetTotal()
        {
            float averageGrade = 0;
            int i = 0;

            for (; i < grades.Count; i++)
            {
                averageGrade += grades[i].Value;
            }

            if (i > 0)
                averageGrade /= i;

            return Total + (averageGrade * 100);
        }
        public void Destroy() => IDestroyable.Destroy(this); 
        public void ForceDestroy()
        {
            Level.Completed -= OnLevelCompleted;
        }
    }
}