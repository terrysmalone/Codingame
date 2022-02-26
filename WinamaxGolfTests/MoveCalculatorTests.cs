using NUnit.Framework;

namespace WinamaxGolfTests
{
    public class Tests
    {
        [Test]
        public void Test1()
        {

            var course = new Course(3, 3);

            course.AddContent(2, 0, CourseContent.Water);
            course.AddContent(2, 1, CourseContent.Hole);
            course.AddContent(1, 2, CourseContent.Hole);

            course.AddBall(0, 0, 2);
            course.AddBall(2, 2, 1);



            var moves = new MoveCalculator().CalculateMoves(course);
        }

        [Test]
        public void Test2()
        {

            var course = new Course(3, 3);

            course.AddContent(2, 0, CourseContent.Water);
            course.AddContent(2, 1, CourseContent.Hole);

            course.AddBall(0, 0, 2);

            var moves = new MoveCalculator().CalculateMoves(course);
        }
    }
}
