using System.Drawing;

namespace _2026_1_WinterChallenge_SnakeByte;

internal class Game
{
    internal int Width { get; private set; }
    internal int Height { get; private set; }

    private Level _level;

    internal List<SnakeBot> MySnakeBots { get; set; }
    internal List<SnakeBot> OpponentSnakeBots { get; set; }

    private PathFinder _pathFinder;

    private List<Point> _movesThisTurn;

    public Game(int width, int height, bool[,] platforms)
    {
        Width = width;
        Height = height;

        _level = new Level(width, height, platforms);

        _pathFinder = new PathFinder(this);

        MySnakeBots = new List<SnakeBot>();
        OpponentSnakeBots = new List<SnakeBot>();
    }

    internal void MarkAllSnakesForRemoval()
    {
       foreach (var snakeBot in MySnakeBots)
        {
            snakeBot.Remove = true;
        }

        foreach (var snakeBot in OpponentSnakeBots)
        {
            snakeBot.Remove = true;
        }
    }

    internal void RemoveMarkedSnakes()
    {

        // Iterate through the snakes backwards to safely remove any where Remove == true
        for (int i = MySnakeBots.Count - 1; i >= 0; --i)
        {
            if (MySnakeBots[i].Remove)
            {
                MySnakeBots.RemoveAt(i);
            }
        }

        for (int i = OpponentSnakeBots.Count - 1; i >= 0; --i)
        {
            if (OpponentSnakeBots[i].Remove)
            {
                OpponentSnakeBots.RemoveAt(i);
            }
        }
    }

    internal void AddMySnake(SnakeBot snakeBot)
    {
        MySnakeBots.Add(snakeBot);
    }

    internal void AddOpponentSnake(SnakeBot snakeBot)
    {
        OpponentSnakeBots.Add(snakeBot);
    }

    internal SnakeBot GetSnake(int snakebotId)
    {
        return MySnakeBots.FirstOrDefault(s => s.Id == snakebotId) ?? OpponentSnakeBots.FirstOrDefault(s => s.Id == snakebotId);
    }

    internal void RemoveAllPowerSources()
    {
        _level.PowerSources.Clear();
    }

    internal void AddPowerSource(int x, int y)
    {
        _level.PowerSources.Add(new Point(x, y));
    }

    internal List<string> GetActions()
    {
        _movesThisTurn = new List<Point>();
        // Logger.EntireGame(_level.Platforms, MySnakeBots, OpponentSnakeBots, _level.PowerSources);

        List<string> actions = new List<string>();

        foreach (var snakeBot in MySnakeBots)
        {
            Console.Error.WriteLine($"Checking Snake {snakeBot.Id}");
            Console.Error.WriteLine($"_movesThisTurn: {string.Join(";", _movesThisTurn.Select(p => $"{p.X},{p.Y}"))}");
            int shortestPathCount = int.MaxValue;
            var shortestPathPoints = new List<Point>();

            // Use an iterative deepening approach to finding targets
            bool stopLooking = false;
            int maxDistance = 5;

            //while (stopLooking == false)
            //{
                Console.Error.WriteLine($"maxDistance: {maxDistance}");
                (List<Point> path, bool triedSomething) = GetShortestPath(snakeBot, Math.Min(shortestPathCount-1, maxDistance));

                if (path.Count > 0)
                {
                    Console.Error.WriteLine($"Found a path of length {path.Count} to a power source");
                    stopLooking = true;

                    shortestPathCount = path.Count;
                    shortestPathPoints = path.ToList();
                }

                if (stopLooking == false && triedSomething == true)
                {
                    // We tried a closer one and couldn't get to it. For now, don't try more
                    stopLooking = true;
                }                

            //    maxDistance += 5;
            //    if (maxDistance > 10)
            //    {
            //        stopLooking = true;
            //    }
            //}

            Console.Error.WriteLine(string.Join(";", shortestPathPoints.Select(p => $"{p.X},{p.Y}")));

            if (shortestPathPoints.Count == 0)
            {
                string direction = GetValidDirection(snakeBot);

                actions.Add($"{snakeBot.Id} {direction}");
                snakeBot.AddMove(DirectionHelper.GetNewPosition(snakeBot.Body[0], direction));
                _movesThisTurn.Add(DirectionHelper.GetNewPosition(snakeBot.Body[0], direction));
            }
            else
            {
                Console.Error.WriteLine($"Patrh fount to {shortestPathPoints[shortestPathPoints.Count-1].X},{shortestPathPoints[shortestPathPoints.Count - 1].Y}, moving towards it");
                string direction = DirectionHelper.GetDirection(snakeBot.Body[0], shortestPathPoints[0]);

                actions.Add($"{snakeBot.Id} {direction}");
                snakeBot.AddMove(shortestPathPoints[0]);
                _movesThisTurn.Add(shortestPathPoints[0]);
            }
        }

        return actions;
    }

