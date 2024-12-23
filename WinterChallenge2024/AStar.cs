using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WinterChallenge2024;
internal sealed partial class AStar
{
    private readonly Game _game;

    private List<Node> _nodes = new List<Node>();

    internal AStar(Game game)
    {
        _game = game;
    }

    internal List<Point> GetShortestPath(Point startPoint, Point targetPoint, int maxDistance)
    {
        return GetShortestPath(startPoint, targetPoint, maxDistance, false);
    }

    internal List<Point> GetShortestPath(Point startPoint, Point targetPoint, int maxDistance, bool canGrowOnProteins)
    {
        _nodes = new List<Node>();

        // Create a node for the start Point
        Node currentNode = new Node(startPoint);
        _nodes.Add(currentNode);

        bool targetFound = false;

        int timeToSearch = 0;
        while (!targetFound)
        {
            Point[] pointsToCheck = new Point[4];

            pointsToCheck[0] = new Point(currentNode.Position.X, currentNode.Position.Y + 1);
            pointsToCheck[1] = new Point(currentNode.Position.X + 1, currentNode.Position.Y);
            pointsToCheck[2] = new Point(currentNode.Position.X, currentNode.Position.Y - 1);
            pointsToCheck[3] = new Point(currentNode.Position.X - 1, currentNode.Position.Y);

            // for each adjacent square
            foreach (Point pointToCheck in pointsToCheck)
            {
                Node? existingNode = _nodes.SingleOrDefault(n => n.Position == pointToCheck);

                // If a node doesnt exists  
                if (existingNode == null)
                {
                    // Create a node if the position is walkable
                    if (MapChecker.CanGrowOn(pointToCheck, canGrowOnProteins, _game))
                    {                        
                        Node node = new Node(pointToCheck);

                        node.Parent = currentNode.Position;

                        node.G = currentNode.G + 1;

                        if (node.G > maxDistance)
                            continue;

                        node.H = (Math.Abs(targetPoint.X - pointToCheck.X) + Math.Abs(targetPoint.Y - pointToCheck.Y));
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

            if (_nodes.Count(n => n.Closed == false) == 0)
            {
                return new List<Point>();
            }

            currentNode.Closed = true;

            if (currentNode.Position == targetPoint)
            {
                targetFound = true;
            }
            else
            {
                // Sort nodes
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

        List<Point> shortestPath = new List<Point>();
        shortestPath.Add(currentNode.Position);

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
}
