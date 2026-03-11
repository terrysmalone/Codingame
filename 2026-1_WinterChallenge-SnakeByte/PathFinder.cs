using System.Drawing;
using System.Xml.Linq;

namespace _2026_1_WinterChallenge_SnakeByte;

internal sealed class PathFinder
{
    private Game _game;
    private readonly PositionChecker _positionChecker;
    private int _debugCount = 0;

    public PathFinder(Game game, PositionChecker positionChecker)
    {
        _game = game;
        _positionChecker = positionChecker;
    }

    internal List<Point> GetShortestPath(Point startPoint, Point targetPoint, SnakeBot snake, List<Point> excludePoints)
    {
        _debugCount = 0;
        SnakeBot currentSnake = new SnakeBot(snake.Id)
        {
            Body = snake.Body.Select(p => new Point(p.X, p.Y)).ToList()
        };

        Dictionary<Point, Node> nodesByPosition = new Dictionary<Point, Node>();
        PriorityQueue<Node, int> openNodes = new PriorityQueue<Node, int>();

        Node currentNode = new Node(startPoint);
        currentNode.SnakeBodyAtNode = snake.Body.Select(p => new Point(p.X, p.Y)).ToList();
        nodesByPosition.Add(startPoint, currentNode);
        openNodes.Enqueue(currentNode, currentNode.F);
        int openNodeCount = 1;

        bool targetFound = false;

        while (!targetFound)
        {
            if (openNodeCount == 0)
            {
                return new List<Point>();
            }

            if (nodesByPosition.Count > 20)
            {
                Console.Error.WriteLine($"NodeByPostion abouve 20. Cutting out");
                return new List<Point>();
            }

            Point[] pointsToCheck = new Point[4];

            // Prioritise heading towards the target as the first move
            if (Math.Abs(currentNode.Position.X - targetPoint.X) >= Math.Abs(currentNode.Position.Y - targetPoint.Y))
            {
                pointsToCheck[0] = new Point(Math.Min(_game.Width, currentNode.Position.X + 1), currentNode.Position.Y);
                pointsToCheck[1] = new Point(Math.Max(-1, currentNode.Position.X - 1), currentNode.Position.Y);
                pointsToCheck[2] = new Point(currentNode.Position.X, Math.Min(_game.Height, currentNode.Position.Y + 1));
                pointsToCheck[3] = new Point(currentNode.Position.X, Math.Max(-1, currentNode.Position.Y - 1));
            }
            else
            {
                pointsToCheck[0] = new Point(currentNode.Position.X, Math.Min(_game.Height, currentNode.Position.Y + 1));
                pointsToCheck[1] = new Point(currentNode.Position.X, Math.Max(-1, currentNode.Position.Y - 1));
                pointsToCheck[2] = new Point(Math.Min(_game.Width, currentNode.Position.X + 1), currentNode.Position.Y);
                pointsToCheck[3] = new Point(Math.Max(-1, currentNode.Position.X - 1), currentNode.Position.Y);
            }

            foreach (Point pointToCheck in pointsToCheck)
            {
                // If we are expanding the start node, ignore the excludePoints
                if (currentNode.Parent == null && excludePoints.Contains(pointToCheck))
                {
                    continue;
                }

                // Simulate snake movement to this position
                List<Point> snakeBodyAfterMove = SimulateSnakeMovement(
                    currentNode.SnakeBodyAtNode,
                    currentNode.Position,
                    pointToCheck);

                // Apply gravity to the simulated body
                snakeBodyAfterMove = ApplyGravity(snakeBodyAfterMove, currentSnake.Id);

                // IMPORTANT: After gravity, the head position may have changed!
                Point actualHeadPosition = snakeBodyAfterMove[0];

                // Check if a node at this actual position already exists
                nodesByPosition.TryGetValue(actualHeadPosition, out Node? existingNode);

                if (existingNode == null)
                {
                    // Check if the intended move is valid (before gravity)
                    if (pointToCheck == startPoint
                        || pointToCheck == targetPoint
                        || !IsPlatform(pointToCheck, snake, currentSnake))
                    {
                        // Create node at the ACTUAL position after gravity
                        Node node = new Node(actualHeadPosition);
                        node.Parent = currentNode.Position;
                        node.G = currentNode.G + 1;
                        node.H = CalculationUtil.GetManhattanDistance(actualHeadPosition, targetPoint);
                        node.F = node.G + node.H;
                        node.SnakeBodyAtNode = snakeBodyAfterMove;

                        nodesByPosition.Add(actualHeadPosition, node);
                        openNodes.Enqueue(node, node.F);
                        openNodeCount++;
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
                            existingNode.SnakeBodyAtNode = snakeBodyAfterMove;
                            openNodes.Enqueue(existingNode, existingNode.F);
                        }
                    }
                }
            }

            currentNode.Closed = true;
            openNodeCount--;

            if (currentNode.Position == targetPoint)
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

        while (pathNode.Parent != null)
        {
            Node parentNode = nodesByPosition[pathNode.Parent.Value];

            // The move is the direction from parent's ACTUAL position to the intermediate position
            // before gravity was applied in pathNode
            shortestPath.Insert(0, pathNode.Position);
            pathNode = parentNode;
        }

        return shortestPath;
    }

    private List<Point> ApplyGravity(List<Point> snakeBody, int id)
    {
        bool canMoveDown = true;
        while (canMoveDown)
        {
            // Check if we can move down
            if (snakeBody.Any(p => p.Y + 1 >= _game.Height
                                || _positionChecker.IsPlatform(new Point(p.X, p.Y + 1))
                                || _game.MySnakeBots.Any(s => s.Body.Any(bp => bp.X == p.X && bp.Y == p.Y + 1 && s.Id != id))
                                || _game.OpponentSnakeBots.Any(s => s.Body.Any(bp => bp.X == p.X && bp.Y == p.Y + 1 && s.Id != id))
                                || _game.GetPowerUps().Any(pu => pu.X == p.X && pu.Y == p.Y + 1)))
            {
                canMoveDown = false;
            }
            else
            {
                // Move the snake down by one
                for (int i = 0; i < snakeBody.Count; ++i)
                {
                    snakeBody[i] = new Point(snakeBody[i].X, snakeBody[i].Y + 1);
                }
            }
        }

        return snakeBody;
    }

    private List<Point> SimulateSnakeMovement(List<Point> currentBody, Point currentHead, Point newHead)
    {
        List<Point> newBody = currentBody.Select(p => new Point(p.X, p.Y)).ToList();
        newBody.Insert(0, newHead);

        List <Point> powerSources = _game.GetPowerUps();

        if(powerSources.Contains(newHead))
        {
            // Don't remove the tail if we are eating a power source
            return newBody;
        }

        newBody.RemoveAt(newBody.Count - 1);

        return newBody;
    }

    private bool IsPlatform(Point pointToCheck, SnakeBot excludeSnake, SnakeBot currentSnake)
    {
        // Check that it's not a platform

        // Check that it's not a snakes head or body

        if (_positionChecker.IsPlatform(pointToCheck))
        {
            return true;
        }


        // Check all snakes except the current one
        if (_positionChecker.IsSnakePart(pointToCheck, countTails: true, excludeSnake: excludeSnake))
        {
            return true; 
        }

        // Check the current snake
        if (_positionChecker.IsSnakePart(currentSnake, pointToCheck, countTails: false))
        {
            return true;
        }

        return false;
    }    
}