    private string GetValidDirection(SnakeBot snakeBot)
    {
        var possibleDirections = new List<string>() { "UP", "DOWN", "LEFT", "RIGHT" };
        foreach (var direction in possibleDirections)
        {
            Point newHeadPosition = DirectionHelper.GetNewPosition(snakeBot.Body[0], direction);
            if (newHeadPosition.X >= 0
                && newHeadPosition.X < Width
                && newHeadPosition.Y >= 0
                && newHeadPosition.Y < Height
                && !IsPlatform(newHeadPosition)
                && !IsSnakePart(newHeadPosition, countTails: true, null)
                && _movesThisTurn.Contains(newHeadPosition) == false
                && !IsBlocking(newHeadPosition, snakeBot)
                && !ExcludeMove(newHeadPosition, snakeBot))
            {
                return direction;
            }
        }
        // No valid moves, just stay there and hope for the best
        return "LEFT";
    }

    private bool ExcludeMove(Point newHeadPosition, SnakeBot snakeBot)
    {
        if (snakeBot.IsStuck())
        {
            if (newHeadPosition == snakeBot.GetLastMove())
            {
                return true;
            }
        }

        return false;
    }

    private bool IsBlocking(Point newHeadPosition, SnakeBot snakeBot)
    {
        Console.Error.WriteLine($"Checking if move to {newHeadPosition.X},{newHeadPosition.Y} would block us in");
        // Flood fill algorithm to check if the new position would block the snake in
        var visited = new HashSet<Point>();
        var queue = new Queue<Point>();

        queue.Enqueue(newHeadPosition);
        visited.Add(newHeadPosition);

        while (queue.Count > 0)
        {
            Point checkPoint = queue.Dequeue();

            var adjacentPoints = new List<Point>()
            {
                new Point(checkPoint.X + 1, checkPoint.Y),
                new Point(checkPoint.X - 1, checkPoint.Y),
                new Point(checkPoint.X, checkPoint.Y + 1),
                new Point(checkPoint.X, checkPoint.Y - 1)
            };

            foreach (var adjacentPoint in adjacentPoints)
            {
                if (adjacentPoint.X >= 0
                    && adjacentPoint.X < Width
                    && adjacentPoint.Y >= 0
                    && adjacentPoint.Y < Height
                    && !IsPlatform(adjacentPoint)
                    && !IsSnakePart(adjacentPoint, countTails: true, null)
                    && !visited.Contains(adjacentPoint))
                {
                    queue.Enqueue(adjacentPoint);
                    visited.Add(adjacentPoint);
                }
            }
        }

        // If we visited less than 3 points, we are probably blocking ourselves in
        if (visited.Count < snakeBot.Body.Count)
        {
            return true;
        }

        return false;
    }

    private (List<Point>, bool) GetShortestPath(SnakeBot snakeBot, int maxDistance)
    {
        bool triedSomething = false;
        int shortestPathCount = int.MaxValue;
        var shortestPathPoints = new List<Point>();

        foreach (Point powerSource in _level.PowerSources)
        {
            // Don't bother trying if it's further away than the shortest one we've found
            if (CalculationUtil.GetManhattanDistance(snakeBot.Body[0], powerSource) >= maxDistance || CalculationUtil.GetManhattanDistance(snakeBot.Body[0], powerSource) >= shortestPathCount)
            {
                continue;
            }

            if (snakeBot.GetAttemptAtPowerSource(powerSource) > 20)
            {
                snakeBot.ClearAttemptsAtPowerSource(powerSource);
                continue;                
            }

            snakeBot.AddAttemptAtPowerSource(powerSource);

            List<Point> excludePoints = new List<Point>();
            if (snakeBot.IsStuck())
            {
                excludePoints.Add(snakeBot.GetLastMove());
            }

            List<Point> path = _pathFinder.GetShortestPath(snakeBot.Body.First(), powerSource, snakeBot, excludePoints.Concat(_movesThisTurn).ToList());
            triedSomething = true;

            if (path != null && path.Count > 0 && path.Count < shortestPathCount)
            {
                shortestPathCount = path.Count;
                shortestPathPoints = path.ToList();
            }
        }

        return (shortestPathPoints, triedSomething);
    }

    internal bool IsPlatform(Point pointToCheck)
    {
        if (_level.IsPlatform(pointToCheck))
        {
            return true;
        }

        return false;
    }

    internal bool IsSnakePart(Point pointToCheck, bool countTails, SnakeBot? excludeSnake)
    {
        foreach (var snakeBot in MySnakeBots)
        {
            if (excludeSnake != null && snakeBot == excludeSnake)
            {
                continue;
            }

            if(IsSnakePart(snakeBot, pointToCheck, countTails))
            {
                return true;
            }
        }

        foreach (var snakeBot in OpponentSnakeBots)
        {
            if (snakeBot.Body.Contains(pointToCheck))
            {
                if (IsSnakePart(snakeBot, pointToCheck, countTails))
                {
                    return true;
                }
            }
        }

        return false;
    }

    internal bool IsSnakePart(SnakeBot snakeBot, Point pointToCheck, bool countTails)
    {
        if (snakeBot.Body.Contains(pointToCheck))
        {
            if (!countTails)
            {
                if (snakeBot.Body[snakeBot.Body.Count - 1] == pointToCheck)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }

        return false;
    }

    internal List<Point> GetPowerUps()
    {
        return _level.PowerSources;
    }
}
