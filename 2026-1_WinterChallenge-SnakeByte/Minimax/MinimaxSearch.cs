using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

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

    internal MinimaxResult GetBestMoves(List<SnakeBot> mySnakes, List<SnakeBot> opponentSnakes, HashSet<Point> powerSources, int maxDepth)
    {
        var state = new MinimaxGameState(mySnakes, opponentSnakes, powerSources);

        int baselineScore = Evaluate(state);

        var myMoveCombinations = GenerateAllMoveCombinations(state.MySnakes, state);

        if (myMoveCombinations.Count == 0)
        {
            return new MinimaxResult(new Dictionary<int, Point>(), 0);
        }

        var scoredMoves = myMoveCombinations.Select(m => (Score: 0, Moves: m)).ToList();

        Dictionary<int, Point>? bestMoves = null;

        int bestScore = int.MinValue;
        long startTime = System.Diagnostics.Stopwatch.GetTimestamp();

        for (int depth = 0; depth <= maxDepth; depth++)
        {
            int depthBestScore = int.MinValue;

            Dictionary<int, Point>? depthBestMoves = null;

            int alpha = int.MinValue + 1;
            int beta = int.MaxValue - 1;

            for (int i = 0; i < scoredMoves.Count; i++)
            {
                int score = MinimaxMin(state, scoredMoves[i].Moves, depth, alpha, beta);
                scoredMoves[i] = (score, scoredMoves[i].Moves);

                if (score > depthBestScore)
                {
                    depthBestScore = score;
                    depthBestMoves = scoredMoves[i].Moves;
                }

                alpha = Math.Max(alpha, bestScore);
            }

            bestScore = depthBestScore;
            bestMoves = depthBestMoves;

            scoredMoves.Sort((a, b) => b.Score.CompareTo(a.Score));

            Logger.Message($"Depth {depth}");
            // Log all move combinations and scores at this depth
            //Logger.MinimaxScores($"Depth {depth}", scoredMoves, baselineScore);            

            Logger.LogTime($"Completed depth {depth} with best score {bestScore} (relative {bestScore - baselineScore})");
        }

        int relativeScore = bestScore - baselineScore;
        return new MinimaxResult(bestMoves ?? new Dictionary<int, Point>(), relativeScore);
    }

    private int MinimaxMin(MinimaxGameState state, Dictionary<int, Point> myMoves, int depth, int alpha, int beta)
    {
        var oppMoveCombinations = GenerateAllMoveCombinations(state.OpponentSnakes, state);

        if (oppMoveCombinations.Count == 0)
        {
            UndoMove undo = ApplyMoves(state, myMoves, new Dictionary<int, Point>());

            int score = depth <= 1 ? Evaluate(state) : MinimaxMax(state, depth - 1, alpha, beta);

            UndoMoves(state, undo);

            return score;
        }

        int worstScore = int.MaxValue;

        foreach (var oppMoves in oppMoveCombinations)
        {
            UndoMove undo = ApplyMoves(state, myMoves, oppMoves);

            int score = depth <= 1 ? Evaluate(state) : MinimaxMax(state, depth - 1, alpha, beta);

            UndoMoves(state, undo);

            if (score < worstScore)
            {
                worstScore = score;
            }

            beta = Math.Min(beta, worstScore);

            if (beta <= alpha)
            {
                break;
            }
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

    private UndoMove ApplyMoves(MinimaxGameState state, Dictionary<int, Point> myMoves, Dictionary<int, Point> oppMoves)
    {
        var undo = new UndoMove
        {
            SnakeBodies = new List<(int, List<Point>)>(),
            EatenPowerUps = new HashSet<Point>()
        };

        // Save snake bodies for undo
        foreach (var s in state.MySnakes)
        {
            undo.SnakeBodies.Add((s.Id, s.Body.ToList()));
        }

        foreach (var s in state.OpponentSnakes)
        {
            undo.SnakeBodies.Add((s.Id, s.Body.ToList()));
        }

        var originalPowerSources = new HashSet<Point>(state.PowerSources);

        foreach (var snake in state.MySnakes)
        {
            if (myMoves.TryGetValue(snake.Id, out Point h))
            {
                MoveSnake(snake, h, state.PowerSources, undo.EatenPowerUps);
            }
        }

        foreach (var snake in state.OpponentSnakes)
        {
            if (oppMoves.TryGetValue(snake.Id, out Point h))
            {
                MoveSnake(snake, h, state.PowerSources, undo.EatenPowerUps);
            }
        }

        foreach (var eaten in undo.EatenPowerUps)
        {
            state.PowerSources.Remove(eaten);
        }

        HandleCollisions(state, originalPowerSources);
        ApplyGravityToAll(state);
        RemoveOutOfBoundsSnakes(state);

        return undo;
    }

    private void UndoMoves(MinimaxGameState state, UndoMove undo)
    {
        // Restore power sources
        foreach (var ps in undo.EatenPowerUps)
        {
            state.PowerSources.Add(ps);
        }

        // Restore all snake bodies
        foreach (var (id, oldBody) in undo.SnakeBodies)
        {
            var snake = state.MySnakes.FirstOrDefault(s => s.Id == id)
                        ?? state.OpponentSnakes.First(s => s.Id == id);
                  
            snake.Body = oldBody;
        }
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
        // Collision types
        // Any snakes head collides with any other snakes head.
        // Any snake head collides with any other snake body (including its own).
        HashSet<int> collidingSnakeIds = new HashSet<int>();

        foreach (var snake in state.MySnakes)
        {
            if (snake.Body.Count == 0)
            {
                continue;
            }

            // MoveSnake already updated the head position and handled power-up eating and growing,
            // so we don't need to do that here

            if (CollidesWithOtherSnake(state, snake))
            {
                collidingSnakeIds.Add(snake.Id);                
            }
        }

        foreach (var snake in state.OpponentSnakes)
        {
            if (snake.Body.Count == 0)
            {
                continue;
            }
            if (CollidesWithOtherSnake(state, snake))
            {
                collidingSnakeIds.Add(snake.Id);                
            }
        }

        foreach (var collidingSnake in collidingSnakeIds)
        {
            var mySnake = state.MySnakes.FirstOrDefault(s => s.Id == collidingSnake);
            if (mySnake != null)
            {
                if (mySnake.Body.Count <= 3)
                {
                    mySnake.Body.Clear();
                }
                else
                {
                    RemoveSegments(mySnake, 1);
                }
            }

            var oppSnake = state.OpponentSnakes.FirstOrDefault(s => s.Id == collidingSnake);
            if (oppSnake != null)
            {
                if (oppSnake.Body.Count <= 3)
                {
                    oppSnake.Body.Clear();
                }
                else
                {
                    RemoveSegments(oppSnake, 1);
                }
            }
        }
    }

    private bool CollidesWithOtherSnake(MinimaxGameState state, MinimaxSnake snake)
    {
        var allSnakes = new List<MinimaxSnake>(state.MySnakes.Count + state.OpponentSnakes.Count);
        allSnakes.AddRange(state.MySnakes);
        allSnakes.AddRange(state.OpponentSnakes);

        foreach (var other in allSnakes)
        {
            if (other.Id == snake.Id || other.Body.Count == 0)
            {
                continue;
            }

            if (snake.Body[0] == other.Body[0])
            {
                return true;
            }

            for (int i = 0; i < other.Body.Count; i++)
            {
                if (snake.Body[0] == other.Body[i])
                {
                    return true;
                }
            }
        }

        return false;
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

        var gravityPoints = new HashSet<Point>(_platformPoints);

        foreach (var ps in state.PowerSources)
        {
            gravityPoints.Add(ps);
        }
        
        foreach (MinimaxSnake snake in allSnakes)
        {
            if (snake.Body.Count == 0)
            {
                continue;
            }

            foreach (Point bodyPoint in snake.Body)
            {
                gravityPoints.Add(bodyPoint);
            }
        }

        foreach (var snake in allSnakes)
        {
            if (snake.Body.Count == 0)
            {
                continue;
            }

            // Remove the snake's own body points from the gravity points
            foreach (var bodyPoint in snake.Body)
            {
                gravityPoints.Remove(bodyPoint);
            }

            ApplyGravity(snake, gravityPoints);

            // Add the snake's body points back to the gravity points for the next snake
            foreach (var bodyPoint in snake.Body)
            {
                gravityPoints.Add(bodyPoint);
            }
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

        // TODO: Add score to encourage attacking enemy snake heads when it benefits mine
        // i.e. my snake is longer than the opponent or it's on a power source

        // Add score for closeness to power sources
        if (state.PowerSources.Count > 0)
        {
            foreach (var snake in state.MySnakes)
            {
                if (snake.Body.Count == 0)
                {
                    continue;
                }

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
