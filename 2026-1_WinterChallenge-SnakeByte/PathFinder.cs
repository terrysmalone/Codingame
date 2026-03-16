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
    private int _debugCount = 0;

    private const int MAX_NODE_COUNT = 300;
    private const int MAX_EXPANSIONS_WITHOUT_H_IMPROVEMENT = 40;

    public PathFinder(Game game, PositionChecker positionChecker)
    {
        _game = game;
        _positionChecker = positionChecker;
    }

    internal List<Point> GetShortestPath(Point startPoint, Point targetPoint, SnakeBot snake, List<Point> excludePoints)
    {
        // Calculate points we use for collision detection and gravity, then we can use it for every search
        // Note: This doesn't include the current snake body since that will be moving as we simulate movement

        HashSet<Point> powerUpPoints = _game.GetPowerSources().ToHashSet(); 
        HashSet<Point> collisionPoints = BuildCollisionPoints(snake.Id, powerUpPoints);
        HashSet<Point> platformPoints = BuildPlatformPoints(snake.Id, powerUpPoints);
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
                expansionsWithoutHImprovement++;
                if (expansionsWithoutHImprovement >= MAX_EXPANSIONS_WITHOUT_H_IMPROVEMENT)
                {
                    return new List<Point>();
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

            if (currentNode.Position.Y + 1 <= _game.Height)
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
                    || (!collisionPoints.Contains(pointToCheck)
                        && !IsSelfCollision(pointToCheck, currentNode.SnakeBodyAtNode, checkHead: true, checkTail: false));


                if (!isValidMove)
                {
                    continue;
                }

                isValidMove = true;

                // Simulate snake movement to this position
                List<Point> snakeBodyAfterMove = SimulateSnakeMovement(
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
                    snakeBodyAfterMove = ApplyGravity(snakeBodyAfterMove, platformPoints);
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

    private HashSet<Point> BuildPlatformPoints(int excludeSnakeId, HashSet<Point> powerUpPoints)
    {
        HashSet<Point> collisionPoints = new HashSet<Point>();

        foreach (var snake in _game.MySnakeBots)
        {
            if (snake.Id == excludeSnakeId)
            {
                continue;
            }
            foreach (var bodyPart in snake.Body)
            {
                collisionPoints.Add(bodyPart);
            }
        }

        foreach (var snake in _game.OpponentSnakeBots)
        {
            foreach (var bodyPart in snake.Body)
            {
                collisionPoints.Add(bodyPart);
            }
        }

        foreach (var platform in _game.GetAllPlatformPositions())
        {
            collisionPoints.Add(platform);
        }

        foreach (var powerUp in powerUpPoints)
        {
            collisionPoints.Add(powerUp);
        }

        return collisionPoints;
    }

    private HashSet<Point> BuildCollisionPoints(int excludeSnakeId, HashSet<Point> powerUpPoints)
    {
        HashSet<Point> collisionPoints = new HashSet<Point>();

        foreach (var snake in _game.MySnakeBots)
        {
            if (snake.Id == excludeSnakeId)
            {
                continue;
            }
            foreach (var bodyPart in snake.Body)
            {
                collisionPoints.Add(bodyPart);
            }
        }

        foreach (var snake in _game.OpponentSnakeBots)
        {
            foreach (var bodyPart in snake.Body)
            {
                collisionPoints.Add(bodyPart);
            }
        }

        foreach (var platform in _game.GetAllPlatformPositions())
        {
            collisionPoints.Add(platform);
        }

        foreach (var powerUp in powerUpPoints)
        {
            collisionPoints.Add(powerUp);
        }

        return collisionPoints;
    }

    private List<Point> ApplyGravity(List<Point> snakeBody, HashSet<Point> platformPoints)
    {
        int count = 0;
        bool canMoveDown = true;
        while (canMoveDown)
        {
            foreach (var bodyPart in snakeBody)
            {
                Point bodyCheckPoint = new Point(bodyPart.X, bodyPart.Y + 1);

                if (platformPoints.Contains(bodyCheckPoint))
                {
                    canMoveDown = false;
                    break;
                }
            }

            if (canMoveDown)
            {
                // Move the snake down by one
                for (int i = 0; i < snakeBody.Count; ++i)
                {
                    snakeBody[i] = new Point(snakeBody[i].X, snakeBody[i].Y + 1);
                }
            }

            count++;
            if (count > 20)
            {
                Console.Error.WriteLine($"ERROR: Gravity count exceeded max of 20");
                Console.Error.WriteLine($"ERROR: Snake body: {string.Join(";", snakeBody.Select(p => $"({p.X},{p.Y})"))}");
            }
        }

        return snakeBody;
    }

    private List<Point> SimulateSnakeMovement(List<Point> currentBody, Point currentHead, Point newHead, HashSet<Point> powerUpPoints)
    {
        List<Point> newBody = currentBody.Select(p => new Point(p.X, p.Y)).ToList();
        newBody.Insert(0, newHead);

        if(powerUpPoints.Contains(newHead))
        {
            // Don't remove the tail if we are eating a power source
            return newBody;
        }

        newBody.RemoveAt(newBody.Count - 1);

        return newBody;
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

