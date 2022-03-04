using System.Collections.Generic;
using System.Drawing;

namespace WinamaxGolf
{
    internal sealed class Ball
    {
        private Stack<Direction> _moveDirections;

        public Point Position { get; set; }

        public int NumberOfHits { get; set; }

        public Ball(Point position, int numberOfHits)
        {
            Position = position;
            NumberOfHits = numberOfHits;

            _moveDirections = new Stack<Direction>();
            _moveDirections.Push(Direction.Vertical);
        }
        public void AddDirection(Direction direction)
        {
            _moveDirections.Push(direction);
        }
        public Direction PeekMoveDirection()
        {
            return _moveDirections.Peek();
        }

        public Direction PopMoveDirection()
        {
            return _moveDirections.Pop();
        }
    }
}
