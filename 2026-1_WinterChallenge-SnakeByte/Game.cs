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

    internal List<SnakeBot> MySnakeBots { get; set; }
    internal List<SnakeBot> OpponentSnakeBots { get; set; }

    private Level _level;

    private PathFinder _pathFinder;
    private PositionChecker _positionChecker;
    
    private List<Point> _movesThisTurn;    

    public Game(int width, int height, bool[,] platforms)
    {
        Width = width;
        Height = height;

        _level = new Level(width, height, platforms);
                
        _positionChecker = new PositionChecker(this, _level);
        _pathFinder = new PathFinder(this, _positionChecker);

        MySnakeBots = new List<SnakeBot>();
        OpponentSnakeBots = new List<SnakeBot>();
    }

    internal void AddMySnake(SnakeBot snakeBot)
    {
        MySnakeBots.Add(snakeBot);
    }

    internal void AddOpponentSnake(SnakeBot snakeBot)
    {
        OpponentSnakeBots.Add(snakeBot);
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

    internal SnakeBot GetSnake(int snakebotId)
    {
        return MySnakeBots.FirstOrDefault(s => s.Id == snakebotId) ?? OpponentSnakeBots.FirstOrDefault(s => s.Id == snakebotId);
    }

    internal void AddPowerSource(int x, int y)
    {
        _level.PowerSources.Add(new Point(x, y));
    }

    internal void RemoveAllPowerSources()
    {
        _level.PowerSources.Clear();
    }    

    internal List<string> GetActions()
    {
        _movesThisTurn = new List<Point>();
        // Logger.EntireGame(_level.Platforms, MySnakeBots, OpponentSnakeBots, _level.PowerSources);

        List<string> actions = new List<string>();

        foreach (var snakeBot in MySnakeBots)
        {
            // TODO: CHeck for chance toi destroy an opponent snake and do that if possible

            List<Point> bestPathToPower = GetBestPathToPowerSource(snakeBot);            

            if (bestPathToPower.Count != 0)
            {
                string direction = DirectionHelper.GetDirection(snakeBot.Body[0], bestPathToPower[0]);

                actions.Add($"{snakeBot.Id} {direction} CHASING POWER");
                snakeBot.AddMove(bestPathToPower[0]);
                _movesThisTurn.Add(bestPathToPower[0]);
            }
            else
            {
                string direction = GetValidDirection(snakeBot);

                actions.Add($"{snakeBot.Id} {direction} ANY MOVE");
                snakeBot.AddMove(DirectionHelper.GetNewPosition(snakeBot.Body[0], direction));
                _movesThisTurn.Add(DirectionHelper.GetNewPosition(snakeBot.Body[0], direction));
            }
        }

        // TODO: After we've come up with moves check for clashes and try to resolve them

        return actions;
    }

    private List<Point> GetBestPathToPowerSource(SnakeBot snakeBot)
    {
        int shortestPathCount = int.MaxValue;
        var shortestPathPoints = new List<Point>();

        // Use an iterative deepening approach to finding targets
        bool stopLooking = false;
        int maxDistance = 5;

        while (stopLooking == false)
        {
            (List<Point> path, bool triedSomething) = GetShortestPath(snakeBot, Math.Min(shortestPathCount - 1, maxDistance));

            if (path.Count > 0)
            {
                stopLooking = true;

                shortestPathCount = path.Count;
                shortestPathPoints = path.ToList();
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

        return shortestPathPoints;
    }

    private string GetValidDirection(SnakeBot snakeBot)
    {
        // Prioritise moving towards the nearest powersource
        Point nearestPowerSource = GetNearestPowerSource(snakeBot);


        List<string> possibleDirections = nearestPowerSource.X > snakeBot.Body[0].X ? new List<string>() { "RIGHT", "UP", "DOWN", "LEFT" } 
                                                                                    : new List<string>() { "LEFT", "UP", "DOWN", "RIGHT" };


        // First, remove the hard no's
        RemoveAllHardNos(possibleDirections, snakeBot);
        
        if (possibleDirections.Count == 0)
        {
            // No valid moves, just stay there and hope for the best
            return "LEFT";
        }

        // Store the first just in case we need it
        var bestSoFar = possibleDirections[0];

        RemoveOtherSnakeBodyPositions(possibleDirections, snakeBot);

        string direction;

        if (!string.IsNullOrEmpty(direction = GetEarlyReturn(possibleDirections, bestSoFar)))
        {
            return direction;
        }

        RemoveBlockingDirections(possibleDirections, snakeBot);

        if (!string.IsNullOrEmpty(direction = GetEarlyReturn(possibleDirections, bestSoFar)))
        {
            return direction;
        }

        RemoveHeadDangerPositions(possibleDirections, snakeBot);

        if (!string.IsNullOrEmpty(direction = GetEarlyReturn(possibleDirections, bestSoFar)))
        {
            return direction;
        }

        RemoveStuckDirections(possibleDirections, snakeBot);

        if (possibleDirections.Count == 0)
        {
            // No valid moves, just stay there and hope for the best
            return bestSoFar;
        }

        return possibleDirections[0];
    }

    private string GetEarlyReturn(List<string> possibleDirections, string bestSoFar)
    {
        if (possibleDirections.Count == 0)
        {
            // No valid moves, just stay there and hope for the best
            return bestSoFar;
        }

        if (possibleDirections.Count == 1)
        {
            return possibleDirections[0];
        }

        return string.Empty;
    }

    private void RemoveAllHardNos(List<string> possibleDirections, SnakeBot snakeBot)
    {
        for (int i = possibleDirections.Count - 1; i >= 0; i--)
        {
            Point newHeadPosition = DirectionHelper.GetNewPosition(snakeBot.Body[0], possibleDirections[i]);

            if (newHeadPosition.X < -1
                || newHeadPosition.X >= Width
                || newHeadPosition.Y < -1
                || newHeadPosition.Y >= Height
                || _positionChecker.IsPlatform(newHeadPosition)
                || _positionChecker.IsSnakePart(newHeadPosition, countTails: false, null))
            {
                possibleDirections.Remove(possibleDirections[i]);
            }
        }
    }

    private void RemoveOtherSnakeBodyPositions(List<string> possibleDirections, SnakeBot snakeBot)
    {
        if (possibleDirections.Count > 1)
        {
            for (int i = possibleDirections.Count - 1; i >= 0; i--)
            {
                Point newHeadPosition = DirectionHelper.GetNewPosition(snakeBot.Body[0], possibleDirections[i]);

                if (_positionChecker.IsSnakePart(newHeadPosition, countTails: true, null))
                {
                    possibleDirections.Remove(possibleDirections[i]);
                }
            }
        }
    }

    private void RemoveBlockingDirections(List<string> possibleDirections, SnakeBot snakeBot)
    {
        // Exclude in priority order until we only have one left
        if (possibleDirections.Count > 1)
        {

            for (int i = possibleDirections.Count - 1; i >= 0; i--)
            {
                Point newHeadPosition = DirectionHelper.GetNewPosition(snakeBot.Body[0], possibleDirections[i]);
                if (_positionChecker.IsBlocking(newHeadPosition, snakeBot))
                {
                    possibleDirections.Remove(possibleDirections[i]);
                }
            }
        }
    }

    private void RemoveHeadDangerPositions(List<string> possibleDirections, SnakeBot snakeBot)
    {
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
    }

    private void RemoveStuckDirections(List<string> possibleDirections, SnakeBot snakeBot)
    {
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
            else if (_positionChecker.IsBlocking(possibleMove, snakeBot))
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

    internal List<Point> GetPowerUps()
    {
        return _level.PowerSources;
    }
}