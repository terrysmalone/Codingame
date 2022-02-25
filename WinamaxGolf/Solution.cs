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

            DebugDisplayer.DisplayCourseText(courseText);
            
            // Convert to Course
            var course = CourseConverter.TextToCourse(courseText);

            DebugDisplayer.DisplayCourse(course);

            var moveCalculator = new MoveCalculator();

            var moves = MoveCalculator.CalculateMoves(course);


            Console.WriteLine(moves);
        }
    }
}
