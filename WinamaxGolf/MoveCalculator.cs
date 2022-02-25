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

            foreach (var ball in course.GetBalls())
            {
                possibleMoves.AddRange(CalculateMovesForBall(courseContents, ball.Item1.X, ball.Item1.Y, ball.Item2));
            }

            Console.Error.WriteLine($"Base calculate move. {possibleMoves.Count} possible moves found");

            foreach (var possibleMove in possibleMoves)
            {
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

                    Console.Error.WriteLine($"VerifiedMove count: {verifiedMoves.Count}");

                    var courseConverter = new CourseConverter();

                    return CourseConverter.CreateMoveBoard(courseContents.GetLength(0), courseContents.GetLength(1), verifiedMoves);


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
            if (AreAllBallsInSeparateHoles(course))
            {
                Console.Error.WriteLine($"All balls in holes");
                return true;
            }

            // if it fails
                // return false
            //else
            {
                var possibleMoves = new List<(Point, Point)>();

                var courseContents = course.Contents;

                foreach (var ball in course.GetBalls())
                {
                    possibleMoves.AddRange(CalculateMovesForBall(courseContents, ball.Item1.X, ball.Item1.Y, ball.Item2));
                }

                Console.Error.WriteLine($"Calculate move. {possibleMoves.Count} possible moves found");

                foreach (var possibleMove in possibleMoves)
                {
                    // make move
                    course.MoveBall(possibleMove.Item1, possibleMove.Item2);
                    verifiedMoves.Add(possibleMove);

                    var works = CalculateMoves(verifiedMoves, course);

                    course.UnMoveBall(possibleMove.Item1, possibleMove.Item2);

                    if (works)
                    {
                        return true;
                    }
                    else
                    {
                        verifiedMoves.RemoveAt(verifiedMoves.Count-1);
                    }
                }
            }

            return false;
        }

        private static IEnumerable<(Point, Point)> CalculateMovesForBall(CourseContent[,] courseContent, int xStart, int yStart, int numberOfHitsAllowed)
        {
            var allowedMoves = new List<(Point, Point)>();

            var startPoint = new Point(xStart, yStart);

            // check left

            // check right
            var xPosition = startPoint.X + 1;
            var yPosition = startPoint.Y;

            while (xPosition < courseContent.GetLength(0))
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

                xPosition++;

            }

            // check up

            //check down

            return allowedMoves;
        }

        private static bool AreAllBallsInSeparateHoles(Course course)
        {
            var balls = course.GetBalls();
            var courseContents = course.Contents;

            var duplicates = balls.GroupBy(b => new { b.Item1.X, b.Item1.Y }).Where(x => x.Skip(1).Any()).Any();

            if (duplicates)
            {
                Console.Error.WriteLine($"Duplicates found");
                return false;
            }

            foreach (var ball in balls)
            {
                Console.Error.WriteLine($"Checking {ball.Item1.X},{ball.Item1.Y} - courseContents[ball.Item1.X, ball.Item1.Y]");
                if (courseContents[ball.Item1.X, ball.Item1.Y] != CourseContent.Hole)
                {
                    Console.Error.WriteLine($"A ball is not in a hole");
                    return false;
                }
            }

            return true;
        }
    }
}
