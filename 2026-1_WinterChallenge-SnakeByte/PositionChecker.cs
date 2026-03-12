using System.Drawing;

namespace _2026_1_WinterChallenge_SnakeByte;

internal sealed class PositionChecker
{
    private Game _game;
    private readonly Level _level;

    internal PositionChecker(Game game, Level level)
    {
        _game = game;
        _level = level;
    }

    internal bool IsOutOfMapBounds(Point pointToCheck)
    {
        if (pointToCheck.X < 0 || pointToCheck.X >= _game.Width || pointToCheck.Y < 0 || pointToCheck.Y >= _game.Height)
        {
            return true;
        }
        return false;
    }

    internal bool IsBlocking(Point newHeadPosition, SnakeBot snakeBot)
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
                    && adjacentPoint.X < _game.Width
                    && adjacentPoint.Y >= 0
                    && adjacentPoint.Y < _game.Height
                    && !IsPlatform(adjacentPoint)
                    && !IsPointInAnySnake(adjacentPoint, countTails: true)
                    && !visited.Contains(adjacentPoint))
                {
                    queue.Enqueue(adjacentPoint);
                    visited.Add(adjacentPoint);
                }
            }

            if (visited.Count > snakeBot.Body.Count)
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

    internal bool IsPlatform(Point pointToCheck)
    {
        if (IsOutOfMapBounds(pointToCheck))
        {
            return false;
        }

        if (_level.IsPlatform(pointToCheck))
        {
            return true;
        }

        return false;
    }

    internal bool IsPointInAnySnake(Point pointToCheck, bool countTails, int excludeSnakeId=-1)
    {
        foreach (var snakeBot in _game.MySnakeBots)
        {
            if (excludeSnakeId > 0 && snakeBot.Id == excludeSnakeId)
            {
                continue;
            }

            if (IsPointInGivenSnake(snakeBot.Body, pointToCheck, countTails))
            {
                return true;
            }
        }

        foreach (var snakeBot in _game.OpponentSnakeBots)
        {
            if (snakeBot.Body.Contains(pointToCheck))
            {
                if (IsPointInGivenSnake(snakeBot.Body, pointToCheck, countTails))
                {
                    return true;
                }
            }
        }

        return false;
    }

    internal bool IsPointInGivenSnake(List<Point> snakeBody, Point pointToCheck, bool countTails)
    {
        if (snakeBody.Contains(pointToCheck))
        {
            if (!countTails)
            {
                if (snakeBody[snakeBody.Count - 1] == pointToCheck)
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

    internal bool IsStuckMove(Point newHeadPosition, SnakeBot snakeBot)
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

}
