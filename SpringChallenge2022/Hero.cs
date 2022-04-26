using System;
using System.Collections.Generic;
using System.Drawing;

namespace SpringChallenge2022;

internal sealed class Hero
{
    private int _currentGuardPoint = 0;
    private List<Point> _guardPoints;

    internal int Id { get; }
    internal Point Position { get; set; }

    internal int CurrentMonster { get; set; } = -1;

    internal bool IsControlled { get; set; }

    internal int ShieldLife { get; set; }

    internal Strategy Strategy { get; set;} = Strategy.Defend;

    internal  bool IsShielding { get; set; }

    internal Hero(int id, Point position, bool isControlled, int shieldLife)
    {
        Id = id;
        Position = position;
        IsControlled = isControlled;
        ShieldLife = shieldLife;

        _guardPoints = new List<Point>();
    }


    internal void SetGuardPoints(List<Point> guardPoints)
    {
        _guardPoints = new List<Point>(guardPoints);
        _currentGuardPoint = 0;
    }

    internal Point GetCurrentGuardPoint()
    {
        return new Point(_guardPoints[_currentGuardPoint].X, _guardPoints[_currentGuardPoint].Y);
    }

    internal Point GetNextGuardPoint()
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

    internal int GetNumberOfGuardPoints()
    {
        return _guardPoints.Count;
    }

    internal void ClearGuardPoints()
    {
        _guardPoints = new List<Point>();
        _currentGuardPoint = 0;
    }
}
