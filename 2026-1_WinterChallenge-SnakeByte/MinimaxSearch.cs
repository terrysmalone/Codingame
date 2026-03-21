using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace _2026_1_WinterChallenge_SnakeByte;

internal sealed class MinimaxSearch
{
    private readonly int _width;
    private readonly int _height;
    private readonly HashSet<Point> _platformPoints;

    private static readonly Point[] MoveOffsets =
    {
        new(1, 0),
        new(-1, 0),
        new(0, -1),
        new(0, 1)
    };

    internal MinimaxSearch(int width, int height, HashSet<Point> platformPoints)
    {
        _width = width;
        _height = height;
        _platformPoints = platformPoints;
    }

    internal MinimaxResult GetBestMoves(
        List<SnakeBot> mySnakes,
        List<SnakeBot> opponentSnakes,
        HashSet<Point> powerSources,
        int maxDepth)
    {
        var state = new MinimaxGameState(mySnakes, opponentSnakes, powerSources);

        int baselineScore = Evaluate(state);

        int bestScore = int.MinValue;
        Dictionary<int, Point>? bestMyMoves = null;

        var myMoveCombinations = GenerateAllMoveCombinations(state.MySnakes, state);

        if (myMoveCombinations.Count == 0)
        {
            return new MinimaxResult(new Dictionary<int, Point>(), 0);
        }

        int alpha = int.MinValue + 1;
        int beta = int.MaxValue - 1;

        foreach (var myMoves in myMoveCombinations)
        {
            int score = MinimaxMin(state, myMoves, maxDepth, alpha, beta);

            if (score > bestScore)
            {
                bestScore = score;
                bestMyMoves = myMoves;
            }

            alpha = Math.Max(alpha, bestScore);
        }

        int relativeScore = bestScore - baselineScore;

        return new MinimaxResult(bestMyMoves ?? new Dictionary<int, Point>(), relativeScore);
    }

    private int MinimaxMin(MinimaxGameState state, Dictionary<int, Point> myMoves, int depth, int alpha, int beta)
    {
        var oppMoveCombinations = GenerateAllMoveCombinations(state.OpponentSnakes, state);

        if (oppMoveCombinations.Count == 0)
        {
            var nextState = ApplyMoves(state, myMoves, new Dictionary<int, Point>());

            if (depth <= 1)
            {
                return Evaluate(nextState);
            }

            return MinimaxMax(nextState, depth - 1, alpha, beta);
        }

        int worstScore = int.MaxValue;

        foreach (var oppMoves in oppMoveCombinations)
        {
            var nextState = ApplyMoves(state, myMoves, oppMoves);

            int score;
            if (depth <= 1)
            {
                score = Evaluate(nextState);
            }
            else
            {
                score = MinimaxMax(nextState, depth - 1, alpha, beta);
            }

            if (score < worstScore)
                worstScore = score;

            beta = Math.Min(beta, worstScore);
            if (beta <= alpha)
                break;
        }

        return worstScore;
    }

    private int MinimaxMax(MinimaxGameState state, int depth, int alpha, int beta)
    {
        var myMoveCombinations = GenerateAllMoveCombinations(state.MySnakes, state);

        if (myMoveCombinations.Count == 0)
            return Evaluate(state);

        int bestScore = int.MinValue;

        foreach (var myMoves in myMoveCombinations)
        {
            int score = MinimaxMin(state, myMoves, depth, alpha, beta);

            if (score > bestScore)
                bestScore = score;

            alpha = Math.Max(alpha, bestScore);
            if (beta <= alpha)
                break;
        }

        return bestScore;
    }

