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

    internal List<Point> GetShortestPath(Point startPosition, Point targetPosition)
    {
        var nodesByPos = new Dictionary<Point, Node>();
        var open = new PriorityQueue<Node, int>();

        var startNode = new Node(startPosition)
        {
            G = 0,
            H = CalculationUtil.GetManhattanDistance(startPosition, targetPosition)
        };

        startNode.F = startNode.G + startNode.H;

        nodesByPos[startPosition] = startNode;
        open.Enqueue(startNode, startNode.F);

        while (open.Count > 0)
        {
            var current = open.Dequeue();
            if (current.Closed)
                continue;

            current.Closed = true;

            if (current.Position == targetPosition)
            {
                // build path
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

            var neighbours = GetPointsToCheck(current);
            foreach (var neighbour in neighbours)
            {
                if (!nodesByPos.TryGetValue(neighbour, out var existing))
                {
                    var node = new Node(neighbour)
                    {
                        Parent = current,
                        G = current.G + 1,
                        H = CalculationUtil.GetManhattanDistance(neighbour, targetPosition)
                    };
                    node.F = node.G + node.H;
                    nodesByPos[neighbour] = node;
                    open.Enqueue(node, node.F);
                }
                else if (!existing.Closed)
                {
                    int tentativeG = current.G + 1;
                    if (tentativeG < existing.G)
                    {
                        existing.G = tentativeG;
                        existing.F = existing.G + existing.H;
                        existing.Parent = current;
                        open.Enqueue(existing, existing.F); // re-enqueue with new priority
                    }
                }
            }
        }

        // No path found
        return new List<Point>();
    }

    private List<Point> GetPointsToCheck(Node currentNode)
    {
        List<Point> pointsToCheck = new List<Point>();

        if (currentNode.Position.Y - 1 >= 0)
        {
            pointsToCheck.Add(new Point(currentNode.Position.X, currentNode.Position.Y - 1));
        }

        if (currentNode.Position.X + 1 < _width)
        {
            pointsToCheck.Add(new Point(currentNode.Position.X + 1, currentNode.Position.Y));
        }

        if (currentNode.Position.Y + 1 < _height)
        {
            pointsToCheck.Add(new Point(currentNode.Position.X, currentNode.Position.Y + 1));
        }

        if (currentNode.Position.X - 1 >= 0)
        {
            pointsToCheck.Add(new Point(currentNode.Position.X - 1, currentNode.Position.Y));
        }

        if (pointsToCheck.Count == 0)
        {
            Console.Error.WriteLine("ERROR: No points to check from current node");
        }

        return pointsToCheck;
    }
}