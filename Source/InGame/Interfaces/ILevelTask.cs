using InGame.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InGame.Interfaces
{
    public interface ILevelTask
    {
        public int CycleCount { get; }

        public event Action OnCycleComplete;

        public void Start();
        public void Finish();
    }
}