    private List<Dictionary<int, Point>> GenerateAllMoveCombinations(List<MinimaxSnake> snakes, MinimaxGameState state)
    {
        var movesPerSnake = new List<(int Id, List<Point> Moves)>();

        foreach (var snake in snakes)
        {
            if (snake.Body.Count == 0) continue;

            var validMoves = GetValidMoves(snake, state);
            if (validMoves.Count > 0)
                movesPerSnake.Add((snake.Id, validMoves));
        }

        if (movesPerSnake.Count == 0)
            return new List<Dictionary<int, Point>>();

        var combinations = new List<Dictionary<int, Point>>();
        BuildCombinations(movesPerSnake, 0, new Dictionary<int, Point>(), combinations);

        // Remove combinations where two snakes on the same team try to occupy the same cell
        if (movesPerSnake.Count > 1)
        {
            combinations.RemoveAll(combo =>
            {
                var positions = combo.Values;
                var seen = new HashSet<Point>();
                foreach (var p in positions)
                {
                    if (!seen.Add(p)) return true;
                }
                return false;
            });
        }

        return combinations;
    }

    private void BuildCombinations(
        List<(int Id, List<Point> Moves)> movesPerSnake,
        int index,
        Dictionary<int, Point> current,
        List<Dictionary<int, Point>> results)
    {
        if (index == movesPerSnake.Count)
        {
            results.Add(new Dictionary<int, Point>(current));
            return;
        }

        var (id, moves) = movesPerSnake[index];
        foreach (var move in moves)
        {
            current[id] = move;
            BuildCombinations(movesPerSnake, index + 1, current, results);
        }
        current.Remove(id);
    }

    private List<Point> GetValidMoves(MinimaxSnake snake, MinimaxGameState state)
    {
        var validMoves = new List<Point>(4);
        Point head = snake.Body[0];

        foreach (var offset in MoveOffsets)
        {
            Point newHead = new Point(head.X + offset.X, head.Y + offset.Y);

            if (newHead.X < -1 || newHead.X > _width || newHead.Y < -1 || newHead.Y > _height)
                continue;

            if (IsInMapBounds(newHead) && _platformPoints.Contains(newHead))
                continue;

            if (IsBlockedBySnake(newHead, snake.Id, state))
                continue;

            if (IsSelfCollision(newHead, snake.Body))
                continue;

            validMoves.Add(newHead);
        }

        return validMoves;
    }

    private bool IsInMapBounds(Point p)
    {
        return p.X >= 0 && p.X < _width && p.Y >= 0 && p.Y < _height;
    }

    private bool IsBlockedBySnake(Point newHead, int excludeId, MinimaxGameState state)
    {
        foreach (var snake in state.MySnakes)
        {
            if (snake.Id == excludeId || snake.Body.Count == 0) continue;
            for (int i = 0; i < snake.Body.Count - 1; i++)
            {
                if (snake.Body[i] == newHead) return true;
            }
        }

        foreach (var snake in state.OpponentSnakes)
        {
            if (snake.Id == excludeId || snake.Body.Count == 0) continue;
            for (int i = 0; i < snake.Body.Count - 1; i++)
            {
                if (snake.Body[i] == newHead) return true;
            }
        }

        return false;
    }

    private bool IsSelfCollision(Point newHead, List<Point> body)
    {
        for (int i = 1; i < body.Count - 1; i++)
        {
            if (body[i] == newHead) return true;
        }
        return false;
    }

    private MinimaxGameState ApplyMoves(MinimaxGameState state,
        Dictionary<int, Point> myMoves, Dictionary<int, Point> oppMoves)
    {
        var newState = state.Clone();
        var originalPowerSources = new HashSet<Point>(state.PowerSources);
        var eatenPowerUps = new HashSet<Point>();

        foreach (var snake in newState.MySnakes)
        {
            if (myMoves.TryGetValue(snake.Id, out Point newHead))
            {
                MoveSnake(snake, newHead, newState.PowerSources, eatenPowerUps);
            }
        }

        foreach (var snake in newState.OpponentSnakes)
        {
            if (oppMoves.TryGetValue(snake.Id, out Point newHead))
            {
                MoveSnake(snake, newHead, newState.PowerSources, eatenPowerUps);
            }
        }

        foreach (var eaten in eatenPowerUps)
        { 
            newState.PowerSources.Remove(eaten);
        }

        HandleCollisions(newState, originalPowerSources);
        ApplyGravityToAll(newState);
        RemoveOutOfBoundsSnakes(newState);

        return newState;
    }

