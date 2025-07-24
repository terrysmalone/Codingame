

using System.Drawing;
using System.Runtime.CompilerServices;

namespace SummerChallenge2025_SoakOverflow;

partial class Game
{
    public int Width { get; private set; }
    public int Height { get; private set; }

    public int MyId { get; private set; }

    List<Agent> _playerAgents = new List<Agent>();
    List<Agent> _opponentAgents = new List<Agent>();

    int[,] cover;

    private int[,] _splashMap;
    Dictionary<int, double[,]> _coverMaps;


    private CoverMapGenerator _coverMapGenerator;
    private DamageMapGenerator _damageMapGenerator;
    private DamageCalculator _damageCalculator;
    private ScoreCalculator _scoreCalculator;

    AStar _aStar;

    private int _moveCount;

    private int _playerScore, _opponentScore = 0;

    public Game(int myId)
    {
        MyId = myId;
    }

    // One line per agent: <agentId>;<action1;action2;...> actions are "MOVE x y | SHOOT id | THROW x y | HUNKER_DOWN | MESSAGE text"
    internal List<string> GetCommands()
    {
        UpdateScores();
        Console.Error.WriteLine($"Player score: {_playerScore}, Opponent score: {_opponentScore}");

        _splashMap = CreateSplashMap();
        _coverMaps = CreateCoverMaps();

        _damageCalculator = new DamageCalculator(_coverMapGenerator);

        UpdatePriorities();

        GetMoveCommands();

        GetActionCommands();
        
        Display.Sources(_playerAgents);

        List<string> commands = GetCommandStrings();
        ResetIntentions();

        _moveCount++;

        return commands;
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
            (_, var closestEnemyDistance) = GetClosestEnemyPosition(agent);

            if (isOpponentSplashBombInRange(6, agent.Position)
                && _playerAgents.Any(a => a.Id != agent.Id 
                                     && CalculationUtil.GetEuclideanDistance(a.Position, agent.Position) < 3))
            {
                Console.Error.WriteLine($"Agent {agent.Id} closest enemy distance: {closestEnemyDistance}");
                
                foreach (var a in _playerAgents)
                {
                    Console.Error.WriteLine($"Agent distance {CalculationUtil.GetEuclideanDistance(a.Position, agent.Position)}");
                }

                agent.AgentPriority = Priority.SpreadingOut;
            }
            else if (closestEnemyDistance <= agent.OptimalRange)
            {
                agent.AgentPriority = Priority.FindingBestAttackPosition;
            }
            else if (closestEnemyDistance > agent.OptimalRange * 2 || agent.AgentPriority != Priority.FindingBestAttackPosition)
            {
                agent.AgentPriority = Priority.MovingToEnemy;
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

    private void GetMoveCommands()
    {
        List<Move> currentMovePoints = new List<Move>();

        foreach (var agent in _playerAgents)
        {
            // If opponent still has any splashbombs, spread out any agents that are close to each other
            if (agent.AgentPriority == Priority.SpreadingOut)
            {
                GetSpreadMove(agent);
                if (agent.MoveIntention.Move != new Point(-1, -1))
                {
                    continue;
                }
            }

            if ((agent.AgentPriority == Priority.FindingBestAttackPosition && agent.OptimalRange > 2)
                || (agent.AgentPriority == Priority.SpreadingOut && agent.MoveIntention.Move == new Point(-1, -1)))
            {
                GetBestAttackPosition(agent);
            }

            // Default to moving to Enemy
            if (agent.MoveIntention.Move == new Point(-1, -1))
            {
                double[,] agentDamageMap = _damageMapGenerator.CreateDamageMap(agent, _opponentAgents, _splashMap, _coverMaps, cover);

                (Point bestAttackPoint, _) = ClosestPeakFinder.FindClosestPeak(
                    agent.Position,
                    agentDamageMap);

                Point bestPoint = new Point(bestAttackPoint.X, bestAttackPoint.Y);

                if (agent.Position != bestAttackPoint)
                {
                    // Convert the move to the next adjacent move so we know exactly where we'll be on the next turn
                    List<Point> bestPath = _aStar.GetShortestPath(agent.Position, bestAttackPoint);
                    bestPoint = bestPath[0];
                }

                agent.MoveIntention.Move = bestPoint;
                agent.MoveIntention.Source = "Moving to best attack position";
            }            

            // If this point is already being moved to by another agent don't move
            if (currentMovePoints.Any(p => p.To.X == agent.MoveIntention.Move.X && p.To.Y == agent.MoveIntention.Move.Y))
            {
                // Simple first pass implementation. Just don't move, allowing the other one to move instead
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
                        if (cover[pointToCheck.X, pointToCheck.Y] == 0
                            && relevantMove.From != new Point(pointToCheck.X, pointToCheck.Y))
                        {
                            agent.MoveIntention.Move = new Point(pointToCheck.X, pointToCheck.Y);
                            agent.MoveIntention.Source = "Avoiding a collision";
                            break;
                        }
                    }
                }
            }

            currentMovePoints.Add(new Move(agent.Position, agent.MoveIntention.Move));
        }
    }

