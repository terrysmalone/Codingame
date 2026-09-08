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
    private Dictionary<Point, int> _cellTracks;

    public Region(int id)
    {
        Id = id;
        _cells = new HashSet<Point>();
        _cellTracks = new Dictionary<Point, int>();
    }

    internal void AddCell(int x, int y)
    {
        _cells.Add(new Point(x, y));
    }

    internal IEnumerable<Point> GetCells()
    {
        return _cells;
    }

    internal void AddTrack(int x, int y, int tracksOwner)
    {
        if (IsInked)
        {
            return;
        }

        if (!_cellTracks.ContainsKey(new Point(x, y)))
        {
            _cellTracks.Add(new Point(x, y), tracksOwner);
        }
    }

    internal void UpdateInstability(int instability, bool inked)
    {
        Instability = instability;
        IsInked = inked;
    }
}