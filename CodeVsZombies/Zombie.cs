using System.Drawing;

namespace CodeVsZombies
{
    internal sealed class Zombie
    {
        public int Id { get; }
        public Point Position { get; }
        public Point NextPosition { get; }
    
        internal Zombie(int id, Point position, Point nextPosition)
        {
            Id = id;
            Position = position;
            NextPosition = nextPosition;
        }
    }
}