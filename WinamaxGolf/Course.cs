using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace WinamaxGolf
{
    internal sealed class Course
    {
        internal CourseContent[,] Contents { get; }

        private List<(Point, int)> _balls = new List<(Point, int)>();

        internal Course(int x, int y)
        {
            Contents = new CourseContent[x,y];
        }

        internal void AddBall(int x, int y, int numberOfHits)
        {
            _balls.Add((new Point(x, y), numberOfHits));
        }

        internal void AddContent(int x, int y, CourseContent content)
        {
            Contents[x, y] = content;
        }

        internal List<(Point, int)> GetBalls()
        {
            return _balls.ConvertAll(ball => (ball.Item1, ball.Item2));
        }

        internal int GetNumberOfHits(int x, int y)
        {
            return _balls.Single(b => b.Item1.X == x && b.Item1.Y == y).Item2;
        }

        public void MoveBall(Point startPoint, Point endPoint)
        {
            var numberOfHits = _balls.Single(b => b.Item1.X == startPoint.X && b.Item1.Y == startPoint.Y).Item2;

            _balls.Remove((new Point(startPoint.X, startPoint.Y),numberOfHits));

            _balls.Add((new Point(endPoint.X, endPoint.Y), numberOfHits-1));


        }

        public void UnMoveBall(Point startPoint, Point endPoint)
        {
            var numberOfHits = _balls.Single(b => b.Item1.X == endPoint.X && b.Item1.Y == endPoint.Y).Item2;

            _balls.Remove((new Point(endPoint.X, endPoint.Y),numberOfHits));

            _balls.Add((new Point(startPoint.X, startPoint.Y), numberOfHits+1));
        }
    }
}
