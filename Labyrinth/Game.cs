using System.Drawing;

namespace Labyrinth
{
    internal sealed class Game
    {
        private int _worldWidth;
        private int _worldHeight;

        private Content[,] _worldGrid;
        private Point _characterLocation;

        private readonly int _alarmCountDown;

        private readonly Size _leftMove = new Size(-1, 0);
        private readonly Size _rightMove = new Size(1, 0);
        private readonly Size _upMove = new Size(0, -1);
        private readonly Size _downMove = new Size(0, 1);

        private readonly Size _leftLook = new Size(-3, 0);
        private readonly Size _rightLook = new Size(3, 0);
        private readonly Size _upLook = new Size(0, -3);
        private readonly Size _downLook = new Size(0, 3);

        public Game(int worldWidth, int worldHeight, int alarmCountDown)
        {
            _worldWidth = worldWidth;
            _worldHeight = worldHeight;

            _alarmCountDown = alarmCountDown;
            _worldGrid = new Content[worldWidth, worldHeight];
        }

        internal void UpdateCharacterLocation(int xPos, int yPos)
        {
            _characterLocation = new Point(xPos, yPos);
        }

        internal void UpdateWorld(Content[,] world)
        {
             _worldGrid = world.Clone() as Content[,];
        }

        public MoveDirection GetMove()
        {
             DebugViewer.PrintWorld(_worldGrid, _characterLocation);

             // if we know where the control room is
                // if we have a path back faster than the timer
                    // move back on that path
            // else

            // move to an unexplored area
            var exploreMove = GetExploreMove();

            return exploreMove;
        }
        private MoveDirection GetExploreMove()
        {
            if (ShouldMove(_characterLocation + _leftLook,
                           _characterLocation + _leftMove))
            {
                return MoveDirection.Left;
            }

            if (ShouldMove(_characterLocation + _rightLook,
                           _characterLocation + _rightMove))
            {
                return MoveDirection.Right;
            }

            if (ShouldMove(_characterLocation + _upLook,
                           _characterLocation + _upMove))
            {
                return MoveDirection.Up;
            }

            if (ShouldMove(_characterLocation + _downLook,
                           _characterLocation + _downMove))
            {
                return MoveDirection.Down;
            }

            return MoveDirection.Right;
        }

        private bool ShouldMove(Point lookPoint, Point movePoint)
        {
            // If we havent explored here
            if (lookPoint.X >= 0 && lookPoint.X <= _worldWidth-1 && lookPoint.Y >= 0 && lookPoint.Y <= _worldHeight-1
                && _worldGrid[lookPoint.X, lookPoint.Y] == Content.Unknown)
            {
                // If we can move here
                if (_worldGrid[movePoint.X, movePoint.Y] == Content.Hollow)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
