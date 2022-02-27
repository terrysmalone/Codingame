using System.Drawing;

namespace WinamaxGolf
{
    internal sealed class Ball
    {
        // TODO: Add a flag for already moved

        public Point Position { get; }
        public int NumberOfHits { get; }

        public Ball(Point position, int numberOfHits)
        {
            Position = position;
            NumberOfHits = numberOfHits;
        }
    }
}
