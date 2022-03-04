using System.Drawing;

namespace Labyrinth
{
    internal sealed class Game
    {
        private int _worldWidth;
        private int _worldHeight;

        private Content[,] _worldGrid;
        private bool[,] _visited;
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

        private Point _lastVisited = new Point(-1, -1);

        public Game(int worldWidth, int worldHeight, int alarmCountDown)
        {
            _worldWidth = worldWidth;
            _worldHeight = worldHeight;

            _alarmCountDown = alarmCountDown;
            _worldGrid = new Content[worldWidth, worldHeight];
            _visited = new bool[worldWidth, worldHeight];
        }

        internal void UpdateCharacterLocation(int xPos, int yPos)
        {
            Console.Error.WriteLine($"Updating character location to {xPos},{yPos}");
            _characterLocation = new Point(xPos, yPos);
            _visited[xPos, yPos] = true;
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

            // TODO: Cycles catch us out badly (Test 08 especially). If we're stuck for a move lots look further afield for a preference

            _lastVisited = new Point(_characterLocation.X, _characterLocation.Y);

            return exploreMove;
        }
        private MoveDirection GetExploreMove()
        {
            // first try to move in the direction of an unexplored area
            var unVisitedDirection = GetUnvisitedMove();

            if (unVisitedDirection != MoveDirection.None)
            {
                Console.Error.WriteLine($"Unvisited direction {unVisitedDirection} found.");

                _lastVisited = new Point(_characterLocation.X, _characterLocation.Y);
                return unVisitedDirection;
            }

            Console.Error.WriteLine($"Unvisited direction not found. Looking for other move");

            // We didn't find an unvisited spot
            // Move to any spot except the last
            var possibleDirection = MoveDirection.None;

            if (CanMove(_characterLocation + _leftMove))
            {
                possibleDirection = MoveDirection.Left;

                Console.Error.WriteLine("We can move left");
                Console.Error.WriteLine($"last visited: {_lastVisited.X}, {_lastVisited.Y}");
                Console.Error.WriteLine($"left move: {(_characterLocation + _leftMove).X}, {(_characterLocation + _leftMove).Y}");

                if (!WasLastMove(_characterLocation + _leftMove))
                {
                    return MoveDirection.Left;
                }
            }

            if (CanMove(_characterLocation + _rightMove))
            {
                possibleDirection = MoveDirection.Right;

                Console.Error.WriteLine("We can move right");
                Console.Error.WriteLine($"last visited: {_lastVisited.X}, {_lastVisited.Y}");
                Console.Error.WriteLine($"right move: {(_characterLocation + _rightMove).X}, {(_characterLocation + _rightMove).Y}");

                if (!WasLastMove(_characterLocation + _rightMove))
                {
                    return MoveDirection.Right;
                }
            }

            if (CanMove(_characterLocation + _upMove))
            {
                possibleDirection = MoveDirection.Up;

                Console.Error.WriteLine("We can move up");

                if (!WasLastMove(_characterLocation + _upMove))
                {
                    return MoveDirection.Up;
                }
            }

            if (CanMove(_characterLocation + _downMove))
            {
                Console.Error.WriteLine("We can move down");

                possibleDirection = MoveDirection.Down;

                if (!WasLastMove(_characterLocation + _downMove))
                {
                    return MoveDirection.Down;
                }
            }

            Console.Error.WriteLine($"No new move. Falling back to {possibleDirection}");

            return possibleDirection;
        }

        private MoveDirection GetUnvisitedMove()
        {
            if (Unvisited(_characterLocation + _leftMove))
            {
                Console.Error.WriteLine("WE HAVEN'T VISITED LEFT");
                return MoveDirection.Left;
            }

            if (Unvisited(_characterLocation + _rightMove))
            {
                Console.Error.WriteLine("WE HAVEN'T VISITED RIGHT");
                return MoveDirection.Right;
            }

            if (Unvisited(_characterLocation + _upMove))
            {
                Console.Error.WriteLine("WE HAVEN'T VISITED UP");
                return MoveDirection.Up;
            }

            if (Unvisited(_characterLocation + _downMove))
            {
                Console.Error.WriteLine("WE HAVEN'T VISITED DOWN");
                return MoveDirection.Down;
            }

            return MoveDirection.None;
        }

        private bool Unvisited(Point location)
        {
            // If we can go here
            if (CanMove(location))
            {
                if (!_visited[location.X, location.Y])
                {
                    return true;
                }
            }

            return false;
        }

        private bool CanMove(Point location)
        {
            // If we can go here
            if (location.X >= 0 && location.X <= _worldWidth - 1 && location.Y >= 0 && location.Y <= _worldHeight - 1
                && _worldGrid[location.X, location.Y] == Content.Hollow)
            {
                Console.Error.WriteLine($"Can move to {location.X},{location.Y}");
                return true;
            }

            return false;
        }

        private bool WasLastMove(Point movePoint)
        {
            if (movePoint.X == _lastVisited.X && movePoint.Y == _lastVisited.Y)
            {
                Console.Error.Write("This was the last move");
                return true;
            }

            return false;
        }
    }
}
