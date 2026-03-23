using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

namespace _2026_1_WinterChallenge_SnakeByte;

internal sealed class MinimaxSearch
{
    private int _attackedBigger = 0;
    private int _attackedSmaller = 0;

    private int _evaluationsThisDepth = 0;
    private int _ttHits = 0;

    private long _searchStartTime;
    private double _timeLimitMs;
    private bool _searchAborted;

    private readonly TranspositionTable _transpositionTable = new TranspositionTable();

    private readonly int _width;
    private readonly int _height;
    private readonly HashSet<Point> _platformPoints;

    // Reusable buffer to avoid allocations in gravity computation
    private readonly HashSet<Point> _dynamicPointsBuffer = new();

    // Reusable buffers for flood fill in evaluation
    private readonly HashSet<Point> _floodVisited = new();
    private readonly Queue<Point> _floodQueue = new();
    private readonly HashSet<Point> _floodBlocked = new();

    private static readonly Dictionary<int, Point> EmptyMoves = new();

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

    private bool IsTimeUp() =>
        Stopwatch.GetElapsedTime(_searchStartTime).TotalMilliseconds >= _timeLimitMs;

    internal MinimaxResult GetBestMoves(List<SnakeBot> mySnakes, List<SnakeBot> opponentSnakes, HashSet<Point> powerSources, int maxDepth, double timeLimitMs)
    {
        Logger.Snakes("My snakes", mySnakes);
        // Logger.Snakes("Enemy snakes", opponentSnakes);

        _searchStartTime = Stopwatch.GetTimestamp();
        _timeLimitMs = timeLimitMs;
        _searchAborted = false;

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

        _transpositionTable.Clear();

        for (int depth = 0; depth <= maxDepth; depth++)
        {
            _evaluationsThisDepth = 0;
            _ttHits = 0;
            _searchAborted = false;

            int depthBestScore = int.MinValue;
            Dictionary<int, Point>? depthBestMoves = null;

            int alpha = int.MinValue + 1;
            int beta = int.MaxValue - 1;

            for (int i = 0; i < scoredMoves.Count; i++)
            {
                int score = MinimaxMin(state, scoredMoves[i].Moves, depth, alpha, beta);

                if (_searchAborted)
                {
                    break;
                }

                scoredMoves[i] = (score, scoredMoves[i].Moves);

                if (score > depthBestScore)
                {
                    depthBestScore = score;
                    depthBestMoves = scoredMoves[i].Moves;
                }

                alpha = Math.Max(alpha, depthBestScore);
            }

            if (_searchAborted)
            {
                Logger.LogTime($"Depth {depth} aborted due to time limit ({_evaluationsThisDepth} evals)");
                break;
            }

            bestScore = depthBestScore;
            bestMoves = depthBestMoves;

            scoredMoves.Sort((a, b) => b.Score.CompareTo(a.Score));

            Logger.Message($"Depth {depth} evaluations: {_evaluationsThisDepth}, TT hits: {_ttHits}");
            Logger.LogTime($"Completed depth {depth} with best score {bestScore} (relative {bestScore - baselineScore})");

            if (IsTimeUp())
            {
                Logger.LogTime($"Time limit reached after completing depth {depth}");
                break;
            }
        }

        int relativeScore = bestScore - baselineScore;
        return new MinimaxResult(bestMoves ?? new Dictionary<int, Point>(), relativeScore);
    }

