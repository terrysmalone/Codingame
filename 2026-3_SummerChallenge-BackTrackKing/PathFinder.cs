using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace BackTrackKing;

internal class PathFinder
{
    private readonly int _width;
    private readonly int _height;

    internal PathFinder(int width, int height)
    {
        _width = width;
        _height = height;
    }

    internal List<Point> GetShortestPath(Point startPosition, Point targetPosition, HashSet<Point>? excludePoints = null)
    {
        if (startPosition == targetPosition)
        {
            return new List<Point>();
        }

        var nodesByPos = new Dictionary<Point, Node>();
        var queue = new Queue<Node>();

        var startNode = new Node(startPosition)
        {
            G = 0,
            FirstMove = null
        };

        nodesByPos[startPosition] = startNode;
        queue.Enqueue(startNode);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (current.Position == targetPosition)
            {
                var path = new List<Point>();
                var node = current;

                while (node != null && node.Position != startPosition)
                {
                    path.Add(node.Position);
                    node = node.Parent;
                }

                path.Reverse();
                return path;
            }

            // Neighbour priority order: North, East, South, West
            var neighbours = new Point[4];
            neighbours[0] = new Point(current.Position.X, current.Position.Y - 1); // North
            neighbours[1] = new Point(current.Position.X + 1, current.Position.Y); // East
            neighbours[2] = new Point(current.Position.X, current.Position.Y + 1); // South
            neighbours[3] = new Point(current.Position.X - 1, current.Position.Y); // West

            foreach (var pt in neighbours)
            {
                // bounds check
                if (pt.X < 0 || pt.X >= _width || pt.Y < 0 || pt.Y >= _height)
                {
                    continue;
                }

                if (excludePoints != null && excludePoints.Contains(pt))
                {
                    continue;
                }

                // already visited
                if (nodesByPos.ContainsKey(pt))
                {
                    continue;
                }

                var node = new Node(pt)
                {
                    Parent = current,
                    G = current.G + 1,
                    FirstMove = current.Position == startPosition ? pt : current.FirstMove
                };

                nodesByPos[pt] = node;
                queue.Enqueue(node);
            }
        }

        // no path found
        return new List<Point>();
    }
}