    private void MoveSnake(MinimaxSnake snake, Point newHead,
        HashSet<Point> powerSources, HashSet<Point> eatenPowerUps)
    {
        if (snake.Body.Count == 0) return;

        snake.Body.Insert(0, newHead);

        if (powerSources.Contains(newHead))
        {
            eatenPowerUps.Add(newHead);
        }
        else
        {
            snake.Body.RemoveAt(snake.Body.Count - 1);
        }
    }

    private void HandleCollisions(MinimaxGameState state, HashSet<Point> originalPowerSources)
    {
        foreach (var mySnake in state.MySnakes)
        {
            if (mySnake.Body.Count == 0)
            {
                continue;
            }

            foreach (var oppSnake in state.OpponentSnakes)
            {
                if (oppSnake.Body.Count == 0) continue;

                if (mySnake.Body[0] == oppSnake.Body[0])
                {
                    bool powerUpOnSpot = originalPowerSources.Contains(mySnake.Body[0]);

                    int myLoss;
                    int oppLoss;

                    if (powerUpOnSpot)
                    {
                        myLoss = 1;
                        oppLoss = 1;
                    }
                    else
                    {
                        myLoss = mySnake.Body.Count <= 3 ? mySnake.Body.Count : 1;
                        oppLoss = oppSnake.Body.Count <= 3 ? oppSnake.Body.Count : 1;
                    }

                    RemoveSegments(mySnake, myLoss);
                    RemoveSegments(oppSnake, oppLoss);

                    if (mySnake.Body.Count == 0) break;
                }
            }
        }

        var allSnakes = new List<MinimaxSnake>(state.MySnakes.Count + state.OpponentSnakes.Count);
        allSnakes.AddRange(state.MySnakes);
        allSnakes.AddRange(state.OpponentSnakes);

        foreach (var snake in allSnakes)
        {
            if (snake.Body.Count == 0) continue;

            foreach (var other in allSnakes)
            {
                if (other.Id == snake.Id || other.Body.Count == 0) continue;

                for (int i = 1; i < other.Body.Count; i++)
                {
                    if (snake.Body[0] == other.Body[i])
                    {
                        int loss = snake.Body.Count <= 3 ? snake.Body.Count : 1;
                        RemoveSegments(snake, loss);
                        break;
                    }
                }

                if (snake.Body.Count == 0) break;
            }
        }
    }

    private void RemoveSegments(MinimaxSnake snake, int count)
    {
        for (int i = 0; i < count && snake.Body.Count > 0; i++)
            snake.Body.RemoveAt(snake.Body.Count - 1);
    }

    private void ApplyGravityToAll(MinimaxGameState state)
    {
        var allSnakes = new List<MinimaxSnake>(state.MySnakes.Count + state.OpponentSnakes.Count);
        allSnakes.AddRange(state.MySnakes);
        allSnakes.AddRange(state.OpponentSnakes);

        foreach (var snake in allSnakes)
        {
            if (snake.Body.Count == 0) continue;

            var gravityPoints = new HashSet<Point>(_platformPoints);

            foreach (var other in allSnakes)
            {
                if (other.Id == snake.Id || other.Body.Count == 0) continue;
                foreach (var bp in other.Body)
                    gravityPoints.Add(bp);
            }

            foreach (var ps in state.PowerSources)
                gravityPoints.Add(ps);

            ApplyGravity(snake, gravityPoints);
        }
    }