    private int MinimaxMin(MinimaxGameState state, Dictionary<int, Point> myMoves, int depth, int alpha, int beta)
    {
        if (_searchAborted)
        {
            return 0;
        }

        var oppMoveCombinations = GenerateAllMoveCombinations(state.OpponentSnakes, state);

        if (oppMoveCombinations.Count == 0)
        {
            UndoMove undo = ApplyMoves(state, myMoves, EmptyMoves);

            int score = depth <= 1 ? Evaluate(state) : MinimaxMax(state, depth - 1, alpha, beta);

            UndoMoves(state, undo);

            return score;
        }

        // TODO: Sort moves to improve alpha-beta pruning efficiency
        // If snakes touch walls, or snake bodies (not heads, movee them to the bottom of the list since those are less likely to be chosen by the opponent
        OrderMoves(oppMoveCombinations);

        int worstScore = int.MaxValue;

        foreach (var oppMoves in oppMoveCombinations)
        {
            UndoMove undo = ApplyMoves(state, myMoves, oppMoves);

            int score = depth <= 1 ? Evaluate(state) : MinimaxMax(state, depth - 1, alpha, beta);

            UndoMoves(state, undo);

            if (_searchAborted)
            { 
                return 0;
            }

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

    private void OrderMoves(List<Dictionary<int, Point>> moveCombinations)
    {
        moveCombinations.Sort((a, b) => ScoreMoveCombination(a).CompareTo(ScoreMoveCombination(b)));
    }

    private int ScoreMoveCombination(Dictionary<int, Point> moves)
    {
        int score = 0;
        foreach (var move in moves.Values)
        {
            if (IsInMapBounds(move))
            {
                score -= 10; // Prefer moves that stay in bounds
            }
            if (_platformPoints.Contains(move))
            {
                score -= 20; // Strongly prefer moves that avoid platforms
            }
        }
        return score;
    }

    private int MinimaxMax(MinimaxGameState state, int depth, int alpha, int beta)
    {
        if (_searchAborted)
        {
            return 0;
        }

        if (IsTimeUp())
        {
            _searchAborted = true;
            return 0;
        }

        ulong hash = ComputeStateHash(state);
        int origAlpha = alpha;

        if (_transpositionTable.TryGet(hash, depth, out var ttEntry))
        {
            _ttHits++;

            if (ttEntry.Flag == TransFlag.Exact)
            {
                return ttEntry.Score;
            }
            if (ttEntry.Flag == TransFlag.LowerBound)
            {
                alpha = Math.Max(alpha, ttEntry.Score);
            }
            else if (ttEntry.Flag == TransFlag.UpperBound)
            {
                beta = Math.Min(beta, ttEntry.Score);
            }

            if (alpha >= beta)
            {
                return ttEntry.Score;
            }
        }

        var myMoveCombinations = GenerateAllMoveCombinations(state.MySnakes, state);

        if (myMoveCombinations.Count == 0)
        {
            int evalScore = Evaluate(state);
            _transpositionTable.Store(hash, evalScore, depth, TransFlag.Exact);
            return evalScore;
        }

        // TODO: Sort moves to improve alpha-beta pruning efficiency
        OrderMoves(myMoveCombinations);

        int bestScore = int.MinValue;

        foreach (var myMoves in myMoveCombinations)
        {
            int score = MinimaxMin(state, myMoves, depth, alpha, beta);

            if (_searchAborted)
            {
                return 0;
            }

            if (score > bestScore)
                bestScore = score;

            alpha = Math.Max(alpha, bestScore);
            if (beta <= alpha)
                break;
        }

        TransFlag flag;
        if (bestScore <= origAlpha)
        {
            flag = TransFlag.UpperBound;
        }
        else if (bestScore >= beta)
        {
            flag = TransFlag.LowerBound;
        }
        else
        {
            flag = TransFlag.Exact;
        }

        _transpositionTable.Store(hash, bestScore, depth, flag);

        return bestScore;
    }

    private List<Dictionary<int, Point>> GenerateAllMoveCombinations(List<MinimaxSnake> snakes, MinimaxGameState state)
    {
        var movesPerSnake = new List<(int Id, List<Point> Moves)>();

        foreach (var snake in snakes)
        {
            if (snake.Body.Count == 0)
            { 
                continue; 
            }

            var validMoves = GetValidMoves(snake, state);
            movesPerSnake.Add((snake.Id, validMoves));

        }

        if (movesPerSnake.Count == 0)
        {
            return new List<Dictionary<int, Point>>();
        }

        var combinations = new List<Dictionary<int, Point>>();
        BuildCombinations(movesPerSnake, 0, new Dictionary<int, Point>(), combinations);

        return combinations;
    }

    private void BuildCombinations(List<(int Id, List<Point> Moves)> movesPerSnake, int index, Dictionary<int, Point> current, List<Dictionary<int, Point>> results)
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
        var safeMoves = new List<Point>();
        var unsafeMoves = new List<Point>();

        Point head = snake.Body[0];

        foreach (var offset in MoveOffsets)
        {
            Point newHead = new Point(head.X + offset.X, head.Y + offset.Y);

            if (newHead.X < -1 || newHead.X > _width || newHead.Y < -1 || newHead.Y > _height)
            {
                continue;
            }

            if(newHead == snake.Body[1])
            {
                continue;
            }


            if ((IsInMapBounds(newHead) && _platformPoints.Contains(newHead))
                || IsBlockedBySnake(newHead, snake.Id, state)
                || IsSelfCollision(newHead, snake.Body))
            {
                unsafeMoves.Add(newHead);
            }
            else
            {
                safeMoves.Add(newHead);
            }
        }

        return safeMoves.Count > 0 ? safeMoves : unsafeMoves;
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
        _attackedBigger = 0;
        _attackedSmaller = 0;

        int myCount = state.MySnakes.Count;
        int oppCount = state.OpponentSnakes.Count;
        var changes = new SnakeChange[myCount + oppCount];

        var eatenPowerUps = new HashSet<Point>();               

        for (int i = 0; i < myCount; i++)
        {
            var snake = state.MySnakes[i];
            changes[i].Id = snake.Id;

            if (myMoves.TryGetValue(snake.Id, out Point move))
            {
                ApplySnakeChange(snake, move, state.PowerSources, eatenPowerUps, ref changes[i]);
            }
        }

        for (int i = 0; i < oppCount; i++)
        {
            var snake = state.OpponentSnakes[i];
            changes[myCount + i].Id = snake.Id;

            if (oppMoves.TryGetValue(snake.Id, out Point move))
            {
                ApplySnakeChange(snake, move, state.PowerSources, eatenPowerUps, ref changes[myCount + i]);
            }
        }

        foreach (var eaten in eatenPowerUps)
        {
            state.PowerSources.Remove(eaten);
        }

        HandleCollisionsChange(state, changes);
        ApplyGravityChange(state, changes);
        RemoveOutOfBoundsChange(state, changes);

        return new UndoMove { Changes = changes, EatenPowerUps = eatenPowerUps };
    }

    private void ApplySnakeChange(MinimaxSnake snake, Point newHead, HashSet<Point> powerSources, HashSet<Point> eatenPowerUps, ref SnakeChange delta)
    {
        if (snake.Body.Count == 0)
        {
            return;
        }

        delta.Moved = true;
        snake.Body.Insert(0, newHead);

        if (powerSources.Contains(newHead))
        {
            eatenPowerUps.Add(newHead);
            delta.Grew = true;
        }
        else
        {
            delta.RemovedTail = snake.Body[snake.Body.Count - 1];
            snake.Body.RemoveAt(snake.Body.Count - 1);
        }
    }

    private void UndoMoves(MinimaxGameState state, UndoMove undo)
    {
        // Restore power sources
        foreach (var ps in undo.EatenPowerUps)
        {
            state.PowerSources.Add(ps);
        }

        for (int s = 0; s < state.MySnakes.Count; s++)
            UndoSnakeChange(state.MySnakes[s], undo.Changes);

        for (int s = 0; s < state.OpponentSnakes.Count; s++)
            UndoSnakeChange(state.OpponentSnakes[s], undo.Changes);
    }

    private static void UndoSnakeChange(MinimaxSnake snake, SnakeChange[] changes)
    {
        int index = FindChangeIndex(changes, snake.Id);
        if (index < 0) return;

        ref var change = ref changes[index];

        if (change.Killed)
        {
            snake.Body = change.KilledBody!;
        }

        // Undo gravity
        if (change.GravityFall > 0)
        {
            for (int i = 0; i < snake.Body.Count; i++)
            {
                snake.Body[i] = new Point(snake.Body[i].X, snake.Body[i].Y - change.GravityFall);
            }
        }

        // Undo collision loss
        if (change.LostCollisionSegment)
        {
            snake.Body.Insert(0, change.CollisionHead);
        }

        // Undo move
        if (change.Moved)
        {
            snake.Body.RemoveAt(0);
            if (!change.Grew)
            {
                snake.Body.Add(change.RemovedTail);
            }
        }
    }

    private static int FindChangeIndex(SnakeChange[] changes, int id)
    {
        for (int i = 0; i < changes.Length; i++)
        {
            if (changes[i].Id == id)
            {
                return i;
            }
        }
        
        return -1;        
    }

    private static MinimaxSnake FindSnake(MinimaxGameState state, int id)
    {
        foreach (var s in state.MySnakes)
        {
            if (s.Id == id)
            {
                return s;
            }
        }

        foreach (var s in state.OpponentSnakes)
        {
            if (s.Id == id)
            {
                return s;
            }
        }

        Console.Error.WriteLine($"ERROR: Snake {id} not found");
        throw new InvalidOperationException($"Snake {id} not found");
    }

    private void HandleCollisionsChange(MinimaxGameState state, SnakeChange[] changes)
    {
        int totalSnakes = state.MySnakes.Count + state.OpponentSnakes.Count;
        Span<int> collidingIds = stackalloc int[totalSnakes];
        int collidingCount = 0;

        for (int i = 0; i < state.MySnakes.Count; i++)
        {
            if (state.MySnakes[i].Body.Count > 0 
                && (CollidesWithOtherSnake(state, state.MySnakes[i]) || _platformPoints.Contains(state.MySnakes[i].Body[0])))
            {
                collidingIds[collidingCount++] = state.MySnakes[i].Id;
            }
        }

        for (int i = 0; i < state.OpponentSnakes.Count; i++)
        {
            if (state.OpponentSnakes[i].Body.Count > 0 
                && (CollidesWithOtherSnake(state, state.OpponentSnakes[i]) || _platformPoints.Contains(state.OpponentSnakes[i].Body[0])))
            {
                collidingIds[collidingCount++] = state.OpponentSnakes[i].Id;
            }
        }

        for (int i = 0; i < collidingCount; i++)
        {
            int id = collidingIds[i];
            int index = FindChangeIndex(changes, id);
            var snake = FindSnake(state, id);

            if (snake.Body.Count <= 3)
            {
                changes[index].Killed = true;
                changes[index].KilledBody = snake.Body.ToList();
                snake.Body.Clear();
            }
            else
            {
                changes[index].LostCollisionSegment = true;
                changes[index].CollisionHead = snake.Body[0];
                snake.Body.RemoveAt(0);
            }
        }
    }

    private bool CollidesWithOtherSnake(MinimaxGameState state, MinimaxSnake snake)
    {
        bool checkingMySnake = false;
        for (int i = 0; i < state.MySnakes.Count; i++)
        {
            if (state.MySnakes[i].Id == snake.Id) { checkingMySnake = true; break; }
        }

        Point snakeHead = snake.Body[0];

        for (int i = 0; i < state.MySnakes.Count; i++)
        {
            var other = state.MySnakes[i];
            if (other.Body.Count == 0 || other.Id == snake.Id) continue;

            if (snakeHead == other.Body[0])
                return true;

            for (int j = 1; j < other.Body.Count; j++)
            {
                if (snakeHead == other.Body[j]) return true;
            }
        }

        // Check against opponent snakes (track attacks when checking my snake)
        for (int i = 0; i < state.OpponentSnakes.Count; i++)
        {
            var other = state.OpponentSnakes[i];
            if (other.Body.Count == 0 || other.Id == snake.Id) continue;

            if (snakeHead == other.Body[0])
            {
                if (checkingMySnake)
                {
                    if (snake.Body.Count > other.Body.Count)
                        _attackedBigger++;
                    else if (snake.Body.Count < other.Body.Count)
                        _attackedSmaller++;
                }
                return true;
            }

            for (int j = 1; j < other.Body.Count; j++)
            {
                if (snakeHead == other.Body[j]) return true;
            }
        }

        return false;
    }

    private void ApplyGravityChange(MinimaxGameState state, SnakeChange[] changes)
    {
        _dynamicPointsBuffer.Clear();
        foreach (var ps in state.PowerSources)
            _dynamicPointsBuffer.Add(ps);

        for (int i = 0; i < state.MySnakes.Count; i++)
            foreach (var bp in state.MySnakes[i].Body)
                _dynamicPointsBuffer.Add(bp);

        for (int i = 0; i < state.OpponentSnakes.Count; i++)
            foreach (var bp in state.OpponentSnakes[i].Body)
                _dynamicPointsBuffer.Add(bp);

        for (int i = 0; i < state.MySnakes.Count; i++)
            ApplyGravityToSnake(state.MySnakes[i], changes);

        for (int i = 0; i < state.OpponentSnakes.Count; i++)
            ApplyGravityToSnake(state.OpponentSnakes[i], changes);
    }

    private void ApplyGravityToSnake(MinimaxSnake snake, SnakeChange[] changes)
    {
        if (snake.Body.Count == 0) return;

        foreach (var bp in snake.Body)
            _dynamicPointsBuffer.Remove(bp);

        int fall = 0;
        for (int f = 0; f < 20; f++)
        {
            bool canFall = true;
            foreach (var bodyPoint in snake.Body)
            {
                var oneDown = new Point(bodyPoint.X, bodyPoint.Y + 1);
                if (_dynamicPointsBuffer.Contains(oneDown) || _platformPoints.Contains(oneDown)) 
                { 
                    canFall = false; break; 
                }
            }

            if (!canFall) break;

            for (int j = 0; j < snake.Body.Count; j++)
                snake.Body[j] = new Point(snake.Body[j].X, snake.Body[j].Y + 1);
            fall++;
        }

        changes[FindChangeIndex(changes, snake.Id)].GravityFall = fall;

        foreach (var bodyPoint in snake.Body)
            _dynamicPointsBuffer.Add(bodyPoint);
    }

    private void RemoveOutOfBoundsChange(MinimaxGameState state, SnakeChange[] changes)
    {
        foreach (var snake in state.MySnakes)
        {
            if (snake.Body.Count > 0 && IsFullyOutOfBounds(snake.Body))
            {
                int index = FindChangeIndex(changes, snake.Id);
                changes[index].Killed = true;
                changes[index].KilledBody = snake.Body.ToList();
                snake.Body.Clear();
            }
        }

        foreach (var snake in state.OpponentSnakes)
        {
            if (snake.Body.Count > 0 && IsFullyOutOfBounds(snake.Body))
            {
                int index = FindChangeIndex(changes, snake.Id);
                changes[index].Killed = true;
                changes[index].KilledBody = snake.Body.ToList();
                snake.Body.Clear();
            }
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

    private void BuildFloodBlockedSet(MinimaxGameState state)
    {
        _floodBlocked.Clear();

        foreach (var snake in state.MySnakes)
        {
            if (snake.Body.Count == 0)
            {
                continue;
            }

            for (int i = 0; i < snake.Body.Count - 1; i++)
            {
                _floodBlocked.Add(snake.Body[i]);
            }
        }

        foreach (var snake in state.OpponentSnakes)
        {
            if (snake.Body.Count == 0)
            {
                continue;
            }

            for (int i = 0; i < snake.Body.Count - 1; i++)
            { 
                _floodBlocked.Add(snake.Body[i]);
            }
        }
    }

    private int FloodFillReachable(Point head, int cutOff)
    {
        _floodVisited.Clear();
        _floodQueue.Clear();

        _floodQueue.Enqueue(head);
        _floodVisited.Add(head);

        while (_floodQueue.Count > 0)
        {
            if (_floodVisited.Count >= cutOff)
            { 
                return cutOff;
            }

            Point current = _floodQueue.Dequeue();

            for (int d = 0; d < MoveOffsets.Length; d++)
            {
                Point neighbor = new Point(current.X + MoveOffsets[d].X, current.Y + MoveOffsets[d].Y);

                if (neighbor.X < -1 || neighbor.X > _width || neighbor.Y < -1 || neighbor.Y > _height)
                {
                    continue;
                }

                if (_platformPoints.Contains(neighbor) || _floodBlocked.Contains(neighbor) || _floodVisited.Contains(neighbor))
                {
                    continue;
                }

                _floodVisited.Add(neighbor);
                _floodQueue.Enqueue(neighbor);
            }
        }

        return _floodVisited.Count;
    }

    private int Evaluate(MinimaxGameState state)
    {
        _evaluationsThisDepth++;
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

        // Encourage attacking snakes when I'm longer than them, and discourage it when I'm shorter than them
        score += _attackedBigger * 500;
        score -= _attackedSmaller * 500;

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

        // Negative score for me getting trapped, bonus for enemy getting trapped.
        BuildFloodBlockedSet(state);

        foreach (var snake in state.MySnakes)
        {
            if (snake.Body.Count == 0)
            {
                continue;
            }

            int reachable = FloodFillReachable(snake.Body[0], snake.Body.Count);

            if (reachable < snake.Body.Count)
            {
                score -= (snake.Body.Count - reachable) * 1500;
            }
        }

        foreach (var snake in state.OpponentSnakes)
        {
            if (snake.Body.Count == 0)
            {
                continue;
            }

            int reachable = FloodFillReachable(snake.Body[0], snake.Body.Count);

            if (reachable < snake.Body.Count)
            {
                score += (snake.Body.Count - reachable) * 1500;
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

    private static ulong ComputeStateHash(MinimaxGameState state)
    {
        ulong hash = 0xCBF29CE484222325UL;

        foreach (var snake in state.MySnakes)
        {
            hash = FnvMix(hash, (ulong)(uint)snake.Id);
            hash = FnvMix(hash, (ulong)(uint)snake.Body.Count);
            foreach (var p in snake.Body)
            {
                hash = FnvMix(hash, ((ulong)(uint)p.X << 32) | (ulong)(uint)p.Y);
            }
        }

        hash = FnvMix(hash, 0xAAAAAAAAAAAAAAAAUL);

        foreach (var snake in state.OpponentSnakes)
        {
            hash = FnvMix(hash, (ulong)(uint)snake.Id);
            hash = FnvMix(hash, (ulong)(uint)snake.Body.Count);
            foreach (var p in snake.Body)
            {
                hash = FnvMix(hash, ((ulong)(uint)p.X << 32) | (ulong)(uint)p.Y);
            }
        }

        ulong psHash = 0;
        foreach (var ps in state.PowerSources)
        {
            ulong ph = ((ulong)(uint)ps.X << 32) | (ulong)(uint)ps.Y;
            ph *= 0x9E3779B97F4A7C15UL;
            ph ^= ph >> 30;
            psHash ^= ph;
        }
        hash = FnvMix(hash, psHash);

        return hash;
    }

    private static ulong FnvMix(ulong hash, ulong value)
    {
        hash ^= value;
        hash *= 0x100000001B3UL;
        return hash;
    }
}
