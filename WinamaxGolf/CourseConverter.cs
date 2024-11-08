using System;
using System.Collections.Generic;
using System.Drawing;

namespace WinamaxGolf
{
    internal sealed class CourseConverter
    {
        internal static Course TextToCourse(char[,] courseText)
        {
            Course course = new Course(courseText.GetLength(0), courseText.GetLength(1));

            for (int y = 0; y < courseText.GetLength(1); y++)
            {
                for (int x = 0; x < courseText.GetLength(0); x++)
                {
                    char character = courseText[x,y];

                    int result = 0;

                    if(int.TryParse(character.ToString(), out result))
                    {
                        //Console.Error.WriteLine($"Adding ball to {x},{y}");
                        course.AddBall(x, y, result);
                    }
                    else
                    {
                        CourseContent courseContent = character switch
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

            course.OrderBalls();

            return course;
        }

        internal static char[,] CourseToText(Course course)
        {
            CourseContent[,] contents = course.Contents;

            char[,] courseText = new char[contents.GetLength(0), contents.GetLength(1)];

            for (int y = 0; y < contents.GetLength(1); y++)
            {
                for (int x = 0; x < contents.GetLength(0); x++)
                {
                    CourseContent content = contents[x,y];

                    int result = 0;

                    char character = contents[x,y] switch
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
            char[,] moveChars = new char[width, height];

            for (int y = 0; y < moveChars.GetLength(1); y++)
            {
                for (int x = 0; x < moveChars.GetLength(0); x++)
                {
                    moveChars[x, y] = '.';
                }
            }

            //TODO: Add arrows for the whole move, not just the start

            foreach ((Point, Point) move in verifiedMoves)
            {
                char arrowDirection;

                if (move.Item2.X > move.Item1.X)
                {
                    for (int x = move.Item1.X; x < move.Item2.X; x++)
                    {
                        moveChars[x, move.Item1.Y] = '>';
                    }
                }
                else if (move.Item2.X < move.Item1.X)
                {
                    for (int x = move.Item1.X; x > move.Item2.X; x--)
                    {
                        moveChars[x, move.Item1.Y] = '<';
                    }
                }
                else if(move.Item2.Y < move.Item1.Y)
                {
                    for (int y = move.Item1.Y; y > move.Item2.Y; y--)
                    {
                        moveChars[move.Item1.X, y] = '^';
                    }

                }
                else if(move.Item2.Y > move.Item1.Y)
                {
                    for (int y = move.Item1.Y; y < move.Item2.Y; y++)
                    {
                        moveChars[move.Item1.X, y] = 'v';
                    }
                }
            }

            return moveChars;
        }

        internal static string ConvertMoveBoardToString(char[,] moveBoard)
        {
            string answer = string.Empty;

            for (int y = 0; y < moveBoard.GetLength(1); y++)
            {
                for (int x = 0; x < moveBoard.GetLength(0); x++)
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