    private void GetSpreadMove(Agent agent)
    {
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
            }
            else if (closestAgent.Position.X > agent.Position.X && agent.Position.X - 1 >= 0)
            {
                agent.MoveIntention.Move = new Point(agent.Position.X - 1, agent.Position.Y);    
            }
            else if (closestAgent.Position.Y > agent.Position.Y && agent.Position.Y - 1 >= 0)
            {
                agent.MoveIntention.Move = new Point(agent.Position.X, agent.Position.Y - 1);
            }
            else if (closestAgent.Position.Y < agent.Position.Y && agent.Position.Y + 1 <= Height - 1)
            {
                agent.MoveIntention.Move = new Point(agent.Position.X, agent.Position.Y + 1);
            }
            else
            {
                // If we can't move in any direction, just stay still
                agent.MoveIntention.Move = agent.Position;
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

    private void GetBestAttackPosition(Agent agent)
    {
        // Look around the agent by optimal range / 2
        var move = new Point(-1, -1);

        var distanceToCheck = agent.OptimalRange / 2;
        int minX = Math.Max(0, agent.Position.X - distanceToCheck);
        int maxX = Math.Min(Width - 1, agent.Position.X + distanceToCheck);
        int minY = Math.Max(0, agent.Position.Y - distanceToCheck);
        int maxY = Math.Min(Height - 1, agent.Position.Y + distanceToCheck);

        double maxDamageScore = double.MinValue;
        int minDistanceToAgent = int.MaxValue;

        // For each point calculate Damage - Potential damage
        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            { 
                // Calculate possible damage
                var attackDamage = _damageCalculator.CalculateHighestAttackingPlayerDamage(agent, x, y, _opponentAgents);

                // Calculate possible damage taken
                var receivingDamage = _damageCalculator.CalculateReceivingDamage(x, y, _opponentAgents);

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
                List<Point> bestPath = _aStar.GetShortestPath(agent.Position, move);
                bestPoint = bestPath[0];
            }

            agent.MoveIntention.Move = bestPoint;
            agent.MoveIntention.Source = "Moving to best defended attack position";
        }
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

        // Get highest score from splashDamageMap within 4 
        // Manhattan distance from agent's position
        int minX = Math.Max(0, movePoint.X - 4);
        int maxX = Math.Min(Width - 1, movePoint.X + 4);
        int minY = Math.Max(0, movePoint.Y - 4);
        int maxY = Math.Min(Height - 1, movePoint.Y + 4);

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
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
                    (_, var closestEnemyDistance) =  GetClosestEnemyPosition(new Point(x, y));
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
        var mostDamage = 0.0;
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

            var damage = _damageCalculator.CalculateDamage(
                movePoint.X,
                movePoint.Y,
                agent.OptimalRange,
                agent.SoakingPower,
                enemy.Position.X,
                enemy.Position.Y);

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


        if (mostDamage <= 0.0)
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

        cover = new int[Width, Height];
        
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
        cover[x, y] = tileType;
    }

    internal void UpdateCoverRelatedMaps()
    {
        _coverMapGenerator = new CoverMapGenerator(cover);
        _aStar = new AStar(cover);
    }
}
