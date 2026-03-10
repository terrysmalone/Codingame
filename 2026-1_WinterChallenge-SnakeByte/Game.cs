using System.Drawing;

namespace _2026_1_WinterChallenge_SnakeByte;

internal class Game
{
    internal int Width { get; private set; }
    internal int Height { get; private set; }

    private Level _level;

    internal List<SnakeBot> MySnakeBots { get; set; }
    internal List<SnakeBot> OpponentSnakeBots { get; set; }
    public int TurnCount { get; internal set; } = 0;

    private PathFinder _pathFinder;

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
        // Logger.EntireGame(_level.Platforms, MySnakeBots, OpponentSnakeBots, _level.PowerSources);

        //List<string> actions = GetBestPathActions();

        List<string> actions = GetMiniMaxActions();
        


        return actions;
    }

    private List<string> GetMiniMaxActions()
    {
        GameState gameState = new GameState(this, MySnakeBots, OpponentSnakeBots, _level.PowerSources, TurnCount);
        SimultaneousMiniMax miniMax = new SimultaneousMiniMax();
        MoveSet bestMoveSet = miniMax.FindBestMoveSet(gameState);

        List<string> actions = new List<string>();

        foreach (var move in bestMoveSet.Moves)
        {
            actions.Add($"{move.SnakeId} {move.Direction}");
        }

        return actions;
    }

    private List<string> GetBestPathActions()
    {
        List<string> actions = new List<string>();

        foreach (var snakeBot in MySnakeBots)
        {
            Console.Error.WriteLine($"Checking Snake {snakeBot.Id}");
            int shortestPathCount = int.MaxValue;
            var shortestPathPoints = new List<Point>();

            // Use an iterative deepening approach to finding targets
            bool stopLooking = false;
            int maxDistance = 5;

            while (stopLooking == false)
            {
                Console.Error.WriteLine($"Trying to find target within distance {Math.Min(shortestPathCount - 1, maxDistance)}");

                (List<Point> path, bool triedSomething) = GetShortestPath(snakeBot, Math.Min(shortestPathCount - 1, maxDistance));

                if (path.Count > 0)
                {
                    stopLooking = true;

                    shortestPathCount = path.Count;
                    shortestPathPoints = path.ToList();

                    Console.Error.WriteLine($"Path found at {string.Join(",", shortestPathPoints)}");
                }

                if (stopLooking == false && triedSomething == true)
                {
                    Console.Error.WriteLine($"No targets found but we tried. Don't bother trying again");
                    // We tried a closer one and couldn't get to it. For now, don't try more
                    stopLooking = true;
                }

                maxDistance += 5;
            }

            Console.Error.WriteLine(string.Join(";", shortestPathPoints.Select(p => $"{p.X},{p.Y}")));

            if (shortestPathPoints.Count == 0)
            {
                // TODO: Don't just stay there. See if there are any valid moves
                var direction = GetValidDirection(snakeBot);

                actions.Add($"{snakeBot.Id} {direction}");
            }
            else
            {
                string direction = DirectionHelper.GetDirection(snakeBot.Body[0], shortestPathPoints[0]);

                actions.Add($"{snakeBot.Id} {direction}");
                snakeBot.AddMove(shortestPathPoints[0]); 
            }
        }

        return actions;
    }

    private object GetValidDirection(SnakeBot snakeBot)
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
                && !IsSnakePart(newHeadPosition, countTails: false, null))
            {
                return direction;
            }
        }
        // No valid moves, just stay there and hope for the best
        return "DOWN";
    }

    private (List<Point>, bool) GetShortestPath(SnakeBot snakeBot, int maxDistance)
    {
        bool triedSomething = false;
        int shortestPathCount = int.MaxValue;
        var shortestPathPoints = new List<Point>();

        foreach (Point powerSource in _level.PowerSources)
        {
            // Don't bother trying if it's further away than the shortest one we've found
            if (CalculationUtil.GetManhattanDistance(snakeBot.Body[0], powerSource) >= maxDistance)
            {
                continue;
            }

            Point? excludeFirst = null;
            if (snakeBot.IsStuck())
            {
                excludeFirst = snakeBot.GetLastMove();
            }

            List<Point> path = _pathFinder.GetShortestPath(snakeBot.Body.First(), powerSource, snakeBot, excludeFirst);
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

    internal bool[,] GetPlatforms()
    {
        return _level.Platforms;
    }
}