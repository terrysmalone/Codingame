using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;

namespace WinamaxGolf
{
    internal sealed class MoveCalculator
    {
        private Stopwatch _totalTimeStopwatch = new Stopwatch();

        internal string CalculateMoves(Course course)
        {
            _totalTimeStopwatch.Start();

            var verifiedMoves = new List<(Point, Point)>();
            var possibleMoves = new List<(Point, Point)>();

            var courseContents = course.Contents;
            var moveBoard = CourseConverter.CreateMoveBoard(courseContents.GetLength(0), courseContents.GetLength(1), verifiedMoves);

            foreach (var ball in course.GetBalls())
            {
                possibleMoves.AddRange(CalculateMovesForBall(courseContents, moveBoard, ball));
            }

            //Console.Error.WriteLine($"Base calculate move. {possibleMoves.Count} possible moves found");

            foreach (var possibleMove in possibleMoves)
            {
                //Console.Error.WriteLine($"Attempting move {possibleMove.Item1.X},{possibleMove.Item1.Y} to {possibleMove.Item2.X},{possibleMove.Item2.Y}");

                // Make move
                course.MoveBall(possibleMove.Item1, possibleMove.Item2);
                verifiedMoves.Add(possibleMove);

                var works = CalculateMoves(verifiedMoves, course);

                // Unmake move
                course.UnMoveBall(possibleMove.Item1, possibleMove.Item2);

                if (works)
                {
                    // convert verified moves to output board

                    //Console.Error.WriteLine($"VerifiedMove count: {verifiedMoves.Count}");

                    _totalTimeStopwatch.Stop();

                    var timeSpan = _totalTimeStopwatch.Elapsed;
                    Console.Error.WriteLine($"Total time: {timeSpan}");

                    return CourseConverter.ConvertMoveBoardToString(CourseConverter.CreateMoveBoard(courseContents.GetLength(0), courseContents.GetLength(1), verifiedMoves));
                }
                else
                {
                    verifiedMoves.RemoveAt(verifiedMoves.Count-1);
                }
            }

            return string.Empty;
        }

        private static bool CalculateMoves(List<(Point, Point)> verifiedMoves, Course course)
        {
            // If a ball has 0 hits left and isn't in a hole don't bother
            if (AreAnyDeadBalls(course))
            {
                return false;
            }

            // If any balls are in the same grid return
            if (AreAnyBallsInSameSpot(course.GetBalls()))
            {
                return false;
            }

            if (AreAllBallsInSeparateHoles(course))
            {
                //Console.Error.WriteLine("All balls in holes. Returning true");
                return true;
            }

            var possibleMoves = new List<(Point, Point)>();

            var courseContents = course.Contents;

            var moveBoard = CourseConverter.CreateMoveBoard(courseContents.GetLength(0), courseContents.GetLength(1), verifiedMoves);

            foreach (var ball in course.GetBalls())
            {
                if (ball.NumberOfHits > 0)
                {
                    possibleMoves.AddRange(CalculateMovesForBall(courseContents, moveBoard, ball));
                }
            }

            //Console.Error.WriteLine($"Calculate move. {possibleMoves.Count} possible moves found");

            if (possibleMoves.Count == 0)
            {
                //Console.Error.WriteLine("returning false");
                return false;
            }

            //Console.Error.WriteLine("=======================================");
            //Console.Error.WriteLine("Before move");
            //DebugDisplayer.DisplayMoves(courseContents.GetLength(0), courseContents.GetLength(1), verifiedMoves);
            //DebugDisplayer.DisplayBallLocations(course.Contents.GetLength(0), course.Contents.GetLength(1), course.GetBalls());


            foreach (var possibleMove in possibleMoves)
            {
                //Console.Error.WriteLine("=======================================");
                //Console.Error.WriteLine("Before make move");
                //Console.Error.WriteLine($"Attempting move {possibleMove.Item1.X},{possibleMove.Item1.Y} to {possibleMove.Item2.X},{possibleMove.Item2.Y}");

                // make move
                course.MoveBall(possibleMove.Item1, possibleMove.Item2);

                verifiedMoves.Add(possibleMove);

                //Console.Error.WriteLine("=======================================");
                //Console.Error.WriteLine("After make move");
                //DebugDisplayer.DisplayMoves(courseContents.GetLength(0), courseContents.GetLength(1), verifiedMoves);
                //DebugDisplayer.DisplayBallLocations(course.Contents.GetLength(0), course.Contents.GetLength(1), course.GetBalls());

                var works = CalculateMoves(verifiedMoves, course);

                if (works)
                {
                    //Console.Error.WriteLine("returning true");
                    return true;
                }
                else
                {
                    verifiedMoves.RemoveAt(verifiedMoves.Count-1);

                    //Console.Error.WriteLine("=======================================");
                    //Console.Error.WriteLine("After unmake move");
                    //DebugDisplayer.DisplayMoves(courseContents.GetLength(0), courseContents.GetLength(1), verifiedMoves);
                    //DebugDisplayer.DisplayBallLocations(course.Contents.GetLength(0), course.Contents.GetLength(1), course.GetBalls());
                }

                course.UnMoveBall(possibleMove.Item1, possibleMove.Item2);
            }

            //Console.Error.WriteLine("returning false");
            return false;
        }

        private static bool AreAnyBallsInSameSpot(List<Ball> balls)
        {
            var duplicates = balls.GroupBy(b => new { b.Position.X, b.Position.Y }).Where(x => x.Skip(1).Any()).Any();

            if (duplicates)
            {
                //Console.Error.WriteLine($"Duplicates found");
                return true;
            }

            return false;
        }