    private void ApplyGravity(MinimaxSnake snake, HashSet<Point> solidPoints)
    {
        for (int fall = 0; fall < 20; fall++)
        {
            bool canFall = true;
            foreach (var bp in snake.Body)
            {
                if (solidPoints.Contains(new Point(bp.X, bp.Y + 1)))
                {
                    canFall = false;
                    break;
                }
            }

            if (!canFall) break;

            for (int i = 0; i < snake.Body.Count; i++)
                snake.Body[i] = new Point(snake.Body[i].X, snake.Body[i].Y + 1);
        }
    }

    private void RemoveOutOfBoundsSnakes(MinimaxGameState state)
    {
        foreach (var snake in state.MySnakes)
        {
            if (snake.Body.Count > 0 && IsFullyOutOfBounds(snake.Body))
                snake.Body.Clear();
        }

        foreach (var snake in state.OpponentSnakes)
        {
            if (snake.Body.Count > 0 && IsFullyOutOfBounds(snake.Body))
                snake.Body.Clear();
        }
    }

    private bool IsFullyOutOfBounds(List<Point> body)
    {
        foreach (var p in body)
        {
            if (IsInMapBounds(p)) return false;
        }
        return true;
    }

    private int Evaluate(MinimaxGameState state)
    {
        int myBodyTotal = 0;
        int oppBodyTotal = 0;

        foreach (var s in state.MySnakes)
        {
            myBodyTotal += s.Body.Count;
        }

        foreach (var s in state.OpponentSnakes)
        {
            oppBodyTotal += s.Body.Count;
        }

        int score = (myBodyTotal - oppBodyTotal) * 1000;

        if (state.PowerSources.Count > 0)
        {
            foreach (var snake in state.MySnakes)
            {
                if (snake.Body.Count == 0) continue;

                int minDist = MinDistanceToPowerSource(snake.Body[0], state.PowerSources);

                if (minDist < int.MaxValue)
                {
                    score += Math.Max(0, 20 - minDist) * 10;
                }
            }
        }

        return score;
    }

    private int MinDistanceToPowerSource(Point head, HashSet<Point> powerSources)
    {
        int minDist = int.MaxValue;
        foreach (var ps in powerSources)
        {
            int dist = Math.Abs(head.X - ps.X) + Math.Abs(head.Y - ps.Y);

            if (dist < minDist)
            {
                minDist = dist; 
            }
        }
        return minDist;
    }
}

internal sealed class MinimaxGameState
{
    internal List<MinimaxSnake> MySnakes { get; set; } = new();
    internal List<MinimaxSnake> OpponentSnakes { get; set; } = new();
    internal HashSet<Point> PowerSources { get; set; } = new();

    internal MinimaxGameState(List<SnakeBot> mySnakes, List<SnakeBot> oppSnakes, HashSet<Point> powerSources)
    {
        MySnakes = mySnakes.Select(s => new MinimaxSnake(s.Id, s.Body)).ToList();
        OpponentSnakes = oppSnakes.Select(s => new MinimaxSnake(s.Id, s.Body)).ToList();
        PowerSources = new HashSet<Point>(powerSources);
    }

    private MinimaxGameState() { }

    internal MinimaxGameState Clone()
    {
        return new MinimaxGameState
        {
            MySnakes = MySnakes.Select(s => s.Clone()).ToList(),
            OpponentSnakes = OpponentSnakes.Select(s => s.Clone()).ToList(),
            PowerSources = new HashSet<Point>(PowerSources)
        };
    }
}

internal sealed class MinimaxSnake
{
    internal int Id { get; }
    internal List<Point> Body { get; set; }

    internal MinimaxSnake(int id, List<Point> body)
    {
        Id = id;
        Body = body.Select(p => new Point(p.X, p.Y)).ToList();
    }

    internal MinimaxSnake Clone()
    {
        return new MinimaxSnake(Id, Body);
    }
}

internal sealed class MinimaxResult
{
    internal Dictionary<int, Point> BestMoves { get; }
    internal int Score { get; }

    internal MinimaxResult(Dictionary<int, Point> bestMoves, int score)
    {
        BestMoves = bestMoves;
        Score = score;
    }
}