using GlobalTypes.Interfaces;
using InGame.GameObjects;
using System.Collections.Generic;

namespace InGame
{
    public class Score : IDestroyable
    {
        public bool IsDestroyed { get; set; } = false;
        public int StagesCompleted { get; private set; } = 0;

        public int BitsDeposited { get; private set; } = 0;
        public int BitsSpent { get; private set; } = 0;

        public int CodesPushed { get; private set; } = 0;
        public int CodesPopped { get; private set; } = 0;
        public int CodesPoppedManually { get; private set; } = 0;

        public int TaskCyclesCompleted { get; private set; } = 0;

        public int MistakesMade { get; private set; } = 0;

        public int MatchedPushes { get; private set; }
       
        public Grade AverageGrade { get; private set; } = new(0);

        private List<Grade> grades = new();

        public Score(Player player)
        {
            Level.Completed += OnLevelCompleted;

            player.BitWallet.Deposited += a => BitsDeposited += a;
            player.BitWallet.PaySuccessful += a => BitsSpent += a;

            player.Codes.Pushed += c => CodesPushed++;
            player.Codes.Popped += c => CodesPushed++;

            player.Codes.ManuallyPopped += c => CodesPoppedManually++;
        }

        private void OnLevelCompleted()
        {
            StagesCompleted++;
            TaskCyclesCompleted += Level.CurrentTask.CycleCount;

            var filler = Level.GetObject<StorageFiller>();
            if (filler != null)
            {
                MistakesMade += filler.MistakeCount;
            }

            var storage = Level.GetObject<CodeStorage>();
            if (storage != null)
            {
                CodesPushed += storage.TotalPushes;
                MatchedPushes = storage.MatchedPushes;
                grades.Add(storage.CompletionGrade);

                float totalGradeValue = 0;
                for (int i = 0; i < grades.Count; i++)
                {
                    totalGradeValue += grades[i].Value;
                }

                AverageGrade = new(totalGradeValue / grades.Count);
            }
        }
        
        public float GetTotal()
        {
            return
                (StagesCompleted * 100) +
                (BitsDeposited * 10) +
                
                (CodesPushed * 12) +
                (CodesPoppedManually * 3) +
                
                TaskCyclesCompleted -
                (MistakesMade * 2) +
                (MatchedPushes * 6) +
                (AverageGrade.Value * 100);  
        }

        public void Destroy() => IDestroyable.Destroy(this); 
        public void ForceDestroy()
        {
            Level.Completed -= OnLevelCompleted;
        }

        //~Score() => Monoconsole.WriteLine("Score dector");
    }
}