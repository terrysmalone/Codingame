using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace _2026_1_WinterChallenge_SnakeByte;

internal class SnakeBot
{
    internal int Id { get; }
    internal List<Point> Body { get; set; }
    
    internal bool Remove { get; set; }

    private List<Point> previousMoves = new List<Point>();

    private Dictionary<Point, int> _attemptsAtPowerSources = new Dictionary<Point, int>();
    private HashSet<Point> _checkedPowerSourcesThisTurn = new HashSet<Point>();

    private HashSet<Point> _attemptedToClimbLedge = new HashSet<Point>();

    private List<Plan> _plans = new List<Plan>();

    internal SnakeBot(int id)
    {
        Id = id;

        Body = new List<Point>();
    }

    internal void AddMove(Point move)
    {
        previousMoves.Add(move);
    }

    internal bool IsStuck()
    {
        if (previousMoves.Count < 4)
        {
            return false;
        }

        int repetitions = 0;
        Point LastMove = previousMoves.Last();

        for (int i = previousMoves.Count - 2; i >= 0; --i)
        {
            if (previousMoves[i] == previousMoves.Last())
            {
                repetitions++;
            }
            else
            {
                break;
            }
        }

        if (repetitions > 3)
        {
            return true;
        }

        return false;
    }

    internal Point GetLastMove()
    {
        if (previousMoves.Count == 0)
        {
            return new Point(-1, -1);
        }
        return previousMoves.Last();
    }

    internal void AddAttemptAtPowerSource(Point powerSource)
    {
        if (_attemptsAtPowerSources.ContainsKey(powerSource))
        {
            _attemptsAtPowerSources[powerSource]++;
        }
        else
        {
            _attemptsAtPowerSources[powerSource] = 1;
        }
    }

    internal int GetAttemptsAtPowerSource(Point powerSource)
    {
        if (_attemptsAtPowerSources.ContainsKey(powerSource))
        {
            return _attemptsAtPowerSources[powerSource];
        }
        else
        {
            return 0;
        }
    }

    internal void ClearAttemptsAtPowerSource(Point powerSource)
    {
        if (_attemptsAtPowerSources.ContainsKey(powerSource))
        {
            _attemptsAtPowerSources.Remove(powerSource);
        }
    }

    internal void AddPlan(Plan plan)
    {
        _plans.Add(plan);
    }

    internal void AddPlans(List<Plan> plans)
    {
        _plans.AddRange(plans);
    }

    internal void ClearAllPlans()
    {
        _plans.Clear();
    }

    internal void RemovePlan(Plan plan)
    {
        _plans.Remove(plan);
    }

    internal List<Plan> GetPlans()
    {
        return _plans;
    }

    internal void ClearCheckedPowerSources()
    {
        _checkedPowerSourcesThisTurn.Clear();
    }

    internal void AddCheckedPowerSource(Point powerSource)
    {
        _checkedPowerSourcesThisTurn.Add(powerSource);
    }
    internal bool HasCheckedPowerSource(Point powerSource)
    {
        return _checkedPowerSourcesThisTurn.Contains(powerSource);
    }    

    internal void AddAttemptedClimbLedge(Point ledge)
    {
        _attemptedToClimbLedge.Add(ledge);
    }

    internal bool HasAttemptedClimbLedge(Point ledge)
    {
        return _attemptedToClimbLedge.Contains(ledge);
    }
}

