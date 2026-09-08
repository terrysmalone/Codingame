using System;
using System.Collections.Generic;
using System.Drawing;

namespace BackTrackKing;

internal class Region
{
    internal int Id { get; private set; }

    internal bool IsInked { get; private set; } = false;

    internal int Instability { get; private set; }

    private HashSet<Point> _cells;
    private HashSet<Point> _allTracks;
    private HashSet<Point> _myTracks;
    private HashSet<Point> _opponentTracks;
    private HashSet<Point> _jointTracks;

    public Region(int id)
    {
        Id = id;
        _cells = new HashSet<Point>();
        _allTracks = new HashSet<Point>();
        _myTracks = new HashSet<Point>();
        _opponentTracks = new HashSet<Point>();
        _jointTracks = new HashSet<Point>();
    }

    internal void AddCell(int x, int y)
    {
        _cells.Add(new Point(x, y));
    }

    internal IEnumerable<Point> GetCells()
    {
        return _cells;
    }

    internal void AddJointTrack(int x, int y)
    {
        if (IsInked)
        {
            return;
        }

        _jointTracks.Add(new Point(x, y));
        _allTracks.Add(new Point(x, y));
    }

    internal void AddMyTrack(int x, int y)
    {
        if (IsInked)
        {
            return;
        }

        _myTracks.Add(new Point(x, y));
        _allTracks.Add(new Point(x, y));
    }

    internal void AddOpponentTrack(int x, int y)
    {
        if (IsInked)
        {
            return;
        }

        _opponentTracks.Add(new Point(x, y));
        _allTracks.Add(new Point(x, y));
    }

    internal void UpdateInstability(int instability, bool inked)
    {
        Instability = instability;
        IsInked = inked;
    }

    internal int GetEnemyTracks()
    {
        return _opponentTracks.Count;
    }

    internal int GetMyTracks()
    {
        return _myTracks.Count;
    }
}