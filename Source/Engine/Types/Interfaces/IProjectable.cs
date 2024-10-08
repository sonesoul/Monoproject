using Microsoft.Xna.Framework;

namespace Engine.Types.Interfaces
{
    public interface IProjectable
    {
        public Projection ProjectOn(Vector2 axis);
    }
}