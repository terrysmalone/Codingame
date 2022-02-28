using System.Drawing;

namespace WinamaxGolf
{
    internal sealed class Ball
    {
        // TODO: Add a flag for already moved

        public Point Position { get; set; }

        public int NumberOfHits { get; set; }

        public Ball(Point position, int numberOfHits)
        {
            Position = position;
            NumberOfHits = numberOfHits;
        }
    }
}
