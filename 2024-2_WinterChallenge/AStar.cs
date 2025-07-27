using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace WinterChallenge2024;
internal sealed class AStar
{
    private int _diagnosticCount = 0;

    private readonly Game _game;

    private List<Node> _nodes = new List<Node>();

    internal AStar(Game game)
    {
        _game = game;
    }

    internal List<Point> GetShortestPath(Point startPoint, Point targetPoint, int maxDistance)
    {
        return GetShortestPath(startPoint, targetPoint, maxDistance, GrowStrategy.NO_PROTEINS, false);
    }

    internal List<Point> GetShortestPath(Point startPoint, Point targetPoint, int maxDistance, GrowStrategy growStrategy, bool walkOnOpponentTentaclePath)
    {
        _diagnosticCount = 0;
        _nodes = new List<Node>();

        Node currentNode = new Node(startPoint);

        _nodes.Add(currentNode);

        bool targetFound = false;

        int timeToSearch = 0;
        while (!targetFound)
        {
            if (_nodes.Count(n => n.Closed == false) == 0)
            {
                return new List<Point>();
            }

            Point[] pointsToCheck =
            [
                new Point(currentNode.Position.X, currentNode.Position.Y + 1),
                new Point(currentNode.Position.X + 1, currentNode.Position.Y),
                new Point(currentNode.Position.X, currentNode.Position.Y - 1),
                new Point(currentNode.Position.X - 1, currentNode.Position.Y),
            ];

            foreach (Point pointToCheck in pointsToCheck)
            {
                _diagnosticCount++;
                Node? existingNode = _nodes.SingleOrDefault(n => n.Position == pointToCheck);
 
                if (existingNode == null)
                {
                    if (pointToCheck == startPoint || pointToCheck == targetPoint || MapChecker.CanGrowOn(pointToCheck, _game, growStrategy, walkOnOpponentTentaclePath))
                    {                        
                        Node node = new Node(pointToCheck);

                        node.Parent = currentNode.Position;

                        node.G = currentNode.G + 1;

                        if (node.G > maxDistance)
                            continue;

                        node.H = MapChecker.CalculateManhattanDistance(pointToCheck, targetPoint);
                        node.F = node.G + node.H;

                        _nodes.Add(node);
                    }
                }
                else
                {
                    if (!existingNode.Closed)
                    {
                        int g = currentNode.G + 1;

                        if (g < existingNode.G)
                        {
                            existingNode.G = g;

                            if (existingNode.G > maxDistance)
                                continue;

                            existingNode.F = existingNode.G + existingNode.H;

                            existingNode.Parent = currentNode.Position;
                        }
                    }
                }
            }

            currentNode.Closed = true;

            if (currentNode.Position == targetPoint)
            {
                targetFound = true;
            }
            else
            {
                _nodes = _nodes.OrderBy(n => n.Closed == true).ThenBy(n => n.F).ToList();

                currentNode = _nodes.First();
            }

            timeToSearch++;

            if (timeToSearch > 1000)
            {
                Console.Error.WriteLine("Warning: Time to search hit 1000");
            }
        }

        int numberOfSteps = currentNode.G;

        List<Point> shortestPath = [currentNode.Position];

        bool atStart = false;

        while (!atStart)
        {
            currentNode = _nodes.Single(n => n.Position == currentNode.Parent);

            if (currentNode.Position == startPoint)
            {
                atStart = true;
            }
            else
            {
                shortestPath.Insert(0, currentNode.Position);
            }
        }

        return shortestPath;
    }

    internal int GetDiagnosticCount()
    {
        return _diagnosticCount;
    }
}
