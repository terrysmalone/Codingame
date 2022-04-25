using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SpringChallenge2022;

internal sealed class GuardPointGenerator
{
    private readonly Point _playerBaseLocation;

    private int _xMax;
    private int _yMax;
    public GuardPointGenerator(Point playerBaseLocation, int xMax, int yMax)
    {
        _playerBaseLocation = playerBaseLocation;
        _xMax = xMax;
        _yMax = yMax;
    }

    internal List<List<Point>> GetGuardPoints(List<Hero> playerHeroes)
    {
        var guardPoints = new List<List<Point>>();


        guardPoints.AddRange(GetDefenders(playerHeroes));

        guardPoints.AddRange(GetCollectors(playerHeroes));

        guardPoints.AddRange(GetAttackers(playerHeroes));

        // Set guard points
        if (playerHeroes.Count != guardPoints.Count)
        {
            Console.Error.WriteLine("ERROR: Player heroes count doesn't match guard point count");
        }

        // At some point we need to make sure we move heroes around to minimise travel to new spots

        return guardPoints;
    }

    private List<List<Point>> GetDefenders(List<Hero> playerHeroes)
    {
        var numberOfDefenders = playerHeroes.Count(h => h.Strategy == Strategy.Defend);

        var defendPoints = new List<List<Point>>();

        if (numberOfDefenders == 1)
        {
            if (_playerBaseLocation.X == 0)
            {
                defendPoints.Add(new List<Point> { new Point(4000, 4000) });
            }
            else
            {
                defendPoints.Add(new List<Point> { new Point(_xMax - 4000, _yMax - 4000) });
            }
        }
        else if (numberOfDefenders == 2)
        {
            if (_playerBaseLocation.X == 0)
            {
                defendPoints.Add(new List<Point>
                {
                    new Point(5700, 2500),
                    new Point(8000, 2000),
                    new Point(6500, 4500)
                });
                defendPoints.Add(new List<Point>
                {
                    new Point(2500, 5700),
                    new Point(4500, 6500),
                    new Point(2000, 8000)
                });
            }
            else
            {
                defendPoints.Add(new List<Point>
                {
                    new Point(_xMax - 5700, _yMax - 2500),
                    new Point(_xMax - 8000, _yMax - 2000),
                    new Point(_xMax - 6500, _yMax - 4500)
                });
                defendPoints.Add(new List<Point>
                {
                    new Point(_xMax - 2500, _yMax - 5700),
                    new Point(_xMax - 4500, _yMax - 6500),
                    new Point(_xMax - 2000, _yMax - 8000)
                });
            }
        }
        else if (numberOfDefenders == 3)
        {
            if (_playerBaseLocation.X == 0)
            {
                defendPoints.Add(new List<Point> { new Point(5000, 2000) });
                defendPoints.Add(new List<Point> { new Point(4000, 4000) });
                defendPoints.Add(new List<Point> { new Point(2000, 5000) });
            }
            else
            {
                defendPoints.Add(new List<Point> { new Point(_xMax - 5000, _yMax - 2000) });
                defendPoints.Add(new List<Point> { new Point(_xMax - 4000, _yMax - 4000) });
                defendPoints.Add(new List<Point> { new Point(_xMax - 2000, _yMax - 5000) });
            }
        }

        return defendPoints;
    }

    private IEnumerable<List<Point>> GetCollectors(List<Hero> playerHeroes)
    {
        var numberOfCollectors = playerHeroes.Count(h => h.Strategy == Strategy.Collect);

        var collectPoints = new List<List<Point>>();

        if (numberOfCollectors == 1)
        {
            if (_playerBaseLocation.X == 0)
            {
                collectPoints.Add(new List<Point>
                {
                    new Point(_xMax / 2, _yMax / 2),
                    new Point(10000, 6000),
                    new Point(_xMax / 2, _yMax / 2),
                    new Point(12000, 3000)
                });
            }
            else
            {
                collectPoints.Add(new List<Point>
                {
                    new Point(_xMax / 2, _yMax / 2),
                    new Point(_xMax - 10000, _yMax - 6000),
                    new Point(_xMax / 2, _yMax / 2),
                    new Point(_xMax - 12000, _yMax - 3000)
                });
            }
        }

        return collectPoints;
    }

    private List<List<Point>> GetAttackers(List<Hero> playerHeroes)
    {
        var numberOfAttackers = playerHeroes.Count(h => h.Strategy == Strategy.Attack);

        var attackPoints = new List<List<Point>>();

        if (numberOfAttackers == 1)
        {
            if (_playerBaseLocation.X == 0)
            {
                attackPoints.Add(new List<Point>
                {
                    new Point(_xMax - 3000, _yMax - 2500)
                });
            }
            else
            {
                attackPoints.Add(new List<Point>
                {
                    new Point(3000, 2500)
                });
            }
        }

        return attackPoints;
    }

}
