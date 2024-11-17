using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InGame.GameObjects
{
    public class StrongJumpPad : JumpPad
    {
        public StrongJumpPad(Vector2 position) : base(position)
        {
            Force = new(0, -13);
            Sprite = "..^..";
        }
    }
}