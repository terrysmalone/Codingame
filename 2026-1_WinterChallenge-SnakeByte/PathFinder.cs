using System.Drawing;
using System.Xml.Linq;

namespace _2026_1_WinterChallenge_SnakeByte;

internal sealed class PathFinder
{
    private Game _game;

    private int _debugCount = 0;

    public PathFinder(Game game)
    {
        _game = game;
    }

    internal List<Point> GetShortestPath(Point startPoint, Point targetPoint, SnakeBot snake, List<Point> excludePoints)
    {
        _debugCount = 0;
        SnakeBot currentSnake = new SnakeBot(snake.Id)
        {
            // create a deep copy of snake.body so that we can modify it without affecting the original snake
            Body = snake.Body.Select(p => new Point(p.X, p.Y)).ToList()
        };

        List<Node> nodes = new List<Node>();

        Node currentNode = new Node(startPoint);

        // Store initial snake body
        currentNode.SnakeBodyAtNode = snake.Body.Select(p => new Point(p.X, p.Y)).ToList();
        nodes.Add(currentNode);

        bool targetFound = false;

        while (!targetFound)
        {
            if (nodes.Count(n => n.Closed == false) == 0)
            {
                return new List<Point>();
            }

            if (nodes.Count > 100)
            {
                Console.Error.WriteLine("Too many nodes, breaking out of loop");
                return new List<Point>();
            }

            Point[] pointsToCheck = new Point[4];

            // Prioritise heading towards the target as the first move
            if (Math.Abs(startPoint.X - targetPoint.X) >= Math.Abs(startPoint.Y - targetPoint.Y))
            {
                pointsToCheck[0] = new Point(Math.Min(_game.Width - 1, currentNode.Position.X + 1), currentNode.Position.Y);
                pointsToCheck[1] = new Point(Math.Max(0, currentNode.Position.X - 1), currentNode.Position.Y);
                pointsToCheck[2] = new Point(currentNode.Position.X, Math.Min(_game.Height - 1, currentNode.Position.Y + 1));
                pointsToCheck[3] = new Point(currentNode.Position.X, Math.Max(0, currentNode.Position.Y - 1));
            }
            else
            {
                pointsToCheck[0] = new Point(currentNode.Position.X, Math.Min(_game.Height - 1, currentNode.Position.Y + 1));
                pointsToCheck[1] = new Point(currentNode.Position.X, Math.Max(0, currentNode.Position.Y - 1));
                pointsToCheck[2] = new Point(Math.Min(_game.Width - 1, currentNode.Position.X + 1), currentNode.Position.Y);
                pointsToCheck[3] = new Point(Math.Max(0, currentNode.Position.X - 1), currentNode.Position.Y);
            }

            foreach (Point pointToCheck in pointsToCheck)
            {
                // If there is only one node, we are at the start and we want to ignore the excludePoints
                if (nodes.Count == 1 && excludePoints.Contains(pointToCheck))
                {
                    continue;
                }


                Node? existingNode = nodes.SingleOrDefault(n => n.Position == pointToCheck);

                if (existingNode == null)
                {
                    // Simulate snake movement to this position
                    List<Point> snakeBodyAfterMove = SimulateSnakeMovement(
                        currentNode.SnakeBodyAtNode,
                        currentNode.Position,
                        pointToCheck);

                    // Apply gravity to the simulated body
                    snakeBodyAfterMove = ApplyGravity(snakeBodyAfterMove, currentSnake.Id);

                    if (pointToCheck == startPoint
                        || pointToCheck == targetPoint
                        || !IsPlatform(pointToCheck, snake, currentSnake))
                    {
                        Node node = new Node(pointToCheck);

                        node.Parent = currentNode.Position;

                        node.G = currentNode.G + 1;

                        node.H = CalculationUtil.GetManhattanDistance(pointToCheck, targetPoint);
                        node.F = node.G + node.H;
                        node.SnakeBodyAtNode = snakeBodyAfterMove;

                        nodes.Add(node);
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

                            // Update snake body for this path
                            List<Point> snakeBodyAfterMove = SimulateSnakeMovement(
                                currentNode.SnakeBodyAtNode,
                                currentNode.Position,
                                pointToCheck);
                            existingNode.SnakeBodyAtNode = ApplyGravity(snakeBodyAfterMove, currentSnake.Id);
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
                nodes = nodes.OrderBy(n => n.Closed == true).ThenBy(n => n.F).ToList();

                currentNode = nodes.First();
            }

            _debugCount++;
        }

        Console.Error.WriteLine($"Debug count: {_debugCount}");

        int numberOfSteps = currentNode.G;

        List<Point> shortestPath = [currentNode.Position];

        bool atStart = false;

        while (!atStart)
        {
            Console.Error.WriteLine("X");
            currentNode = nodes.Single(n => n.Position == currentNode.Parent);

            if (currentNode.Position == startPoint)
            {
                atStart = true;
            }
            else
            {
                shortestPath.Insert(0, currentNode.Position);
            }
        }

        Console.Error.WriteLine("Created shortestPath");

        return shortestPath;
    }

    private List<Point> ApplyGravity(List<Point> snakeBody, int id)
    {
        bool canMoveDown = true;
        while (canMoveDown)
        {
            // Check if we can move down
            if (snakeBody.Any(p => p.Y + 1 >= _game.Height
                                || _game.IsPlatform(new Point(p.X, p.Y + 1))
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

        if (_game.IsPlatform(pointToCheck))
        {
            return true;
        }


        // Check all snakes except the current one
        if (_game.IsSnakePart(pointToCheck, countTails: true, excludeSnake: excludeSnake))
        {
            return true; 
        }

        // Check the current snake
        if (_game.IsSnakePart(currentSnake, pointToCheck, countTails: false))
        {
            return true;
        }

        return false;
    }
}
