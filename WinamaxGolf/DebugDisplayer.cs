using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;

namespace WinamaxGolf
{
    internal static class DebugDisplayer
    {
        internal static void DisplayCourse(Course course)
        {
            var display = string.Empty;

            var courseContents = course.Contents;

            for (var y = 0; y < courseContents.GetLength(1); y++)
            {
                for (var x = 0; x < courseContents.GetLength(0); x++)
                {
                    var character = courseContents[x,y] switch
                    {
                        CourseContent.Empty => '.',
                        CourseContent.Water => 'X',
                        CourseContent.Hole => 'H',
                        _ => '.'
                    };

                    Console.Error.WriteLine($"{x}, {y}, {character}");

                    display += character;
                }

                display += "\n";
            }

            Console.Error.WriteLine(display);
        }

        internal static void DisplayCourseText(char[,] course)
        {
            var display = string.Empty;

            for (var y = 0; y < course.GetLength(1); y++)
            {
                for (var x = 0; x < course.GetLength(0); x++)
                {
                    display += course[x,y];
                }

                display += "\n";
            }

            Console.Error.WriteLine(display);
        }

        internal static void DisplayMoves(int width, int height, List<(Point, Point)> moves)
        {
            var board = new string[width, height];

            for (var y = 0; y < board.GetLength(1); y++)
            {
                for (var x = 0; x < board.GetLength(0); x++)
                {
                    board[x, y] = "  ";
                }
            }

            for (var i = 0; i < moves.Count; i++)
            {
                var move = moves[i];
                board[move.Item1.X, move.Item1.Y] = i + "a";
                board[move.Item2.X, move.Item2.Y] = i + "b";
            }

            var display = string.Empty;

            for (var i = 0; i < width; i++)
            {
                display += "---";
            }

            display += "\n";

            for (var y = 0; y < board.GetLength(1); y++)
            {
                display += "|";
                for (var x = 0; x < board.GetLength(0); x++)
                {
                    display += board[x,y];
                    display += "|";
                }

                display += "\n";
            }

            for (var i = 0; i < width; i++)
            {
                display += "---";
            }

            Console.Error.WriteLine(display);
        }
        public static void DisplayBallLocations(int width, int height, List<Ball> balls)
        {
        var board = new char[width, height];

            for (var y = 0; y < board.GetLength(1); y++)
            {
                for (var x = 0; x < board.GetLength(0); x++)
                {
                    board[x, y] = ' ';
                }
            }

            foreach (var ball in balls)
            {
                board[ball.Position.X, ball.Position.Y] = (char)('0' + ball.NumberOfHits);
                Console.Error.WriteLine($"{ball.Position.X},{ball.Position.Y}, {ball.NumberOfHits}");
            }

            var display = string.Empty;

            for (var i = 0; i < width; i++)
            {
                display += "---";
            }

            display += "\n";

            for (var y = 0; y < board.GetLength(1); y++)
            {
                display += "|";
                for (var x = 0; x < board.GetLength(0); x++)
                {
                    display += board[x,y];
                    display += "|";
                }

                display += "\n";
            }

            for (var i = 0; i < width; i++)
            {
                display += "---";
            }

            Console.Error.WriteLine(display);
        }

        internal static void DisplayMoveIndexes(IEnumerable<int> moveIndexes)
        {
            Console.Error.Write("MoveIndexes: ");

            foreach (var moveIndex in moveIndexes)
            {
                Console.Error.Write(moveIndex + " ");
            }

            Console.Error.WriteLine();
        }
    }
}
