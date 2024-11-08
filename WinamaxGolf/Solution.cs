using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("WinamaxGolfTests")]
namespace WinamaxGolf
{
    internal sealed class Solution
    {
        static void Main(string[] args)
        {
            string[] inputs = Console.ReadLine().Split(' ');
            int width = int.Parse(inputs[0]);
            int height = int.Parse(inputs[1]);

            char[,] courseText = new char[width, height];

            for (int y = 0; y < height; y++)
            {
                string? row = Console.ReadLine();

                char[] cols = row.ToCharArray();

                for (int x = 0; x < width; x++)
                {
                    courseText[x,y] = cols[x];
                }

                //Console.Error.WriteLine(row);
            }

            //DebugDisplayer.DisplayCourseText(courseText);

            // Convert to Course
            Course course = CourseConverter.TextToCourse(courseText);

            //DebugDisplayer.DisplayCourse(course);
            //DebugDisplayer.DisplayBallLocations(course.Contents.GetLength(0), course.Contents.GetLength(1), course.GetBalls());

            MoveCalculator moveCalculator = new MoveCalculator();

            string moves = moveCalculator.CalculateMoves(course);

            string[] results = moves.Split("\n");

            foreach (string result in results)
            {
                //Console.Error.WriteLine($"result - {result}");
                Console.WriteLine(result);
            }
        }
    }
}
