using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;

namespace _2026_1_WinterChallenge_SnakeByte;

internal sealed class PathFinder
{
    private Game _game;
    private readonly PositionChecker _positionChecker;
    private readonly MovementHelper _movementHelper;
    private int _debugCount = 0;

    private const int MAX_NODE_COUNT = 300;
    private const int MAX_EXPANSIONS_WITHOUT_H_IMPROVEMENT = 40;

    public PathFinder(Game game, PositionChecker positionChecker, MovementHelper movementHelper)
    {
        _game = game;
        _positionChecker = positionChecker;
        _movementHelper = movementHelper;
    }

    internal List<Point> GetShortestPath(Point startPoint, 
                                         Point targetPoint, 
                                         SnakeBot snake, 
                                         List<Point> excludePoints, 
                                         HashSet<Point> solidPoints, 
                                         HashSet<Point> powerUpPoints)
    {
        // TODO: collisionPoints and platformPoints might be the same now. Consolidate

        _debugCount = 0;

        Dictionary<SnakeState, Node> nodesByState = new Dictionary<SnakeState, Node>();
        PriorityQueue<Node, int> openNodes = new PriorityQueue<Node, int>();

        Node currentNode = new Node(startPoint);
        currentNode.SnakeBodyAtNode = snake.Body.Select(p => new Point(p.X, p.Y)).ToList();
        currentNode.H = CalculationUtil.GetManhattanDistance(startPoint, targetPoint);

        SnakeState startState = new SnakeState(startPoint, currentNode.SnakeBodyAtNode);

        nodesByState.Add(startState, currentNode);
        openNodes.Enqueue(currentNode, currentNode.F);
        int openNodeCount = 1;

        bool targetFound = false;

        int bestHSeen = currentNode.H;
        int expansionsWithoutHImprovement = 0;        

        while (!targetFound)
        {
            // TODO: I need to experiment with this number.
            if (openNodeCount == 0 || nodesByState.Count > MAX_NODE_COUNT)
            {
                return new List<Point>();
            }

            if (currentNode.H < bestHSeen)
            {
                bestHSeen = currentNode.H;
                expansionsWithoutHImprovement = 0;
            }
            else
            {
                // Only count stagnation when gravity is holding the snake in place
                bool gravityHeldPosition = currentNode.Parent != null
                    && currentNode.Position == currentNode.Parent.Position;

                if (gravityHeldPosition)
                {
                    expansionsWithoutHImprovement++;
                    if (expansionsWithoutHImprovement >= MAX_EXPANSIONS_WITHOUT_H_IMPROVEMENT)
                    {
                        return new List<Point>();
                    }
                }
            }

            List<Point> pointsToCheck = new List<Point>();

            if (currentNode.Position.X + 1 <= _game.Width)
            {
                pointsToCheck.Add(new Point(currentNode.Position.X + 1, currentNode.Position.Y));
            }
            
            if (currentNode.Position.X - 1 >= -1)
            {
                pointsToCheck.Add(new Point(currentNode.Position.X - 1, currentNode.Position.Y));
            }

            if (currentNode.Position.Y + 1 < _game.Height)
            {
                pointsToCheck.Add(new Point(currentNode.Position.X, currentNode.Position.Y + 1));
            }

            if (currentNode.Position.Y - 1 >= -1)
            {
                pointsToCheck.Add(new Point(currentNode.Position.X, currentNode.Position.Y - 1));
            }

            if(pointsToCheck.Count == 0)
            {
                Console.Error.WriteLine("ERROR: No points to check from current node");
            }

            foreach (Point pointToCheck in pointsToCheck)
            {
                // If pointToCheck is the same as the current point skip it
                if (pointToCheck == currentNode.Position)
                {
                    continue;
                }

                // If we are expanding the start node, ignore the excludePoints
                if (currentNode.Parent == null && excludePoints.Contains(pointToCheck))
                {
                    continue;
                }

                // We can check the majority of collisions before simulating snake movement, which is expensive.
                // We only need to simulate the movement if the point is not an immediate collision.

                bool isValidMove =
                    pointToCheck == targetPoint
                    || (!solidPoints.Contains(pointToCheck)
                        && !IsSelfCollision(pointToCheck, currentNode.SnakeBodyAtNode, checkHead: true, checkTail: false));


                if (!isValidMove)
                {
                    continue;
                }

                isValidMove = true;

                // Simulate snake movement to this position
                List<Point> snakeBodyAfterMove = _movementHelper.SimulateSnakeMovement(
                    currentNode.SnakeBodyAtNode,
                    currentNode.Position,
                    pointToCheck,
                    powerUpPoints);

                // TODO: We can check most of the below before simulating snake movement. 
                // We can opt out before it on everything except current snake checks.
                // Check if the move is valid. If it's not we don't want to continue
                isValidMove =
                    !IsSelfCollision(pointToCheck, snakeBodyAfterMove, checkHead: false, checkTail: true)
                    && !IsFullyOutOfBounds(snakeBodyAfterMove);

                if (!isValidMove)
                {
                    continue;
                }

                bool onPowerUp = powerUpPoints.Contains(pointToCheck);

                // Apply gravity to the simulated body UNLESS we're on any power up (including the target)
                // (reaching the target means eating a power-up, gravity doesn't apply mid-eating)
                if (!onPowerUp)
                {
                    snakeBodyAfterMove = _movementHelper.ApplyGravity(snakeBodyAfterMove, solidPoints);
                }

                // IMPORTANT: After gravity, the head position may have changed!
                Point actualHeadPosition = snakeBodyAfterMove[0];

                SnakeState newState = new SnakeState(actualHeadPosition, snakeBodyAfterMove);

                // Check if a node at this actual position already exists
                nodesByState.TryGetValue(newState, out Node? existingNode);

                if (existingNode == null)
                {
                    // Create node at the ACTUAL position after gravity
                    Node node = new Node(actualHeadPosition);
                    node.IntendedMove = pointToCheck;
                    node.Parent = currentNode;
                    node.G = currentNode.G + 1;
                    node.H = CalculationUtil.GetManhattanDistance(actualHeadPosition, targetPoint);
                    node.F = node.G + node.H;
                    node.SnakeBodyAtNode = snakeBodyAfterMove;

                    nodesByState.Add(newState, node);

                    // Check if we've reached the target immediately
                    if (actualHeadPosition == targetPoint)
                    {
                        currentNode = node;
                        targetFound = true;
                        break; // Exit the foreach loop
                    }

                    // Check if we've on another power up
                    if (onPowerUp)
                    {
                        currentNode = node;
                        targetFound = true;
                        break; // Exit the foreach loop
                    }

                    openNodes.Enqueue(node, node.F);
                    openNodeCount++;
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
                            existingNode.SnakeBodyAtNode = snakeBodyAfterMove;
                            openNodes.Enqueue(existingNode, existingNode.F);
                        }
                    }
                }
            }

