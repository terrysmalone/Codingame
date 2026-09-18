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
    private Map _map;

    private const int COST_SCALE = 10; // Multiply base costs by this so fractional weighting doesn't collapse to 0
    private const float SAFE_REGION_WEIGHT = 0.9f;


    internal PathFinder(int width, int height, Map map)
    {
        _width = width;
        _height = height;
        _map = map;
    }

    internal List<Point> GetShortestPath(Point startPosition, Point targetPosition, HashSet<Point>? excludePoints = null, PathCostMode costMode = PathCostMode.TrackAware)
    {
        if (startPosition == targetPosition)
        {
            return new List<Point>();
        }

        var nodesByPos = new Dictionary<Point, Node>();

        // Prioritise nodes first by cost, and then by insertion order (since we add by N > E > S > W)
        var queue = new PriorityQueue<Node, (int Cost, int Order)>();
        int insertionCounter = 0;

        var startNode = new Node(startPosition)
        {
            G = 0,
            FirstMove = null
        };

        nodesByPos[startPosition] = startNode;
        queue.Enqueue(startNode, (startNode.G, insertionCounter++));

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

                int newG = current.G + GetCost(pt, costMode);

                if (nodesByPos.TryGetValue(pt, out var existingNode))
                {
                    if (newG < existingNode.G)
                    {
                        existingNode.G = newG;
                        existingNode.Parent = current;
                        existingNode.FirstMove = current.Position == startPosition ? pt : current.FirstMove;

                        queue.Enqueue(existingNode, (newG, insertionCounter++));
                    }

                    continue;
                }

                var node = new Node(pt)
                {
                    Parent = current,
                    G = newG,
                    FirstMove = current.Position == startPosition ? pt : current.FirstMove
                };

                nodesByPos[pt] = node;
                queue.Enqueue(node, (newG, insertionCounter++));
            }
        }

        // no path found
        return new List<Point>();
    }

    private int GetCost(Point point, PathCostMode costMode)
    {
        if (costMode == PathCostMode.TrackAware
            && !_map.isTrackFree(point.X, point.Y))
        {
            return 0;
        }

        int baseCost = _map.CellCosts[point.X, point.Y] * COST_SCALE;

        float weight = 1.0f;

        // Add weigthtings
        if (_map.IsInTown(point.X, point.Y))
        {
            weight *= SAFE_REGION_WEIGHT;
        }

        return (int)(baseCost * weight);
    }
}