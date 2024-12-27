using System.Diagnostics;
using System.Drawing;
using WinterChallenge2024;

namespace WinterChallenge2024Tests;

[TestFixture]
public class AStarTests
{
    [Test]
    public void TestCutOff()
    {
        Game game = new Game(10, 3);
        game.UpdateHasProteins();
        game.UpdateIsBlocked();


        AStar aStar = new AStar(game);
        List<Point> shortestPath = aStar.GetShortestPath(new Point(1, 1), new Point(8, 1), 4);

        Assert.That(shortestPath.Count, Is.EqualTo(0));
    }

    public static object[] SimpleSearches =
    {
        new object[] { new Point(1, 2), new Point(1, 7), 5 },
        new object[] { new Point(9, 6), new Point(2, 6), 7 },
        new object[] { new Point(2, 2), new Point(3, 8), 7 }
    };

    [TestCaseSource(nameof(SimpleSearches))]
    public void TestSimpleSearchIsCorrect(Point startPoint, Point targetPoint, int expectedSteps)
    {
        Game game = new Game(10, 10);
        game.UpdateHasProteins();
        game.UpdateIsBlocked();

        AStar aStar = new AStar(game);
        List<Point> shortestPath = aStar.GetShortestPath(startPoint, targetPoint, 20);

        Assert.That(shortestPath.Count, Is.EqualTo(expectedSteps));
    }

    public static object[] BlockingWalls =
    {
        new object[] 
        { 
            new Point(1, 1), 
            new Point(8, 1), 
            new List<Point> 
            { 
                new Point(3, 0),
                new Point(3, 1),
                new Point(3, 2),
                new Point(3, 3),
            }, 
            13
        },
        new object[]
        {
            new Point(2, 1),
            new Point(4, 7),
            new List<Point>
            {
                new Point(0, 3),
                new Point(1, 3),
                new Point(2, 3),
                new Point(3, 3),
                new Point(4, 3),
                new Point(5, 3),

                new Point(0, 5),
                new Point(2, 5),
                new Point(3, 5),
                new Point(4, 5),
                new Point(5, 5),
                new Point(6, 5),
                new Point(7, 5),
                new Point(8, 5),
                new Point(9, 5),
            },
            18
        },
    };

    [Test]
    public void TestOnlyOnePossibility()
    {
        List<Point> walls = new List<Point>()
        {
            new Point(0,0),
            new Point(1,0),
            new Point(2,0),
            new Point(3,0),

            new Point(0,1),
            new Point(3,1),

            new Point(0,2),
            new Point(1,2),
            new Point(3,2),

            new Point(0,3),
            new Point(1,3),
            new Point(2,3),
            new Point(3,3),

        };

        Game game = new Game(4, 4);
        game.SetWalls(walls);
        game.UpdateHasProteins();
        game.UpdateIsBlocked();

        AStar aStar = new AStar(game);
        List<Point> shortestPath = aStar.GetShortestPath(new Point(1,1), new Point(2,2), 2);

        Assert.That(shortestPath.Count, Is.EqualTo(2));
        Assert.That(shortestPath[0], Is.EqualTo(new Point(2,1)));
        Assert.That(shortestPath[1], Is.EqualTo(new Point(2, 2)));

    }

    [TestCaseSource(nameof(BlockingWalls))]
    public void TestWallTraversal(
        Point startPoint, Point targetPoint, List<Point> walls, int expectedSteps)
    {
        Game game = new Game(10, 10);
        game.SetWalls(walls);
        game.UpdateHasProteins();
        game.UpdateIsBlocked();

        AStar aStar = new AStar(game);
        List<Point> shortestPath = aStar.GetShortestPath(startPoint, targetPoint, 20);

        Assert.That(shortestPath.Count, Is.EqualTo(expectedSteps));
    }

    public static object[] NoPath =
    {
        new object[]
        {
            new Point(1, 1),
            new Point(4, 1),
            new List<Point>
            {
                new Point(0, 0),
                new Point(1, 0),
                new Point(2, 0),

                new Point(0, 1),
                new Point(2, 1),

                new Point(0, 2),
                new Point(1, 2),
                new Point(2, 2),
            },
            4,
        },
        new object[]
        {
            new Point(1, 2),
            new Point(5, 5),
            new List<Point>
            {
                new Point(0, 0),
                new Point(1, 0),
                new Point(2, 0),
                new Point(3, 0),

                new Point(0, 1),
                new Point(0, 2),
                new Point(0, 3),
                new Point(0, 4),

                new Point(3, 1),
                new Point(3, 2),
                new Point(3, 3),
                new Point(3, 4),

                new Point(0, 3),
                new Point(1, 3),
                new Point(2, 3),
                new Point(3, 3),

                new Point(2, 3),
            },
            16,
        },
    };

    [TestCaseSource(nameof(NoPath))]
    public void TestNoPath(
        Point startPoint, Point targetPoint, List<Point> walls, int expectedDiagnosticCount)
    {
        Game game = new Game(10, 10);
        game.SetWalls(walls);
        game.UpdateHasProteins();
        game.UpdateIsBlocked();

        AStar aStar = new AStar(game);
        List<Point> shortestPath = aStar.GetShortestPath(startPoint, targetPoint, 20);

        Assert.That(shortestPath.Count, Is.EqualTo(0));

        Assert.That(aStar.GetDiagnosticCount(), Is.EqualTo(expectedDiagnosticCount));
    }

    [Test]
    public void Time()
    {
        Game game = new Game(10, 10);


        List<Point> walls = new List<Point>()
            {
                new Point(0, 3),
                new Point(1, 3),
                new Point(2, 3),
                new Point(3, 3),
                new Point(4, 3),
                new Point(5, 3),

                new Point(0, 5),
                new Point(2, 5),
                new Point(3, 5),
                new Point(4, 5),
                new Point(5, 5),
                new Point(6, 5),
                new Point(7, 5),
                new Point(8, 5),
                new Point(9, 5),
            };
        game.SetWalls(walls);
        game.UpdateHasProteins();
        game.UpdateIsBlocked();

        AStar aStar = new AStar(game);

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        for (int i = 0; i < 100; i++)
        {
            List<Point> shortestPath = aStar.GetShortestPath(new Point(2, 1), new Point(4, 7), 20);
        }

        stopwatch.Stop();

        TimeSpan elapsedTime = stopwatch.Elapsed;

        Console.WriteLine($"Elapsed Time: {elapsedTime.TotalMilliseconds} ms");

        Assert.That(elapsedTime.TotalMilliseconds, Is.EqualTo(-1));

        // 75ms
    }


}