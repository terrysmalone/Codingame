

using System.Drawing;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;

namespace SummerChallenge2025_SoakOverflow;

partial class Game
{
    public int Width { get; private set; }
    public int Height { get; private set; }

    public int MyId { get; private set; }

    List<Agent> _playerAgents = new List<Agent>();
    List<Agent> _opponentAgents = new List<Agent>();

    int[,] _cover;

    private int[,] _splashMap;
    Dictionary<int, double[,]> _coverMaps;


    private CoverMapGenerator _coverMapGenerator;
    private DamageMapGenerator _damageMapGenerator;
    private DamageCalculator _damageCalculator;
    private ScoreCalculator _scoreCalculator;

    AStar _aStar;

    private int _moveCount;

    private int _playerScore, _opponentScore = 0;

    private bool _inOpening = true;
    private bool _inEndGame = false;

    public Game(int myId)
    {
        MyId = myId;
    }

    // One line per agent: <agentId>;<action1;action2;...> actions are "MOVE x y | SHOOT id | THROW x y | HUNKER_DOWN | MESSAGE text"
    internal List<string> GetCommands()
    {
        UpdateScores();
        Console.Error.WriteLine($"Player score: {_playerScore}, Opponent score: {_opponentScore}");

        UpdateMoveLists();

        _splashMap = CreateSplashMap();
        _coverMaps = CreateCoverMaps();

        _damageCalculator = new DamageCalculator(_coverMapGenerator);

        UpdatePriorities();

        Dictionary<int, Point> landGrabAssignments = GetBestLandGrabPositions();

        GetMoveCommands(landGrabAssignments);

        GetActionCommands();
        
        Display.Sources(_playerAgents);
        //Display.AgentHistories(_playerAgents);

        List<string> commands = GetCommandStrings();
        ResetIntentions();

        _moveCount++;

        return commands;
    }

    // We want to include agents current squares as part of their path so that other agents won't
    // want to walk over them
    private void UpdateMoveLists()
    {
        foreach (var agent in _playerAgents)
        {
            agent.MoveList.Add(agent.Position);
        }
    }

    private Dictionary<int, Point> GetBestLandGrabPositions()
    {
        // count of agents who have InitialLandGrab as their priority
        int initialLandGrabCount = _playerAgents.Count(a => a.AgentPriority == Priority.LandGrab);

        List <Point> landGrabPositions = new List<Point>();

        for (int i = 0; i < initialLandGrabCount; i++)
        {
            var bestPoint = new Point(-1, -1);
            var highestDistance = int.MinValue;

            // for each point on the map
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (landGrabPositions.Contains(new Point(x, y)))
                    {
                        continue; // Skip if this point is already in landGrabPositions
                    }

                    (_, var closestEnemyPosition) = GetClosestEnemyPosition(new Point(x, y));

                    // Get the closest player agent distance, excluding any that have priority as InitialLandGrab
                    var closestPlayerAgent = _playerAgents
                        .Where(a => a.AgentPriority != Priority.LandGrab)
                        .Select(a => CalculationUtil.GetManhattanDistance(a.Position, new Point(x, y)))
                        .DefaultIfEmpty(int.MaxValue)
                        .Min();

                    // Get the distance to the closest point in landGrabPositions
                    var closestLandGrabPosition = landGrabPositions
                        .Select(p => CalculationUtil.GetManhattanDistance(p, new Point(x, y)))
                        .DefaultIfEmpty(int.MaxValue)
                        .Min();

                    // Get the distance to the closet Edge of the 4 edges of the map
                    int closestEdge = Math.Min(Math.Min(x, Width - 1 - x), Math.Min(y, Height - 1 - y));


                    var closest = Math.Min(closestEnemyPosition, closestPlayerAgent);
                    closest = Math.Min(closest, closestLandGrabPosition);
                    closest = Math.Min(closest, closestEdge);

                    if (closest > highestDistance)
                    {
                        highestDistance = closest;
                        bestPoint = new Point(x, y);
                    }
                }
            }

