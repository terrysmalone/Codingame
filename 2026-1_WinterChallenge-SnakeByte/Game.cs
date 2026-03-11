using System.Drawing;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Xml.Linq;

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
            Console.Error.WriteLine($"_attemptsAtPowerSources: {string.Join(";", snakeBot.GetAttemptsAtPowerSource().Select(kvp => $"{kvp.Key.X},{kvp.Key.Y}:{kvp.Value}"))}");
            int shortestPathCount = int.MaxValue;
            var shortestPathPoints = new List<Point>();

            // Use an iterative deepening approach to finding targets
            bool stopLooking = false;
            int maxDistance = 5;

            while (stopLooking == false)
            {
                Console.Error.WriteLine($"maxDistance: {maxDistance}");
                (List<Point> path, bool triedSomething) = GetShortestPath(snakeBot, Math.Min(shortestPathCount-1, maxDistance));

                if (path.Count > 0)
                {
                    Console.Error.WriteLine($"Found a path of length {path.Count} to a power source");
                    stopLooking = true;

                    shortestPathCount = path.Count;
                    shortestPathPoints = path.ToList();

                    // DESPERATE FIX: IF we find any path just use it. Hopefully temporary once pathfinder is more efficient
                    // stopLooking = true;
                }

                

                if (stopLooking == false && triedSomething == true)
                {
                    // We tried a closer one and couldn't get to it. For now, don't try more
                    stopLooking = true;
                }                

                maxDistance += 5;
                if (maxDistance > 10)
                {
                    stopLooking = true;
                }
            }

            Console.Error.WriteLine(string.Join(";", shortestPathPoints.Select(p => $"{p.X},{p.Y}")));

            if (shortestPathPoints.Count == 0)
            {
                // We didn't find a shortest path
                // Move towards powersources



                string direction = GetValidDirection(snakeBot);

                actions.Add($"{snakeBot.Id} {direction} ANY MOVE");
                snakeBot.AddMove(DirectionHelper.GetNewPosition(snakeBot.Body[0], direction));
                _movesThisTurn.Add(DirectionHelper.GetNewPosition(snakeBot.Body[0], direction));
            }
            else
            {
                Console.Error.WriteLine($"Path found to {shortestPathPoints[shortestPathPoints.Count-1].X},{shortestPathPoints[shortestPathPoints.Count - 1].Y}, moving towards it");
                string direction = DirectionHelper.GetDirection(snakeBot.Body[0], shortestPathPoints[0]);

                actions.Add($"{snakeBot.Id} {direction} CHASING POWER");
                snakeBot.AddMove(shortestPathPoints[0]);
                _movesThisTurn.Add(shortestPathPoints[0]);
            }
        }

        return actions;
    }

    private string GetValidDirection(SnakeBot snakeBot)
    {

        var possibleDirections = new List<string>();

        // Prioritise moving towards the nearest powersource
        Point nearestPowerSource = GetNearestPowerSource(snakeBot);

        if (nearestPowerSource.X > snakeBot.Body[0].X)
        {
            possibleDirections = new List<string>() { "RIGHT", "UP", "DOWN", "LEFT" };

        }
        else
        {
            possibleDirections = new List<string>() { "LEFT", "UP", "DOWN", "RIGHT" };
        }

        // First, remove the hard no's
        for (int i=possibleDirections.Count - 1; i >= 0; i--)
        {
            Point newHeadPosition = DirectionHelper.GetNewPosition(snakeBot.Body[0], possibleDirections[i]);

            if(newHeadPosition.X < -1
                || newHeadPosition.X >= Width
                || newHeadPosition.Y < -1
                || newHeadPosition.Y >= Height
                || IsPlatform(newHeadPosition)
                || IsSnakePart(newHeadPosition, countTails: false, null))
            {
                possibleDirections.Remove(possibleDirections[i]);
            }
        }

        if (possibleDirections.Count == 0)
        {
            // No valid moves, just stay there and hope for the best
            return "LEFT";
        }

        // Store the first just in case we need it
        var bestSoFar = possibleDirections[0];

        if (possibleDirections.Count > 1)
        {

            for (int i = possibleDirections.Count - 1; i >= 0; i--)
            {
                Point newHeadPosition = DirectionHelper.GetNewPosition(snakeBot.Body[0], possibleDirections[i]);

                if( IsSnakePart(newHeadPosition, countTails: true, null))
                { 
                    possibleDirections.Remove(possibleDirections[i]);
                }
            }
        }

        if (possibleDirections.Count == 0)
        {
            // No valid moves, just stay there and hope for the best
            return bestSoFar;
        }

        if (possibleDirections.Count == 1)
        {
            return possibleDirections[0];
        }



        // Exclude in priority order until we only have one left
        if (possibleDirections.Count > 1)
        {            

            for (int i = possibleDirections.Count - 1; i >= 0; i--)
            {
                Point newHeadPosition = DirectionHelper.GetNewPosition(snakeBot.Body[0], possibleDirections[i]);
                if (IsBlocking(newHeadPosition, snakeBot))
                {
                    possibleDirections.Remove(possibleDirections[i]);
                }               
            }
        }

        if (possibleDirections.Count == 0)
        {
            // No valid moves, just stay there and hope for the best
            return bestSoFar;
        }

        if (possibleDirections.Count == 1)
        {
            return possibleDirections[0];
        }

        if (possibleDirections.Count > 1)
        {            

            for (int i = possibleDirections.Count - 1; i >= 0; i--)
            {
                Point newHeadPosition = DirectionHelper.GetNewPosition(snakeBot.Body[0], possibleDirections[i]);
                if (IsInHeadDanger(newHeadPosition, snakeBot))
                {
                    possibleDirections.Remove(possibleDirections[i]);
                }
            }
        }

        if (possibleDirections.Count == 0)
        {
            // No valid moves, just stay there and hope for the best
            return bestSoFar;
        }

        if (possibleDirections.Count == 1)
        {
            return possibleDirections[0];
        }

        if (possibleDirections.Count > 1)
        {            
            for (int i = possibleDirections.Count - 1; i >= 0; i--)
            {
                Point newHeadPosition = DirectionHelper.GetNewPosition(snakeBot.Body[0], possibleDirections[i]);

                if (IsStuckMove(newHeadPosition, snakeBot) || _movesThisTurn.Contains(newHeadPosition))
                {
                    possibleDirections.Remove(possibleDirections[i]);
                }
            }
        }

        if (possibleDirections.Count == 0)
        {
            // No valid moves, just stay there and hope for the best
            return bestSoFar;
        }

        return possibleDirections[0];
    }

    private bool IsInHeadDanger(Point newHeadPosition, SnakeBot snakeBot)
    {
        // If an enemy snake's head could move to the new head position on their next turn remove it if their body is equal to or bigger than ours
        foreach (var opponentSnake in OpponentSnakeBots)
        {
            if (opponentSnake.Body.Count >= snakeBot.Body.Count)
            {
                var possibleHeadMoves = new List<Point>()
                {
                    new Point(opponentSnake.Body[0].X + 1, opponentSnake.Body[0].Y),
                    new Point(opponentSnake.Body[0].X - 1, opponentSnake.Body[0].Y),
                    new Point(opponentSnake.Body[0].X, opponentSnake.Body[0].Y + 1),
                    new Point(opponentSnake.Body[0].X, opponentSnake.Body[0].Y - 1)
                };

                if (possibleHeadMoves.Contains(newHeadPosition))
                {
                    Console.Error.WriteLine($"Move to {newHeadPosition.X},{newHeadPosition.Y} is in danger of being eaten by snake {opponentSnake.Id}");
                    return true;
                }
            }
        }

        return false;
    }

    private Point GetNearestPowerSource(SnakeBot snakeBot)
    {
        int nearestDistance = int.MaxValue;
        Point nearestPowerSource = new Point(-1, -1);

        foreach (var powerSource in _level.PowerSources)
        {
            int distance = CalculationUtil.GetManhattanDistance(snakeBot.Body[0], powerSource);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestPowerSource = powerSource;
            }
        }

        return nearestPowerSource;
    }

    private bool IsStuckMove(Point newHeadPosition, SnakeBot snakeBot)
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

            if(visited.Count > snakeBot.Body.Count)
            {
                return false;
            }
        }

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
        int shortestManhattanDistanceCount = int.MaxValue;
        var shortestPathPoints = new List<Point>();

        List<Point> excludePoints = new List<Point>();
        if (snakeBot.IsStuck())
        {
            excludePoints.Add(snakeBot.GetLastMove());
        }

        // If any surrounding points are in danger of the enemy attacking them add to exclude points
        var possibleMoves = new List<Point>()
        {
            new Point(snakeBot.Body[0].X + 1, snakeBot.Body[0].Y),
            new Point(snakeBot.Body[0].X - 1, snakeBot.Body[0].Y),
            new Point(snakeBot.Body[0].X, snakeBot.Body[0].Y + 1),
            new Point(snakeBot.Body[0].X, snakeBot.Body[0].Y - 1)
        };

       
        foreach (var possibleMove in possibleMoves)
        {
            // exclude a move if it seems to be in danger of being attacked by an enemy snake on their next turn
            if (IsInHeadDanger(possibleMove, snakeBot))
            {
                excludePoints.Add(possibleMove);
            }
            // exclude a move if it seems immediately blocking
            else if (IsBlocking(possibleMove, snakeBot))
            {
                excludePoints.Add(possibleMove);
            }
        }


        foreach (Point powerSource in _level.PowerSources)
        {
            // Don't bother trying if it's further away than the shortest one we've found
            int manhattanDistance = CalculationUtil.GetManhattanDistance(snakeBot.Body[0], powerSource);
            if (manhattanDistance >= maxDistance 
                || manhattanDistance >= shortestPathCount)
                //|| manhattanDistance >= shortestManhattanDistanceCount)
            {
                continue;
            }

            if (snakeBot.GetAttemptsAtPowerSource(powerSource) > 20)
            {
                snakeBot.ClearAttemptsAtPowerSource(powerSource);
                continue;                
            }

            Console.Error.WriteLine($"Checking path to power source at {powerSource.X},{powerSource.Y}");
            Console.Error.WriteLine($"Current shortest Path: {shortestPathCount}");
            Console.Error.WriteLine($"Current shortest Manhattan Distance: {shortestManhattanDistanceCount}");

            snakeBot.AddAttemptAtPowerSource(powerSource);
             
            List<Point> path = _pathFinder.GetShortestPath(snakeBot.Body.First(), powerSource, snakeBot, excludePoints.Concat(_movesThisTurn).ToList());
            triedSomething = true;

            if (path != null && path.Count > 0 && path.Count < shortestPathCount)
            {
                shortestManhattanDistanceCount = manhattanDistance;
                shortestPathCount = path.Count;
                shortestPathPoints = path.ToList();
            }
        }

        return (shortestPathPoints, triedSomething);
    }

    internal bool IsPlatform(Point pointToCheck)
    {
        if (IsOutOfBounds(pointToCheck))
        {
            return false;
        }

        if (_level.IsPlatform(pointToCheck))
        {
            return true;
        }

        return false;
    }

    private bool IsOutOfBounds(Point pointToCheck)
    {
        if (pointToCheck.X < 0 || pointToCheck.X >= Width || pointToCheck.Y < 0 || pointToCheck.Y >= Height)
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
