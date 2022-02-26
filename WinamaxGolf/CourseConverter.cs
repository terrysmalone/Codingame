using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml.Schema;

namespace WinamaxGolf
{
    internal sealed class CourseConverter
    {
        internal static Course TextToCourse(char[,] courseText)
        {
            var course = new Course(courseText.GetLength(0), courseText.GetLength(1));

            for (var y = 0; y < courseText.GetLength(1); y++)
            {
                for (var x = 0; x < courseText.GetLength(0); x++)
                {
                    var character = courseText[x,y];

                    var result = 0;

                    if(int.TryParse(character.ToString(), out result))
                    {
                        //Console.Error.WriteLine($"Adding ball to {x},{y}");
                        course.AddBall(x, y, result);
                    }
                    else
                    {
                        var courseContent = character switch
                        {
                            '.' => CourseContent.Empty,
                            'X' => CourseContent.Water,
                            'H' => CourseContent.Hole,
                            _ => throw new ArgumentOutOfRangeException()
                        };

                        //Console.Error.WriteLine($"Adding {courseContent} to {x},{y}");

                        course.AddContent(x, y, courseContent);
                    }
                }
            }

            return course;
        }

        internal static char[,] CourseToText(Course course)
        {
            var contents = course.Contents;

            var courseText = new char[contents.GetLength(0), contents.GetLength(1)];

            for (var y = 0; y < contents.GetLength(1); y++)
            {
                for (var x = 0; x < contents.GetLength(0); x++)
                {
                    var content = contents[x,y];

                    var result = 0;

                    var character = contents[x,y] switch
                    {
                        CourseContent.Empty => '.',
                        CourseContent.Water => 'X',
                        CourseContent.Hole => 'H',
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    courseText[x, y] = character;
                }
            }

            return courseText;
        }

        internal static char[,] CreateMoveBoard(int width, int height, List<(Point, Point)> verifiedMoves)
        {
            var moveChars = new char[width, height];

            for (var y = 0; y < moveChars.GetLength(1); y++)
            {
                for (var x = 0; x < moveChars.GetLength(0); x++)
                {
                    moveChars[x, y] = '.';
                }
            }

            //TODO: Add arrows for the whole move, not just the start

            foreach (var move in verifiedMoves)
            {
                char arrowDirection;

                if (move.Item2.X > move.Item1.X)
                {
                    for (var x = move.Item1.X; x < move.Item2.X; x++)
                    {
                        moveChars[x, move.Item1.Y] = '>';
                    }
                }
                else if (move.Item2.X < move.Item1.X)
                {
                    for (var x = move.Item1.X; x > move.Item2.X; x--)
                    {
                        moveChars[x, move.Item1.Y] = '<';
                    }
                }
                else if(move.Item2.Y < move.Item1.Y)
                {
                    for (var y = move.Item1.Y; y > move.Item2.Y; y--)
                    {
                        moveChars[move.Item1.X, y] = '^';
                    }

                }
                else if(move.Item2.Y > move.Item1.Y)
                {
                    for (var y = move.Item1.Y; y < move.Item2.Y; y++)
                    {
                        moveChars[move.Item1.X, y] = 'v';
                    }
                }
            }

            return moveChars;
        }

        internal static string ConvertMoveBoardToString(char[,] moveBoard)
        {
            var answer = string.Empty;

            for (var y = 0; y < moveBoard.GetLength(1); y++)
            {
                for (var x = 0; x < moveBoard.GetLength(0); x++)
                {
                    answer += moveBoard[x,y];
                }

                if (y < moveBoard.GetLength(1) - 1)
                {
                    answer += "\n";
                }
            }

            return answer;
        }
    }
}