            if (bestPoint != new Point(-1, -1))
            {
                landGrabPositions.Add(bestPoint);
                Console.Error.WriteLine($"Land grab position {i + 1}: {bestPoint.X}, {bestPoint.Y} with distance {highestDistance}");
            }
        }

        Dictionary<int, Point> landGrabAssignments = new Dictionary<int, Point>();
        List<int> assignedAgents = new List<int>();
        for (int i = 0; i < landGrabPositions.Count; i++)
        {
            // get the closest agent that isn't already assigned
            var closestAgent = _playerAgents
                .Where(a => !assignedAgents.Contains(a.Id) && a.AgentPriority == Priority.LandGrab)
                .OrderBy(a => CalculationUtil.GetManhattanDistance(a.Position, landGrabPositions[i]))
                .FirstOrDefault();

            if (closestAgent == null)
            {
                Console.Error.WriteLine($"ERROR: Couldn't find agent for land grab position {landGrabPositions[i].X},{landGrabPositions[i].Y}");
            }
            else
            {
                landGrabAssignments.Add(closestAgent.Id, landGrabPositions[i]);
                assignedAgents.Add(closestAgent.Id);
            }
        }


        return landGrabAssignments;
    }

    private void UpdateScores()
    {
        (int player, int opponent) = _scoreCalculator.CalculateScores(_playerAgents, _opponentAgents);

        if (player > opponent)
        {
            _playerScore += (player - opponent);
        }
        else if (player < opponent)
        {
            _opponentScore += (opponent - player);
        }
    }

    private void ResetIntentions()
    {
        foreach (var agent in _playerAgents)
        {
            agent.ResetIntentions();
        }
    }

    private List<string> GetCommandStrings()
    {
        List<string> commands = new List<string>();

        foreach (var agent in _playerAgents)
        {
            string command = $"{agent.Id}; MOVE {agent.MoveIntention.Move.X} {agent.MoveIntention.Move.Y}; {agent.ActionIntention.Command} MESSAGE {agent.AgentPriority}";
            commands.Add(command);
        }

        return commands;
    }

    private int[,] CreateSplashMap()
    {
        SplashMapGenerator splashMapGenerator = new SplashMapGenerator(Width, Height, _playerAgents, _opponentAgents);
        return splashMapGenerator.CreateSplashMap();
    }

    private Dictionary<int, double[,]> CreateCoverMaps()
    {
        var coverMaps = new Dictionary<int, double[,]>();

        foreach (var agent in _playerAgents)
        {
            coverMaps[agent.Id] = _coverMapGenerator.CreateCoverMap(agent.Position.X, agent.Position.Y);
        }
        foreach (var agent in _opponentAgents)
        {
            coverMaps[agent.Id] = _coverMapGenerator.CreateCoverMap(agent.Position.X, agent.Position.Y);
        }

        return coverMaps;
    }

    private void UpdatePriorities()
    {
        foreach (var agent in _playerAgents)
        {
            // Once any player comes out of the opening all players are out of it
            if (_inOpening)
            {
                if (isOpponentSplashBombInRange(6, agent.Position))
                {
                    _inOpening = false;
                }
                else
                {
                    // Currently don't do anything
                    // agent.AgentPriority = Priority.LandGrab;
                    // continue;
                }
            }

            if (!_inEndGame)
            {
                // If there are no more splashbombs 
                if (_opponentAgents.All(a => a.SplashBombs == 0) 
                    && _playerAgents.All(a => a.SplashBombs == 0))
                {
                    _inEndGame = true;
                }
            }
            
            if (_inEndGame)
            {
                // Currently don't do anything
                //agent.AgentPriority = Priority.MaximiseScore;
                //continue;
            }

            (_, var closestEnemyDistance) = GetClosestEnemyPosition(agent);
            (_, var closestBomberDistance) = GetClosestEnemyWithSplashBombsPosition(agent);

            if (closestBomberDistance <= 8)
            {
                agent.AgentPriority = Priority.DodgingBombs;
            }
            //if (isOpponentSplashBombInRange(6, agent.Position)
            //    && _playerAgents.Any(a => a.Id != agent.Id 
            //                         && CalculationUtil.GetEuclideanDistance(a.Position, agent.Position) < 3))
            //{
            //    Console.Error.WriteLine($"Agent {agent.Id} closest enemy distance: {closestEnemyDistance}");
                
            //    agent.AgentPriority = Priority.SpreadingOut;
            //}
            else if (closestEnemyDistance <= agent.OptimalRange)
            {
                agent.AgentPriority = Priority.FindingBestAttackPosition;
            }
            else if (closestEnemyDistance > agent.OptimalRange * 2 || agent.AgentPriority != Priority.FindingBestAttackPosition)
            {
                agent.AgentPriority = Priority.Advancing;
            }
        }
    }

    private bool isOpponentSplashBombInRange(int range, Point position)
    {
        foreach (var agent in _opponentAgents)
        {
            if (agent.SplashBombs > 0
                && CalculationUtil.GetManhattanDistance(agent.Position, position) <= range)
            {
                return true;
            }
        }

        return false;
    }

    private void GetMoveCommands(Dictionary<int, Point> landGrabAssignments)
    {
        List<Move> currentMovePoints = new List<Move>();

        foreach (var agent in _playerAgents)
        {
            if (MoveRepetitionDetected(agent))
            {
                GetSpreadMove(agent);
            }

            // If enemy has a higher score this round maximise score
            (var player, var opponent) = _scoreCalculator.CalculateScores(_playerAgents, _opponentAgents);

            if (player - opponent < -40)
            {
                GetScoreMaximisingMove(agent);
            }

            if (agent.AgentPriority == Priority.DodgingBombs)
            {
                if (!(agent.OptimalRange == 2 && agent.SplashBombs >= 3))
                {
                    GetBombDodgeMove(agent);

                    GetSpreadMove(agent);
                }
            }

            if (player - opponent < -20)
            {
                GetScoreMaximisingMove(agent);
            }

            if (agent.AgentPriority == Priority.FindingBestAttackPosition && agent.OptimalRange > 2)  
            {               
                GetBestAttackPosition(agent);
            }

            // Default to advancing to Enemy

            if (player - opponent < 0)
            {
                // If we still don't have a move, look for a score maximising move
                GetScoreMaximisingMove(agent);
            }
                
            // If we still don't have a move, get the best advancing move
            GetBestAdvancingMove(agent);
                
            UpdateForCollisions(agent, currentMovePoints);

            currentMovePoints.Add(new Move(agent.Position, agent.MoveIntention.Move));
        }
    }

    private bool MoveRepetitionDetected(Agent agent)
    {
        // if the last 4 moves contain two lots of two repeated moves, we're repeating ourselves
        if (agent.MoveList.Count < 4)
        {
            return false; // Not enough moves to check for repetition
        }

        var lastFourMoves = agent.MoveList.Skip(Math.Max(0, agent.MoveList.Count - 4)).ToList();
        // Check if there are two pairs of repeated moves
        if (lastFourMoves[0] == lastFourMoves[2] 
            && lastFourMoves[1] == lastFourMoves[3]
            && lastFourMoves[0] != lastFourMoves[1]
            && lastFourMoves[2] != lastFourMoves[3])
        {
            return true;
        }

        return false;
    }

    private void GetScoreMaximisingMove(Agent agent)
    {
        if (agent.MoveIntention.Move != new Point(-1, -1))
        {
            // If we already have a move, return
            return;
        }

        int maxScoreDiff = _scoreCalculator.CalculateScoreDiff(_playerAgents, _opponentAgents);

        Point[] pointsToCheck = new Point[4];
        pointsToCheck[0] = new Point(Math.Min(Width - 1, agent.Position.X + 1), agent.Position.Y);
        pointsToCheck[1] = new Point(Math.Max(0, agent.Position.X - 1), agent.Position.Y);
        pointsToCheck[2] = new Point(agent.Position.X, Math.Min(Height - 1, agent.Position.Y + 1));
        pointsToCheck[3] = new Point(agent.Position.X, Math.Max(0, agent.Position.Y - 1));

        foreach (Point point in pointsToCheck)
        {
            // If there is cover at this point , skip it
            if (_cover[point.X, point.Y] > 0)
            {
                continue;
            }

            (var player, var opponent) = _scoreCalculator.CalculateScores(_playerAgents, new Dictionary<int, Point> { { agent.Id, point } }, _opponentAgents);

            int scoreDiff = player - opponent;

            if (scoreDiff > maxScoreDiff)
            {
                maxScoreDiff = scoreDiff;
                agent.MoveIntention.Move = point;
                agent.TargetPath = new List<Point>() { point };
                agent.MoveIntention.Source = "Maximising score";
            }
        }
    }

    private void GetBombDodgeMove(Agent agent)
    {
        if (agent.MoveIntention.Move != new Point(-1, -1))
        {
            // If we already have a move, return
            return;
        }

        (var enemyPosition, var enemyDistance) = GetClosestEnemyWithSplashBombsPosition(agent);

        if (enemyPosition == new Point(-1, -1))
        {
            // No enemies with splash bombs in range
            return;
        }

        // If we're too close to dodge, can we just hug close enough that 
        // he won't want to throw a bomb at us?
        if (enemyDistance <= 3 && enemyDistance > 1)
        {
            // Move towards the enemy position
            List<Point> bestPath = _aStar.GetShortestPath(agent.Position, enemyPosition);
            
            var bestPoint = bestPath[0];
            agent.TargetPath = bestPath;
            agent.MoveIntention.Move = bestPoint;
            agent.TargetPath = new List<Point>() { bestPoint };

            agent.MoveIntention.Source = "Hug him!";
            return;
        }

        // For now, just assume x-axis is more important than y-axis
        // If it turns out it's not we can worry about it later
        if (agent.Position.Y == enemyPosition.Y)
        {
            if (enemyDistance == 6)
             {
                // move back
                if (agent.Position.X < enemyPosition.X && agent.Position.X > 0)
                {
                    agent.MoveIntention.Move = new Point(agent.Position.X - 1, agent.Position.Y);
                    agent.TargetPath = new List<Point>() { new Point(agent.Position.X - 1, agent.Position.Y) };
                }
                else if (agent.Position.X < Width - 1)
                {
                    agent.MoveIntention.Move = new Point(agent.Position.X + 1, agent.Position.Y);
                    agent.TargetPath = new List<Point>() { new Point(agent.Position.X + 1, agent.Position.Y) };
                }
            }
            else if (enemyDistance == 7)
            {
                // stay still
                // agent.MoveIntention.Move = new Point(agent.Position.X, agent.Position.Y);
            }
        }
        else if (Math.Abs(agent.Position.Y - enemyPosition.Y) == 1 
                 || Math.Abs(agent.Position.Y - enemyPosition.Y) == 2
                 || Math.Abs(agent.Position.Y - enemyPosition.Y) == 3)
        {
            if (enemyDistance <= 7)
            {
                // move back
                if (agent.Position.X < enemyPosition.X && agent.Position.X > 0)
                {
                    agent.MoveIntention.Move = new Point(agent.Position.X - 1, agent.Position.Y);
                    agent.TargetPath = new List<Point>() { new Point(agent.Position.X - 1, agent.Position.Y) };
                }
                else if (agent.Position.X < Width - 1)
                {
                    agent.MoveIntention.Move = new Point(agent.Position.X + 1, agent.Position.Y);
                    agent.TargetPath = new List<Point>() { new Point(agent.Position.X + 1, agent.Position.Y) };
                }
            }
            else if (enemyDistance <= 8)
            {
                // stay still
                agent.MoveIntention.Move = new Point(agent.Position.X, agent.Position.Y);
            }
        }

        agent.MoveIntention.Source = "Dodging a bomb";
    }

    private void GetSpreadMove(Agent agent)
    {
        if (agent.MoveIntention.Move != new Point(-1, -1))
        {
            // If we already have a move, return
            return;
        }

        // Get the closest agent to this agent
        var closestAgent = _playerAgents
            .Where(a => a.Id != agent.Id)
            .OrderBy(a => CalculationUtil.GetEuclideanDistance(a.Position, agent.Position))
            .FirstOrDefault();

        if (closestAgent != null)
        {
            agent.MoveIntention.Source = "Spreading";
            if (closestAgent.Position.X < agent.Position.X && agent.Position.X + 1 <= Width - 1)
            {
                agent.MoveIntention.Move = new Point(agent.Position.X + 1, agent.Position.Y);
                agent.TargetPath = new List<Point>() { new Point(agent.Position.X + 1, agent.Position.Y) };
            }
            else if (closestAgent.Position.X > agent.Position.X && agent.Position.X - 1 >= 0)
            {
                agent.MoveIntention.Move = new Point(agent.Position.X - 1, agent.Position.Y);
                agent.TargetPath = new List<Point>() { new Point(agent.Position.X - 1, agent.Position.Y) };
            }
            else if (closestAgent.Position.Y > agent.Position.Y && agent.Position.Y - 1 >= 0)
            {
                agent.MoveIntention.Move = new Point(agent.Position.X, agent.Position.Y - 1);
                agent.TargetPath = new List<Point>() { new Point(agent.Position.X, agent.Position.Y - 1) };
            }
            else if (closestAgent.Position.Y < agent.Position.Y && agent.Position.Y + 1 <= Height - 1)
            {
                agent.MoveIntention.Move = new Point(agent.Position.X, agent.Position.Y + 1);
                agent.TargetPath = new List<Point>() { new Point(agent.Position.X, agent.Position.Y + 1) };
            }
            else
            {
                // If we can't move in any direction, just stay still
                agent.MoveIntention.Move = agent.Position;
            }
        }
    }

    private void GetBestAttackPosition(Agent agent)
    {
        if (agent.MoveIntention.Move != new Point(-1, -1))
        {
            // If we already have a move, return
            return;
        }

        // Look around the agent by optimal range / 2
        var move = new Point(-1, -1);

        var distanceToCheck = agent.OptimalRange / 2;
        int minX = Math.Max(0, agent.Position.X - distanceToCheck);
        int maxX = Math.Min(Width - 1, agent.Position.X + distanceToCheck);
        int minY = Math.Max(0, agent.Position.Y - distanceToCheck);
        int maxY = Math.Min(Height - 1, agent.Position.Y + distanceToCheck);

        double maxDamageScore = double.MinValue;
        int minDistanceToAgent = int.MaxValue;

        (_, var distanceToEnemy) = GetClosestEnemyPosition(agent);

        // For each point calculate Damage - Potential damage
        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                (_, var closestEnemyToPoint) = GetClosestEnemyPosition(new Point(x, y));
                if (closestEnemyToPoint > distanceToEnemy)
                {
                    continue;
                }
                

                // If an enemy agent is already at to this point, skip it
                if (_opponentAgents.Any(enemyAgent => enemyAgent.Position.X == x && enemyAgent.Position.Y == y))
                {
                    continue;
                }

                // If there is cover at this point , skip it
                if (_cover[x, y] > 0)
                {
                    continue;
                }

                // Calculate possible damage
                var attackDamage = _damageCalculator.CalculateHighestAttackingPlayerDamage(agent, x, y, _opponentAgents);

                // Calculate possible damage taken
                var receivingDamage = _damageCalculator.CalculateTotalReceivingDamage(x, y, _opponentAgents);

                // Calculate score
                var score = attackDamage - receivingDamage;
                if (score >= maxDamageScore)
                {
                    var distanceToAgent = CalculationUtil.GetManhattanDistance(agent.Position, new Point(x, y));

                    if (score == maxDamageScore)
                    {
                        // If the score is the same, check if it's closer to the agent
                        if (distanceToAgent < minDistanceToAgent)
                        {
                            maxDamageScore = score;
                            minDistanceToAgent = distanceToAgent;
                            move = new Point(x, y);
                        }
                    }
                    else
                    {
                        maxDamageScore = score;
                        minDistanceToAgent = distanceToAgent;
                        move = new Point(x, y);
                    }
                }
            }
        }

        if (move != new Point(-1, -1))
        {
            Point bestPoint = new Point(move.X, move.Y);

            if (agent.Position != move)
            {
                List<Point> bestPath = _aStar.GetShortestPath(agent.Position, move, GetFullPaths());
                bestPoint = bestPath[0];
                agent.TargetPath = bestPath;
            }

            
            agent.MoveIntention.Move = bestPoint;
            agent.MoveIntention.Source = "Moving to best attack position";

        }
    }

    private void GetBestAdvancingMove(Agent agent)
    {
        if (agent.MoveIntention.Move != new Point(-1, -1))
        {
            // If we already have a move, return
            return;
        }

        double[,] agentDamageMap = _damageMapGenerator.CreateDamageMap(agent, _opponentAgents, _splashMap, _coverMaps, _cover);

        (Point bestAttackPoint, _) = ClosestPeakFinder.FindClosestPeak(
            agent.Position,
            agentDamageMap);

        Point bestPoint = new Point(bestAttackPoint.X, bestAttackPoint.Y);
        
        if (agent.Position != bestAttackPoint)
        {
            // Convert the move to the next adjacent move so we know exactly where we'll be on the next turn
            List<Point> bestPath = _aStar.GetShortestPath(agent.Position, bestAttackPoint, GetFullPaths());

            bestPoint = bestPath[0];
            agent.TargetPath = bestPath;
        }

        agent.MoveIntention.Move = bestPoint;
        agent.MoveIntention.Source = "Moving to best advancing position";
    }

    private List<List<Point>> GetFullPaths()
    {
        List<List<Point>> fullPaths = new List<List<Point>>();

        // Don't return any paths if we've moved more than 4 times
        if (_moveCount > 4)
        {
            return fullPaths;
        }

        foreach (var agent in _playerAgents)
        {
            var fullPath = new List<Point>();
            fullPath.AddRange(agent.MoveList);
            fullPath.AddRange(agent.TargetPath);

            fullPaths.Add(fullPath);
        }
        return fullPaths;
    }

    private void UpdateForCollisions(Agent agent, List<Move> currentMovePoints)
    {
        // If this point is already being moved to by another agent don't move
        if (currentMovePoints.Any(p => p.To.X == agent.MoveIntention.Move.X && p.To.Y == agent.MoveIntention.Move.Y))
        {
            // Simple first pass implementation. Just don't move, allowing the other one to move instead
            agent.TargetPath = new List<Point>();
            agent.MoveIntention.Move = agent.Position;
        }

        // If another agent is moving onto this agent
        if (currentMovePoints.Any(p => p.To.X == agent.Position.X && p.To.Y == agent.Position.Y))
        {
            Move relevantMove = currentMovePoints.First(p => p.To.X == agent.Position.X && p.To.Y == agent.Position.Y);
            //   If this agent is staying still or this agent is moving onto that agent's block
            if (agent.Position == agent.MoveIntention.Move || agent.MoveIntention.Move == relevantMove.From)
            {
                Point[] pointsToCheck = new Point[4];
                pointsToCheck[0] = new Point(Math.Min(Width - 1, agent.Position.X + 1), agent.Position.Y);
                pointsToCheck[1] = new Point(Math.Max(0, agent.Position.X - 1), agent.Position.Y);
                pointsToCheck[2] = new Point(agent.Position.X, Math.Min(Height - 1, agent.Position.Y + 1));
                pointsToCheck[3] = new Point(agent.Position.X, Math.Max(0, agent.Position.Y - 1));

                foreach (var pointToCheck in pointsToCheck)
                {
                    if (_cover[pointToCheck.X, pointToCheck.Y] == 0
                        && relevantMove.From != new Point(pointToCheck.X, pointToCheck.Y))
                    {
                        agent.MoveIntention.Move = new Point(pointToCheck.X, pointToCheck.Y);
                        agent.TargetPath = new List<Point>() {pointToCheck };
                        agent.MoveIntention.Source = "Avoiding a collision";
                        break;
                    }
                }
            }
        }
    }

    private (Point, int) GetClosestEnemyPosition(Agent agent)
    {
        Point closestEnemyPosition = new Point(-1, -1);
        int closestDistance = int.MaxValue;

        foreach (var enemy in _opponentAgents)
        {
            int distance = CalculationUtil.GetManhattanDistance(agent.Position, enemy.Position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemyPosition = enemy.Position;
            }
        }

        return (closestEnemyPosition, closestDistance);
    }

    private (Point, int) GetClosestEnemyWithSplashBombsPosition(Agent agent)
    {
        Point closestEnemyPosition = new Point(-1, -1);
        int closestDistance = int.MaxValue;

        foreach (var enemy in _opponentAgents)
        {
            if (enemy.SplashBombs <= 0)
            {
                continue; // Skip enemies without splash bombs
            }

            int distance = CalculationUtil.GetManhattanDistance(agent.Position, enemy.Position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemyPosition = enemy.Position;
            }
        }

        return (closestEnemyPosition, closestDistance);
    }

    private (Point, double) GetClosestEuclideanEnemyPosition(Agent agent)
    {
        Point closestEnemyPosition = new Point(-1, -1);
        double closestDistance = double.MaxValue;

        foreach (var enemy in _opponentAgents)
        {
            double distance = CalculationUtil.GetEuclideanDistance(agent.Position, enemy.Position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemyPosition = enemy.Position;
            }
        }

        return (closestEnemyPosition, closestDistance);
    }

    private (Point, int) GetClosestEnemyPosition(Point position)
    {
        Point closestEnemyPosition = new Point(-1, -1);
        int closestDistance = int.MaxValue;

        foreach (var enemy in _opponentAgents)
        {
            int distance = CalculationUtil.GetManhattanDistance(position, enemy.Position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemyPosition = enemy.Position;
            }
        }

        return (closestEnemyPosition, closestDistance);
    }

    private void GetActionCommands()
    {

        foreach (var agent in _playerAgents)
        {
            // Within range of a valid throw 
            if (agent.SplashBombs > 0)
            {
                GetThrowAction(agent);
            }

            if (agent.ActionIntention.Command == "" && agent.ShootCooldown <= 0)
            {
                GetShootAction(agent);
            }

            if (agent.ActionIntention.Command == "")
            {
                agent.ActionIntention.Command = "HUNKER_DOWN;";
                agent.ActionIntention.Source = "Hunkering down";
            }
        }
    }

    private void GetThrowAction(Agent agent)
    {
        Point movePoint = agent.MoveIntention.Move;
        // If the movePoint is more than one away from the agents position don't throw
        if (CalculationUtil.GetManhattanDistance(agent.Position, movePoint) > 1)
        {
            Console.Error.WriteLine($"ERROR: WE SHOULD NEVER BE HITTING THIS BECAUSE OF THE A* PATH FINDING");
            return;
        }

        int bestX = -1;
        int bestY = -1;
        int bestValue = 0;
        int bestValueDistance = int.MaxValue;

        Point calculationPoint = new Point(-1, -1);

        // Get highest score from splashDamageMap within 
        // Manhattan distance from agent's position

        int distance = 4;
        int minX = Math.Max(0, movePoint.X - distance);
        int maxX = Math.Min(Width - 1, movePoint.X + distance);
        int minY = Math.Max(0, movePoint.Y - distance);
        int maxY = Math.Min(Height - 1, movePoint.Y + distance);

        var playerBombCount = _playerAgents.Sum(a => a.SplashBombs);
        var opponentBombCount = _opponentAgents.Sum(a => a.SplashBombs);

        var onlyPlayerHasBombs = playerBombCount > 0 && opponentBombCount == 0;

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                if (CalculationUtil.GetManhattanDistance(movePoint, new Point(x, y)) > 3 
                    && !onlyPlayerHasBombs)
                {
                    Console.Error.WriteLine($"Agent {agent.Id} skipping because disstance is more than 3");
                    continue; // Skip points that are more than 3 away
                }
                else if (CalculationUtil.GetManhattanDistance(movePoint, new Point(x, y)) > 4)
                {
                    Console.Error.WriteLine($"Agent {agent.Id} skipping because disstance is more than 4");
                    continue; // Skip points that are more than 4 away
                }

                bool friendlyFire = false;
                // We don't want to throw a bomb if it would damage a point any of our agents are moving to
                foreach (var playerAgent in _playerAgents)
                {                    
                    if (CalculationUtil.GetEuclideanDistance(playerAgent.MoveIntention.Move, new Point(x, y)) < 2)
                    {
                        friendlyFire = true;
                    }
                }

                if (friendlyFire)
                {
                    continue;
                }

                var manhattanDistance = CalculationUtil.GetManhattanDistance(
                    movePoint, new Point(x, y));

                if (manhattanDistance <= 4 && _splashMap[x, y] >= bestValue)
                {
                    (var closestEnemyPosition, var closestEnemyDistance) =  GetClosestEnemyPosition(new Point(x, y));
                    Console.Error.WriteLine($"Closest enemy position is {closestEnemyPosition.X},{closestEnemyPosition.Y}");
                    Console.Error.WriteLine($"onlyPlayerHasBombs: {onlyPlayerHasBombs}");
                    if (onlyPlayerHasBombs)
                    {
                        if (new Point(x,y) != closestEnemyPosition)
                        {
                            continue;
                        }
                    }

                    if (_splashMap[x, y] == bestValue)
                    {
                        if (closestEnemyDistance < bestValueDistance)
                        {
                            bestValue = _splashMap[x, y];
                            bestX = x;
                            bestY = y;
                            bestValueDistance = closestEnemyDistance;
                        }
                    }
                    else
                    {
                        bestValue = _splashMap[x, y];
                        bestX = x;
                        bestY = y;
                        bestValueDistance = closestEnemyDistance;
                    }
                }
            }
        }

        if (bestValue > 0)
        {
            agent.ActionIntention.Source = "Throwing a bomb";
            agent.ActionIntention.Command = $"THROW {bestX} {bestY};";
        }
    }

    private void GetShootAction(Agent agent)
    {
        Point movePoint = agent.MoveIntention.Move;

        // Get target
        var mostDamage = double.MinValue;
        var mostDamageId = -1;

        var killId = -1;
        var soakId = -1;

        foreach (var enemy in _opponentAgents)
        {
            // If enemy is not in range of agent, skip it
            if (CalculationUtil.GetManhattanDistance(enemy.Position, agent.Position) - 1 > agent.OptimalRange * 2)
            {
                continue;
            }

            var damage = 0.0;

            // We're trying to move to a square they're already in. We can't calculate a proper amount of damage in this case. 
            // Instead, calculate it as if we don't move. It's a close enough approximation
            if (movePoint == enemy.Position)
            {
                damage = _damageCalculator.CalculateDamage(
                    agent.Position.X,
                    agent.Position.Y,
                    agent.OptimalRange,
                    agent.SoakingPower,
                    enemy.Position.X,
                    enemy.Position.Y);
            }
            else
            {
                damage = _damageCalculator.CalculateDamage(
                    movePoint.X,
                    movePoint.Y,
                    agent.OptimalRange,
                    agent.SoakingPower,
                    enemy.Position.X,
                    enemy.Position.Y);
            }

            if (enemy.Wetness + damage >= 100 && !isAnyOneKilling())
            {
                killId = enemy.Id;
            }
            else if (enemy.Wetness < 50 && enemy.Wetness + damage >= 50 && !isAnyOneSoaking())
            {
                soakId = enemy.Id;
            }

            if (damage > mostDamage && !isAnyOneKilling())
            {
                mostDamage = damage;
                mostDamageId = enemy.Id;
            }
        }

        if (killId != -1)
        {
            agent.ActionIntention.Command = $"SHOOT {killId};";
            agent.ActionIntention.Source = "Shooting to kill";
            agent.ShootToKillId = killId;
            return;
        }
        else if (soakId != -1)
        {
            agent.ActionIntention.Command = $"SHOOT {soakId};";
            agent.ActionIntention.Source = "Shooting to soak";
            agent.ShootToSoakId = soakId;
            return;
        }


        if (mostDamage < 0.0)
        {
            return;
        }

        agent.ActionIntention.Command = $"SHOOT {mostDamageId};";
        agent.ActionIntention.Source = "Shooting to damage";
    }

    private bool isAnyOneKilling()
    {
        foreach (var agent in _playerAgents)
        {
            if (agent.ShootToKillId != -1)
            {
                return true;
            }
        }

        return false;
    }

    private bool isAnyOneSoaking()
    {
        foreach (var agent in _playerAgents)
        {
            if (agent.ShootToSoakId != -1)
            {
                return true;
            }
        }

        return false;
    }

    public void SetGameSize(int width, int height)
    {
        Width = width;
        Height = height;

        _cover = new int[Width, Height];
        
        _damageMapGenerator = new DamageMapGenerator(width, height);
        _scoreCalculator = new ScoreCalculator(width, height);
    }

    internal void AddAgent(int id, int player, int shootCooldown, int optimalRange, int soakingPower, int splashBombs)
    {
        if (player == MyId)
        {
            _playerAgents.Add(new Agent(id, player, shootCooldown, optimalRange, soakingPower, splashBombs));
        }
        else
        {
            _opponentAgents.Add(new Agent(id, player, shootCooldown, optimalRange, soakingPower, splashBombs));
        }
    }

    internal void MarkAllAgentsForCulling()
    {
        foreach (var agent in _playerAgents)
        {
            agent.InGame = false;
        }

        foreach (var agent in _opponentAgents)
        {
            agent.InGame = false;
        }
    }

    internal void DestroyMarkedAgents()
    {
        _playerAgents.RemoveAll(agent => !agent.InGame);
        _opponentAgents.RemoveAll(agent => !agent.InGame);
    }

    internal void UpdateAgent(int agentId, int x, int y, int cooldown, int splashBombs, int wetness)
    {
        Agent agent = _playerAgents.FirstOrDefault(a => a.Id == agentId) 
            ?? _opponentAgents.FirstOrDefault(a => a.Id == agentId);

        if (agent != null)
        {
            agent.UpdatePosition(x, y);
            agent.ShootCooldown = cooldown;
            agent.SplashBombs = splashBombs;
            agent.Wetness = wetness;
            agent.InGame = true;
        }
    }

    internal void SetCover(int x, int y, int tileType)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            Console.Error.WriteLine($"Cover position {x}, {y} out of bounds");
            return;
        }
        _cover[x, y] = tileType;
    }

    internal void UpdateCoverRelatedMaps()
    {
        _coverMapGenerator = new CoverMapGenerator(_cover);
        _aStar = new AStar(_cover);
    }
}