        private static bool AreAnyDeadBalls(Course course)
        {
            return course.GetBalls().Any(b => b.NumberOfHits == 0 && course.Contents[b.Position.X, b.Position.Y] != CourseContent.Hole);
        }

        private static IEnumerable<(Point, Point)> CalculateMovesForBall(CourseContent[,] courseContent, char[,] moveBoard, Ball ball)
        {
            var xStart = ball.Position.X;
            var yStart = ball.Position.Y;
            var numberOfHits = ball.NumberOfHits;
            var direction = ball.PeekMoveDirection();

            var allowedMoves = new List<(Point, Point)>();

            var startPoint = new Point(xStart, yStart);

            int xPosition, yPosition;

            if (direction != Direction.Horizontal)
            {
                // check left
                xPosition = startPoint.X - numberOfHits;
                yPosition = startPoint.Y;

                if (xPosition >= 0)
                {
                    var blocked = false;

                    for (var x = startPoint.X - 1; x >= startPoint.X - numberOfHits; x--)
                    {
                        blocked = IsBlocked(moveBoard, x, yPosition);
                    }

                    if (!blocked)
                    {
                        var gridContent = courseContent[xPosition, yPosition];

                        if (gridContent == CourseContent.Hole)
                        {
                            // verify there's no other ball here

                            allowedMoves.Add((startPoint, new Point(xPosition, yPosition)));

                        }
                        else if (gridContent == CourseContent.Empty)
                        {
                            allowedMoves.Add((startPoint, new Point(xPosition, yPosition)));
                        }
                    }
                }

                // check right
                xPosition = startPoint.X + numberOfHits;
                yPosition = startPoint.Y;

                if (xPosition < moveBoard.GetLength(0))
                {
                    var blocked = false;

                    for (var x = startPoint.X + 1; x <= startPoint.X + numberOfHits; x++)
                    {
                        blocked = IsBlocked(moveBoard, x, yPosition);
                    }

                    if (!blocked)
                    {
                        var gridContent = courseContent[xPosition, yPosition];

                        if (gridContent == CourseContent.Hole)
                        {
                            // verify there's no other ball here

                            allowedMoves.Add((startPoint, new Point(xPosition, yPosition)));

                        }
                        else if (gridContent == CourseContent.Empty)
                        {
                            allowedMoves.Add((startPoint, new Point(xPosition, yPosition)));
                        }
                    }
                }
            }

            if (direction != Direction.Vertical)
            {

                // check up
                xPosition = startPoint.X;
                yPosition = startPoint.Y - numberOfHits;

                if (yPosition >= 0)
                {
                    var blocked = false;

                    for (var y = startPoint.Y - 1; y >= startPoint.Y - numberOfHits; y--)
                    {
                        blocked = IsBlocked(moveBoard, xPosition, y);
                    }

                    if (!blocked)
                    {
                        var gridContent = courseContent[xPosition, yPosition];

                        if (gridContent == CourseContent.Hole)
                        {
                            // verify there's no other ball here

                            allowedMoves.Add((startPoint, new Point(xPosition, yPosition)));

                        }
                        else if (gridContent == CourseContent.Empty)
                        {
                            allowedMoves.Add((startPoint, new Point(xPosition, yPosition)));
                        }
                    }
                }

                //check down
                xPosition = startPoint.X;
                yPosition = startPoint.Y + numberOfHits;

                if (yPosition < moveBoard.GetLength(1))
                {
                    var blocked = false;

                    for (var y = startPoint.Y + 1; y <= startPoint.Y + numberOfHits; y++)
                    {
                        blocked = IsBlocked(moveBoard, xPosition, y);
                    }

                    if (!blocked)
                    {
                        var gridContent = courseContent[xPosition, yPosition];

                        if (gridContent == CourseContent.Hole)
                        {
                            // verify there's no other ball here

                            allowedMoves.Add((startPoint, new Point(xPosition, yPosition)));

                        }
                        else if (gridContent == CourseContent.Empty)
                        {
                            allowedMoves.Add((startPoint, new Point(xPosition, yPosition)));
                        }
                    }
                }
            }

            // = allowedMoves.OrderBy(m => GetDistance(m.Item1, m.Item2)).ToList();

            //allowedMoves = allowedMoves.OrderByDescending(m => GetDistance(m.Item1, m.Item2)).ToList();

            return allowedMoves;
        }

        private static int GetDistance(Point point1, Point point2)
        {
            return Math.Abs(point1.X - point2.X) + Math.Abs(point1.Y - point2.Y);
            //return Math.Sqrt(Math.Pow(point1.X - point2.X, 2) - Math.Pow(point1.Y - point2.Y, 2));
        }
        private static bool IsBlocked(char[,] moveBoard, int x, int yPosition)
        {
            return moveBoard[x, yPosition] != '.';
        }

        private static bool AreAllBallsInSeparateHoles(Course course)
        {
            var balls = course.GetBalls();
            var courseContents = course.Contents;

            foreach (var ball in balls)
            {
                //Console.Error.WriteLine($"Checking {ball.Item1.X},{ball.Item1.Y} - courseContents[ball.Item1.X, ball.Item1.Y]");
                if (courseContents[ball.Position.X, ball.Position.Y] != CourseContent.Hole)
                {
                    //Console.Error.WriteLine($"A ball is not in a hole");
                    return false;
                }
            }

            return true;
        }
    }
}
