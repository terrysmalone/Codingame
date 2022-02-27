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

        private List<Ball> _movedBalls = new List<Ball>();

        public void MoveBall(Point startPoint, Point endPoint)
        {
            //TODO: Don't add and remove balls. Just move them

            var movedBall = _balls.Single(b => b.Position.X == startPoint.X && b.Position.Y == startPoint.Y);

            _movedBalls.Add(movedBall);

            var numberOfHits = movedBall.NumberOfHits;

            _balls.Remove(new Ball(new Point(startPoint.X, startPoint.Y), movedBall.NumberOfHits));

            _balls.Add(new Ball(new Point(endPoint.X, endPoint.Y), numberOfHits-1));
        }

        public void UnMoveBall(Point startPoint, Point endPoint)
        {
            //var numberOfHits = _balls.Single(b => b.Item1.X == endPoint.X && b.Item1.Y == endPoint.Y).Item2;
            var movedBall = _movedBalls[^1];


            _balls.Remove(new Ball(new Point(endPoint.X, endPoint.Y),movedBall.NumberOfHits-1));
            //Console.Error.WriteLine($"Removing ball at {endPoint.X},{endPoint.Y}");
            //Console.Error.WriteLine($"Moving it to {movedBall.Item1.X},{movedBall.Item1.Y}");

            _balls.Add(new Ball(new Point(movedBall.Position.X, movedBall.Position.Y), movedBall.NumberOfHits));

            _movedBalls.RemoveAt(_movedBalls.Count-1);
        }
    }
}
