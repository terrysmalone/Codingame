using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;

namespace SpringChallenge2022;

internal sealed class Hero
{
    private int _currentGuardPoint = 0;
    private List<Point> _guardPoints;

    public int Id { get; }
    public Point Position { get; set; }

    internal int CurrentMonster { get; set; } = -1;

    internal string CurrentAction { get; set; } = "WAIT";

    internal bool UsingSpell {get; set; } = false;

    internal bool IsControlled { get; set; } = false;

    internal int ShieldLife { get; set; }

    internal Strategy Strategy { get; set;} = Strategy.Defend;
    internal  bool IsShielding { get; set; }

    public Hero(int id, Point position, bool isControlled, int shieldLife)
    {
        Id = id;
        Position = position;
        IsControlled = isControlled;
        ShieldLife = shieldLife;

        _guardPoints = new List<Point>();
    }


    public void SetGuardPoints(List<Point> guardPoints)
    {
        Console.Error.WriteLine($"guardPoints.Count: {guardPoints.Count}");
        _guardPoints = new List<Point>(guardPoints);
    }

    public Point GetCurrentGuardPoint()
    {
        return new Point(_guardPoints[_currentGuardPoint].X, _guardPoints[_currentGuardPoint].Y);
    }

    public Point GetNextGuardPoint()
    {
        if (_currentGuardPoint >= _guardPoints.Count - 1)
        {
            _currentGuardPoint = 0;
        }
        else
        {
            _currentGuardPoint++;
        }

        return new Point(_guardPoints[_currentGuardPoint].X, _guardPoints[_currentGuardPoint].Y);
    }

    public int GetNumberOfGuardPoints()
    {
        return _guardPoints.Count;
    }
}
