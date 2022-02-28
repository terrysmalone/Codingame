using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("WinamaxGolfTests")]
namespace WinamaxGolf
{
    internal sealed class Solution
    {
        static void Main(string[] args)
        {
            var inputs = Console.ReadLine().Split(' ');
            var width = int.Parse(inputs[0]);
            var height = int.Parse(inputs[1]);

            var courseText = new char[width, height];

            for (var y = 0; y < height; y++)
            {
                var row = Console.ReadLine();

                var cols = row.ToCharArray();

                for (var x = 0; x < width; x++)
                {
                    courseText[x,y] = cols[x];
                }

                //Console.Error.WriteLine(row);
            }

            //DebugDisplayer.DisplayCourseText(courseText);
            
            // Convert to Course
            var course = CourseConverter.TextToCourse(courseText);

            //DebugDisplayer.DisplayCourse(course);
            //DebugDisplayer.DisplayBallLocations(course.Contents.GetLength(0), course.Contents.GetLength(1), course.GetBalls());

            var moveCalculator = new MoveCalculator();

            var moves = moveCalculator.CalculateMoves(course);

            var results = moves.Split("\n");

            foreach (var result in results)
            {
                //Console.Error.WriteLine($"result - {result}");
                Console.WriteLine(result);
            }
        }
    }
}
