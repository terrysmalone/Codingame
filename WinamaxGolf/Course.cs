using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace WinamaxGolf
{
    internal sealed class Course
    {
        internal CourseContent[,] Contents { get; }

        private List<Ball> _balls = new List<Ball>();

        internal Course(int x, int y)
        {
            Contents = new CourseContent[x,y];
        }

        internal void AddBall(int x, int y, int numberOfHits)
        {
            _balls.Add(new Ball(new Point(x, y), numberOfHits));
        }

        internal void AddContent(int x, int y, CourseContent content)
        {
            Contents[x, y] = content;
        }

        internal List<Ball> GetBalls()
        {
            return _balls.ConvertAll(b => (new Ball(b.Position, b.NumberOfHits)));
        }

        internal int GetNumberOfHits(int x, int y)
        {
            return _balls.Single(b => b.Position.X == x && b.Position.Y == y).NumberOfHits;
        }

        private List<int> _movedIndexes = new List<int>();

        public void MoveBall(Point startPoint, Point endPoint)
        {
            //DebugDisplayer.DisplayBallLocations(Contents.GetLength(0), Contents.GetLength(1), _balls);

            //Console.Error.WriteLine($"Moving ball from {startPoint.X},{startPoint.Y} to {endPoint.X},{endPoint.Y}");

            _movedIndexes.Add(_balls.IndexOf(_balls.Single(b => b.Position.X == startPoint.X && b.Position.Y == startPoint.Y)));

            //DebugDisplayer.DisplayMoveIndexes(_movedIndexes);

            var movedBall = _balls[_movedIndexes[^1]];

            movedBall.Position = new Point(endPoint.X, endPoint.Y);
            movedBall.NumberOfHits--;

            if (startPoint.X - endPoint.X != 0)
            {
                movedBall.AddDirection(Direction.Horizontal);
                //movedBall.MoveDirections.Push(Direction.Horizontal);
                //Console.Error.WriteLine("Direction.Horizontal");
            }
            else
            {
                //movedBall.MoveDirections.Push(Direction.Vertical);
                movedBall.AddDirection(Direction.Vertical);
                //Console.Error.WriteLine("Direction.Vertical");
            }

            //DebugDisplayer.DisplayBallLocations(Contents.GetLength(0), Contents.GetLength(1), _balls);
        }

        public void UnMoveBall(Point startPoint, Point endPoint)
        {
            //DebugDisplayer.DisplayBallLocations(Contents.GetLength(0), Contents.GetLength(1), _balls);

            var lastIndex = _movedIndexes[^1];
            var movedBall = _balls[lastIndex];

            movedBall.Position = new Point(startPoint.X, startPoint.Y);
            movedBall.NumberOfHits++;

            _movedIndexes.RemoveAt(_movedIndexes.Count-1);

            movedBall.PopMoveDirection();

            //DebugDisplayer.DisplayBallLocations(Contents.GetLength(0), Contents.GetLength(1), _balls);
        }
        public void OrderBalls()
        {
            _balls = _balls.OrderBy(b => b.NumberOfHits).ToList();
        }
    }
}
