using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace WinamaxGolf
{
    internal sealed class MoveCalculator
    {
        internal static string CalculateMoves(Course course)
        {
            var verifiedMoves = new List<(Point, Point)>();
            var possibleMoves = new List<(Point, Point)>();

            var courseContents = course.Contents;
            var moveBoard = CourseConverter.CreateMoveBoard(courseContents.GetLength(0), courseContents.GetLength(1), verifiedMoves);

            foreach (var ball in course.GetBalls())
            {
                possibleMoves.AddRange(CalculateMovesForBall(courseContents, moveBoard, ball.Item1.X, ball.Item1.Y, ball.Item2));
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
                //course.UnMoveBall(possibleMove.Item1, possibleMove.Item2);
                course.UnMoveBall(possibleMove.Item1, possibleMove.Item2);

                if (works)
                {
                    // convert verified moves to output board

                    //Console.Error.WriteLine($"VerifiedMove count: {verifiedMoves.Count}");

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
                if (ball.Item2 > 0)
                {
                    possibleMoves.AddRange(CalculateMovesForBall(courseContents, moveBoard, ball.Item1.X, ball.Item1.Y, ball.Item2));
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
            //DebugDisplayer.DisplayBallLocations(course.GetBalls());


            foreach (var possibleMove in possibleMoves)
            {
                //Console.Error.WriteLine($"Attempting move {possibleMove.Item1.X},{possibleMove.Item1.Y} to {possibleMove.Item2.X},{possibleMove.Item2.Y}");

                // make move
                course.MoveBall(possibleMove.Item1, possibleMove.Item2);

                verifiedMoves.Add(possibleMove);

                //Console.Error.WriteLine("=======================================");
                //Console.Error.WriteLine("After make move");
                //DebugDisplayer.DisplayMoves(courseContents.GetLength(0), courseContents.GetLength(1), verifiedMoves);
                //DebugDisplayer.DisplayBallLocations(course.GetBalls());

                var works = CalculateMoves(verifiedMoves, course);

                course.UnMoveBall(possibleMove.Item1, possibleMove.Item2);

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
                    //DebugDisplayer.DisplayBallLocations(course.GetBalls());
                }
            }

            //Console.Error.WriteLine("returning false");
            return false;
        }
        private static bool AreAnyBallsInSameSpot(List<(Point, int)> balls)
        {
            var duplicates = balls.GroupBy(b => new { b.Item1.X, b.Item1.Y }).Where(x => x.Skip(1).Any()).Any();

            if (duplicates)
            {
                //Console.Error.WriteLine($"Duplicates found");
                return true;
            }

            return false;
        }

        private static IEnumerable<(Point, Point)> CalculateMovesForBall(CourseContent[,] courseContent, char[,] moveBoard, int xStart, int yStart, int shotCount)
        {
            var allowedMoves = new List<(Point, Point)>();

            var startPoint = new Point(xStart, yStart);

            // check left
            var xPosition = startPoint.X - shotCount;
            var yPosition = startPoint.Y;

            if (xPosition >= 0)
            {
                var blocked = false;

                for (var x = startPoint.X - 1; x >= startPoint.X - shotCount; x--)
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
            xPosition = startPoint.X + shotCount;
            yPosition = startPoint.Y;

            if (xPosition < moveBoard.GetLength(0))
            {
                var blocked = false;

                for (var x = startPoint.X + 1; x <= startPoint.X + shotCount; x++)
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

            // check up
            xPosition = startPoint.X;
            yPosition = startPoint.Y - shotCount;

            if (yPosition >= 0)
            {
                var blocked = false;

                for (var y = startPoint.Y - 1; y >= startPoint.Y - shotCount; y--)
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
            yPosition = startPoint.Y + shotCount;

            if (yPosition < moveBoard.GetLength(1))
            {
                var blocked = false;

                for (var y = startPoint.Y + 1; y <= startPoint.Y + shotCount; y++)
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

            return allowedMoves;
        }
        private static bool IsBlocked(char[,] moveBoard, int x, int yPosition)
        {
            var blocked = moveBoard[x, yPosition] == '<' || moveBoard[x, yPosition] == '>' || moveBoard[x, yPosition] == '^' || moveBoard[x, yPosition] == 'v';

            return blocked;
        }

        private static bool AreAllBallsInSeparateHoles(Course course)
        {
            var balls = course.GetBalls();
            var courseContents = course.Contents;

            foreach (var ball in balls)
            {
                //Console.Error.WriteLine($"Checking {ball.Item1.X},{ball.Item1.Y} - courseContents[ball.Item1.X, ball.Item1.Y]");
                if (courseContents[ball.Item1.X, ball.Item1.Y] != CourseContent.Hole)
                {
                    //Console.Error.WriteLine($"A ball is not in a hole");
                    return false;
                }
            }

            return true;
        }
    }
}
