﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SummerChallenge2025_SoakOverflow
    ;
internal sealed class AStar
{
    private readonly int[,] cover;
    private int _width, _height;

    private List<Node> _nodes = new List<Node>();
    
    internal AStar(int[,] cover)
    {
        this.cover = cover;
        _width = cover.GetLength(0);
        _height = cover.GetLength(1);
    }


    internal List<Point> GetShortestPath(Point startPoint, Point targetPoint)
    {
        return GetPath(startPoint, targetPoint, new List<List<Point>>());
    }

    internal List<Point> GetShortestPath(Point startPoint, Point targetPoint, List<List<Point>> excludePaths)
    {
        var shortestPath = GetPath(startPoint, targetPoint, excludePaths);

        if (shortestPath.Count == 0)
        {
            shortestPath = GetPath(startPoint, targetPoint, new List<List<Point>>());
        }

        return shortestPath;
    }

    private List<Point> GetPath(Point startPoint, Point targetPoint, List<List<Point>> excludePaths)
    {
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

            Point[] pointsToCheck = new Point[4];

            // Prioritise heading towards the target as the first move
            if (Math.Abs(startPoint.X - targetPoint.X) >=  Math.Abs(startPoint.Y - targetPoint.Y))
            {
                pointsToCheck[0] = new Point(Math.Min(_width - 1, currentNode.Position.X + 1), currentNode.Position.Y);
                pointsToCheck[1] = new Point(Math.Max(0, currentNode.Position.X - 1), currentNode.Position.Y);
                pointsToCheck[2] = new Point(currentNode.Position.X, Math.Min(_height - 1, currentNode.Position.Y + 1));
                pointsToCheck[3] = new Point(currentNode.Position.X, Math.Max(0, currentNode.Position.Y - 1));
            }
            else
            {
                pointsToCheck[0] = new Point(currentNode.Position.X, Math.Min(_height - 1, currentNode.Position.Y + 1));
                pointsToCheck[1] = new Point(currentNode.Position.X, Math.Max(0, currentNode.Position.Y - 1));
                pointsToCheck[2] = new Point(Math.Min(_width - 1, currentNode.Position.X + 1), currentNode.Position.Y);
                pointsToCheck[3] = new Point(Math.Max(0, currentNode.Position.X - 1), currentNode.Position.Y);
            }

            foreach (Point pointToCheck in pointsToCheck)
            {
                Node? existingNode = _nodes.SingleOrDefault(n => n.Position == pointToCheck);

                if (existingNode == null)
                {
                    if (pointToCheck == startPoint 
                        || pointToCheck == targetPoint 
                        || (cover[pointToCheck.X, pointToCheck.Y] == 0 
                            && !IsExcluded(pointToCheck, excludePaths)))
                    {
                        Node node = new Node(pointToCheck);

                        node.Parent = currentNode.Position;

                        node.G = currentNode.G + 1;

                        node.H = CalculationUtil.GetManhattanDistance(pointToCheck, targetPoint);
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

    private bool IsExcluded(Point pointToCheck, List<List<Point>> excludePaths)
    {
        for (var i=0; i < excludePaths.Count; i++)
        {
            for (var j = 0; j < excludePaths[i].Count; j++)
            {
                if (excludePaths[i][j] == pointToCheck)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
