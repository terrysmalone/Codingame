using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.ConstrainedExecution;
using System.Xml.Linq;

namespace SpringChallenge2026;

internal sealed class PathFinder
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
        var nodes = new List<Node>();
        var currentNode = new Node(startPosition);
        currentNode.H = CalculationUtil.GetManhattanDistance(startPosition, targetPosition);
        nodes.Add(currentNode);

        bool targetFound = false;

        while (targetFound == false && nodes.Where(n => n.Closed == false).Any())
        {
            List<Point> pointsToCheck = GetPointsToCheck(currentNode);

            foreach (Point pointToCheck in pointsToCheck)
            {
                // If pointToCheck is the same as the current point skip it
                if (pointToCheck == currentNode.Position)
                {
                    continue;
                }

                Node? existingNode = nodes.SingleOrDefault(n => n.Position == pointToCheck);

                if (existingNode == null)
                {
                    Node node = new Node(pointToCheck);

                    node.Parent = currentNode;

                    node.G = currentNode.G + 1;

                    node.H = CalculationUtil.GetManhattanDistance(pointToCheck, targetPosition);
                    node.F = node.G + node.H;

                    nodes.Add(node);
                    
                }
                else
                {
                    if (!existingNode.Closed)
                    {
                        int g = currentNode.G + 1;

                        if (g < existingNode.G)
                        {
                            existingNode.G = g;

                            existingNode.F = existingNode.G + existingNode.H;

                            existingNode.Parent = currentNode;
                        }
                    }
                }
            }

            currentNode.Closed = true;

            if (currentNode.Position == targetPosition)
            {
                targetFound = true;
            }
            else
            {
                nodes = nodes.OrderBy(n => n.Closed == true).ThenBy(n => n.F).ToList();
                currentNode = nodes.First();
            }
        }

        if (targetFound)
        {
            List<Point> shortestPath = new List<Point>();
            while (currentNode != null)
            {
                if (currentNode.Position != startPosition)
                {
                    shortestPath.Add(currentNode.Position);
                }
                currentNode = currentNode.Parent;
            }
            shortestPath.Reverse();            
            return shortestPath;
        }

        // No path found
        return new List<Point>();
    }

    private List<Point> GetPointsToCheck(Node currentNode)
    {
        List<Point> pointsToCheck = new List<Point>();

        if (currentNode.Position.X + 1 < _width)
        {
            pointsToCheck.Add(new Point(currentNode.Position.X + 1, currentNode.Position.Y));
        }

        if (currentNode.Position.X - 1 >= 0)
        {
            pointsToCheck.Add(new Point(currentNode.Position.X - 1, currentNode.Position.Y));
        }

        if (currentNode.Position.Y + 1 < _height)
        {
            pointsToCheck.Add(new Point(currentNode.Position.X, currentNode.Position.Y + 1));
        }

        if (currentNode.Position.Y - 1 >= 0)
        {
            pointsToCheck.Add(new Point(currentNode.Position.X, currentNode.Position.Y - 1));
        }

        if (pointsToCheck.Count == 0)
        {
            Console.Error.WriteLine("ERROR: No points to check from current node");
        }

        return pointsToCheck;
    }
}


