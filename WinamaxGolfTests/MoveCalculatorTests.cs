using System;
using System.Diagnostics;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace WinamaxGolfTests
{
    public class Tests
    {
        [Test]
        public void TestCase6()
        {
            Course course = new Course(8, 8);

            course.AddContent(4, 1, CourseContent.Hole);
            course.AddContent(5, 1, CourseContent.Hole);
            course.AddContent(0, 3, CourseContent.Hole);
            course.AddContent(7, 3, CourseContent.Water);
            course.AddContent(2, 4, CourseContent.Water);
            course.AddContent(3, 4, CourseContent.Hole);
            course.AddContent(5, 4, CourseContent.Hole);
            course.AddContent(6, 4, CourseContent.Water);
            course.AddContent(7, 4, CourseContent.Water);
            course.AddContent(2, 5, CourseContent.Water);
            course.AddContent(5, 5, CourseContent.Hole);
            course.AddContent(7, 5, CourseContent.Water);
            course.AddContent(2, 6, CourseContent.Water);
            course.AddContent(3, 6, CourseContent.Hole);
            course.AddContent(0, 7, CourseContent.Hole);
            course.AddContent(2, 7, CourseContent.Water);
            course.AddContent(4, 7, CourseContent.Hole);

            course.AddBall(7,1, 2);
            course.AddBall(5,3, 2);
            course.AddBall(6,3, 2);
            course.AddBall(1,7, 2);
            course.AddBall(1,4, 3);
            course.AddBall(3,5, 3);
            course.AddBall(7,7, 3);
            course.AddBall(7,0, 4);
            course.AddBall(2,2, 4);

            Stopwatch stopWatch = new Stopwatch();

            stopWatch.Start();
            string moves = new MoveCalculator().CalculateMoves(course);
            stopWatch.Stop();

            TimeSpan timeSpan = stopWatch.Elapsed;
        }
    }
}
