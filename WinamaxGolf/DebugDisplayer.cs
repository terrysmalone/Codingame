using System;
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
    }
}