            currentNode.Closed = true;
            openNodeCount--;

            if (currentNode.Position == targetPoint || targetFound)
            {
                targetFound = true;
            }
            else
            {
                Node? nextNode = null;
                while (openNodes.Count > 0 && nextNode == null)
                {
                    Node candidate = openNodes.Dequeue();
                    if (!candidate.Closed)
                    {
                        nextNode = candidate;
                    }
                }

                if (nextNode == null)
                {
                    return new List<Point>();
                }

                currentNode = nextNode;
            }

            _debugCount++;
        }

        // Build the path
        List<Point> shortestPath = new List<Point>();
        Node pathNode = currentNode;

        while (pathNode?.Parent != null)
        {
            // The move is the direction from parent's ACTUAL position to the intermediate position
            // before gravity was applied in pathNode
            shortestPath.Insert(0, pathNode.IntendedMove);
            pathNode = pathNode.Parent;
        }

        return shortestPath;
    }

    private bool IsFullyOutOfBounds(List<Point> snakeBodyAfterMove)
    {
        foreach (var bodyPart in snakeBodyAfterMove)
        {
            if (!_positionChecker.IsOutOfMapBounds(bodyPart))
            {
                return false;
            }
        }
        return true;
    }

    private bool IsSelfCollision(Point pointToCheck, List<Point> snakeBodyPoints, bool checkHead, bool checkTail)
    {
        int startPoint = checkHead ? 0 : 1;
        int endPoint = checkTail ? snakeBodyPoints.Count - 1  : snakeBodyPoints.Count - 2;

        // Check the current snake body, skipping the head and tail
        for (int i = startPoint; i <= endPoint; i++)
        {
            if (snakeBodyPoints[i] == pointToCheck)
            {
                return true;
            }
        }

        return false;
    }
}


