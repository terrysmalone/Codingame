

using System.Drawing;
using System.Runtime.CompilerServices;

namespace SummerChallenge2025_SoakOverflow;

partial class Game
{
    public int Width { get; private set; }
    public int Height { get; private set; }

    public int MyId { get; private set; }

    List<Agent> playerAgents = new List<Agent>();
    List<Agent> opponentAgents = new List<Agent>();

    int[,] cover;

    private CoverMapGenerator coverMapGenerator;
    private DamageMapGenerator damageMapGenerator;
    private int[,] coverHillMap;

    AStar _aStar;

    private int _moveCount;

    public Game(int myId)
    {
        MyId = myId;
    }

    // One line per agent: <agentId>;<action1;action2;...> actions are "MOVE x y | SHOOT id | THROW x y | HUNKER_DOWN | MESSAGE text"
    internal List<string> GetMoves()
    {
        int[,] splashMap = CreateSplashMap();
        Dictionary<int, double[,]> coverMaps = CreateCoverMaps();

        List<string> moves = new List<string>();
        List<Move> movePoints = new List<Move>();
        foreach (var agent in playerAgents)
        {
            string fullMove = $"{agent.Id}; ";

            // Get the best move
            (var move, Point nextMove) = GetBestMove(agent, splashMap, coverMaps, coverHillMap, movePoints);
            fullMove += move;

            if (CalculationUtil.GetManhattanDistance(nextMove, agent.Position) > 1)
            {
                Console.Error.WriteLine($"ERROR: Agent {agent.Id}: Next Move ({nextMove.X},{nextMove.Y} is too far away from agent at ({agent.Position.X},{agent.Position.Y}))");
            }

            // Get the best action
            fullMove += GetBestAction(agent, nextMove, splashMap);

            moves.Add(fullMove);
        }

        _moveCount++;

        return moves;
    }

    private int[,] CreateSplashMap()
    {
        SplashMapGenerator splashMapGenerator = new SplashMapGenerator(Width, Height, playerAgents, opponentAgents);
        return splashMapGenerator.CreateSplashMap();
    }

    private Dictionary<int, double[,]> CreateCoverMaps()
    {
        var coverMaps = new Dictionary<int, double[,]>();

        foreach (var agent in playerAgents)
        {
            coverMaps[agent.Id] = coverMapGenerator.CreateCoverMap(agent.Position.X, agent.Position.Y);
        }
        foreach (var agent in opponentAgents)
        {
            coverMaps[agent.Id] = coverMapGenerator.CreateCoverMap(agent.Position.X, agent.Position.Y);
        }

        return coverMaps;
    }

    private (string move, Point nextMove) GetBestMove(Agent agent, 
                                                      int[,] splashMap, 
                                                      Dictionary<int, double[,]> coverMaps, 
                                                      int[,] coverHillMap, 
                                                      List<Move> movePoints)
    {
        var move = "";
        var nextMove = new Point(-1, -1);

        // If the nearest enemy is more than two times the agent's optimal range away
        // then we should move towards the nearest high damage spot
        (_, var closestEnemyDistance) = GetClosestEnemyPosition(agent);

        // Update Priority
        if (agent.AgentPriority == Priority.MovingToEnemy)
        {
            if (closestEnemyDistance <= agent.OptimalRange)
            {
                agent.AgentPriority = Priority.FindingBestAttackPosition;
            }
        }
        else if(agent.AgentPriority == Priority.FindingBestAttackPosition)
        {
            if (closestEnemyDistance > agent.OptimalRange * 2)
            {
                agent.AgentPriority = Priority.MovingToEnemy;
            }
        }

        if (agent.AgentPriority == Priority.FindingBestAttackPosition && agent.OptimalRange > 2)
        {
            // (var coverMove, nextMove) = GetClosestCoverMove(agent, coverHillMap);
            (var coverMove, nextMove) = GetBestAttackPoint(agent);

            if (nextMove != new Point(-1, -1))
            {
                move = coverMove;
                Console.Error.WriteLine($"Agent {agent.Id} move source - Move to best cover");
            }
        }

        if (nextMove == new Point(-1, -1))
        {
            Console.Error.WriteLine($"Agent {agent.Id} looking for best attack position");
            double[,] agentDamageMap = damageMapGenerator.CreateDamageMap(agent, opponentAgents, splashMap, coverMaps, cover);

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

            move = $"MOVE {bestPoint.X} {bestPoint.Y}; ";
            nextMove = bestPoint;

            Console.Error.WriteLine($"Agent {agent.Id} move source - Move to best attack position");
        }

        // If this point is already being moved to by another agent don't move
        if (movePoints.Any(p => p.To.X == nextMove.X && p.To.Y == nextMove.Y))
        {
            // Simple first pass implementation. Just don't move, allowing the other one to move instead
            nextMove = agent.Position;
        }

        Console.Error.WriteLine($"Agent {agent.Id} best attack position at {nextMove.X}, {nextMove.Y}");
        // If another agent is moving onto this agent
        if (movePoints.Any(p => p.To.X == agent.Position.X && p.To.Y == agent.Position.Y))
        {
            Console.Error.WriteLine($"Agent {agent.Id} is moving onto another agent's block at {agent.Position.X}, {agent.Position.Y}");
            Move relevantMove = movePoints.First(p => p.To.X == agent.Position.X && p.To.Y == agent.Position.Y);
            //   If this agent is staying still or this agent is moving onto that agent's block
            if (agent.Position == nextMove || nextMove == relevantMove.From)
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
                        nextMove = new Point(pointToCheck.X, pointToCheck.Y);
                        break;
                    }
                }
            }
        }


        movePoints.Add(new Move(agent.Position, nextMove));
        move = $"MOVE {nextMove.X} {nextMove.Y}; ";

        return (move, nextMove);
    }

    private (Point, int) GetClosestEnemyPosition(Agent agent)
    {
        Point closestEnemyPosition = new Point(-1, -1);
        int closestDistance = int.MaxValue;

        foreach (var enemy in opponentAgents)
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

    private (string, Point) GetBestAttackPoint(Agent agent)
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
                var attackDamage = CalculateHighestAttackingPlayerDamage(agent, x, y);

                // Calculate possible damage taken
                var receivingDamage = CalculateReceivingPlayerDamage(x, y);

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
                Console.Error.WriteLine($"Found attack move at {bestPoint.X}, {bestPoint.Y} with damage score {maxDamageScore} and distance {minDistanceToAgent}");
            }

            return ($"MOVE {bestPoint.X} {bestPoint.Y}; ", bestPoint);
        }

        return ("", move);
    }

    private (string, Point) GetClosestCoverMove(Agent agent, int[,] coverHillMap)
    {
        // TODO: Check closer to current position first
        Console.Error.WriteLine($"Agent {agent.Id} looking for cover");
        var move = new Point(-1, -1);

        var distanceToCheck = 3;
        int minX = Math.Max(0, agent.Position.X - distanceToCheck);
        int maxX = Math.Min(Width - 1, agent.Position.X + distanceToCheck);
        int minY = Math.Max(0, agent.Position.Y - distanceToCheck);
        int maxY = Math.Min(Height - 1, agent.Position.Y + distanceToCheck);

        (var closestEnemyPosition, var closestEnemyDistance) = GetClosestEnemyPosition(agent);

        double min = double.MaxValue;
        int minDistance = int.MaxValue;

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                // If this position contains cover, skip it
                if (cover[x, y] > 0)
                {
                    continue;
                }

                // If another agent is at x, y don't bother checking it
                if (playerAgents.Any(a => a.Position.X == x && a.Position.Y == y && a.Id != agent.Id) ||
                    opponentAgents.Any(a => a.Position.X == x && a.Position.Y == y))
                {
                    continue;
                }

                var distance = CalculationUtil.GetManhattanDistance(new Point(x, y), closestEnemyPosition);
                // If this position would move agent further away from closest player than optimal range, skip it
                if (distance > agent.OptimalRange)
                {
                    continue;
                }

                double possibleDamage = 0.0;

                possibleDamage += CalculateReceivingPlayerDamage(x, y);

                if (possibleDamage <= min)
                {
                    if (possibleDamage == min)
                    {
                        if (distance < minDistance)
                        {
                            Console.Error.WriteLine($"Found better and closer cover at {x}, {y} with damage {possibleDamage}");
                            min = possibleDamage;
                            minDistance = distance;

                            move = new Point(x, y);
                        }
                    }
                    else
                    {
                        Console.Error.WriteLine($"Found better cover at {x}, {y} with damage {possibleDamage}");
                        min = possibleDamage;
                        minDistance = distance;

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
                // Convert the move to the next adjacent move so we know exactly where we'll be on the next turn
                List<Point> bestPath = _aStar.GetShortestPath(agent.Position, move);
                bestPoint = bestPath[0];
                Console.Error.WriteLine($"Found cover at {bestPoint.X}, {bestPoint.Y} with damage {min} and distance {minDistance}");
            }

            return ($"MOVE {bestPoint.X} {bestPoint.Y}; ", bestPoint);
        }

        return ("", move);
    }

    private double CalculateTotalAttackingPlayerDamage(Agent agent, int x, int y)
    {
        var damage = 0.0;
        foreach (var enemy in opponentAgents)
        {
            damage += CalculateDamage(
                x,
                y,
                agent.OptimalRange,
                agent.SoakingPower,
                enemy.Position.X,
                enemy.Position.Y);

        }

        return damage;
    }

    private double CalculateHighestAttackingPlayerDamage(Agent agent, int x, int y)
    {
        var highestDamage = 0.0;
        foreach (var enemy in opponentAgents)
        {
            var damage = CalculateDamage(
                x,
                y,
                agent.OptimalRange,
                agent.SoakingPower,
                enemy.Position.X,
                enemy.Position.Y);

            if (damage > highestDamage)
            {
                highestDamage = damage;
            }

        }

        return highestDamage;
    }

    private double CalculateReceivingPlayerDamage(int x, int y)
    {
        var stationaryReceivingDamage = 0.0;
        foreach (var enemy in opponentAgents)
        {
            stationaryReceivingDamage += CalculateDamage(
                enemy.Position.X,
                enemy.Position.Y,
                enemy.OptimalRange,
                enemy.SoakingPower,
                x,
                y);

        }

        return stationaryReceivingDamage;
    }

    private double CalculateDamage(int fromX, int fromY, int optimalRange, int soakingPower, int targetX, int targetY)
    {
        double[,] map = coverMapGenerator.CreateCoverMap(targetX, targetY);

        var damageMultiplier = map[fromX, fromY];
        var baseDamage = soakingPower * damageMultiplier;

        int manhattanDistance = CalculationUtil.GetManhattanDistance(
            new Point(targetX, targetY), new Point(fromX, fromY));

        if (manhattanDistance <= optimalRange)
        {
            return baseDamage;
        }
        else if (manhattanDistance <= optimalRange * 2)
        {
            return baseDamage / 2;
        }
        else
        {
            return 0;
        }
    }

    private string GetBestAction(Agent agent, Point nextMove, int[,] splashMap)
    {
        var move = "";
        var throwAction = "";

        // Within range of a valid throw 
        if (agent.SplashBombs > 0)
        {
            throwAction += GetThrowAction(agent, nextMove, splashMap);

            if (throwAction != "")
            {
                move += throwAction;
                Console.Error.WriteLine($"Agent {agent.Id} action source - throw a bomb");
            }
        }

        var shootAction = "";
        if (throwAction == "" && agent.ShootCooldown <= 0)
        {
            shootAction = GetShootAction(agent, nextMove);

            if (shootAction != "")
            {
                move += shootAction;
                Console.Error.WriteLine($"Agent {agent.Id} action source - shoot");
            }
        }

        if (shootAction == "" && throwAction == "")
        {
            move += "HUNKER_DOWN;";
            Console.Error.WriteLine($"Agent {agent.Id} action source - default to hunkering");
        }

        return move;
    }

    private string GetThrowAction(Agent agent, Point movePoint, int[,] splashDamageMap)
    {
        // If the movePoint is more than one away from the agents position don't throw
        if (CalculationUtil.GetManhattanDistance(agent.Position, movePoint) > 1)
        {
            Console.Error.WriteLine($"ERROR: WE SHOULD NEVER BE HITTING THIS BECAUSE OF THE A* PATH FINDING");
            return "";
        }

        int bestX = -1;
        int bestY = -1;
        int bestValue = 0;

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
                // We don't want to throw a bomb if it would hit the point we're moving to (movePoint)
                if (Math.Abs(x - movePoint.X) <= 1 && Math.Abs(y - movePoint.Y) <= 1)
                {
                    continue;
                }

                var manhattanDistance = CalculationUtil.GetManhattanDistance(
                    movePoint, new Point(x, y));

                if (manhattanDistance <= 4 && splashDamageMap[x, y] > bestValue)
                {
                    bestValue = splashDamageMap[x, y];
                    bestX = x;
                    bestY = y;
                }
            }
        }

        if (bestValue > 0)
        {
            return $"THROW {bestX} {bestY}";
        }

        return "";
    }

    private string GetShootAction(Agent agent, Point movePoint)
    {
        // Get target
        var bestAttack = 0.0;
        var bestAttackId = -1;

        foreach (var enemy in opponentAgents)
        {
            // If enemy is not in range of agent, skip it
            if (CalculationUtil.GetManhattanDistance(enemy.Position, agent.Position) - 1 > agent.OptimalRange * 2)
            {
                continue;
            }

            var damage = CalculateDamage(
            movePoint.X,
            movePoint.Y,
            agent.OptimalRange,
            agent.SoakingPower,
            enemy.Position.X,
            enemy.Position.Y);

            if (damage > bestAttack)
            {
                bestAttack = damage;
                bestAttackId = enemy.Id;
            }
        }


        if (bestAttack <= 0.0)
        {
            return "";
        }

        return $"SHOOT {bestAttackId}";
    }

    public void SetGameSize(int width, int height)
    {
        Width = width;
        Height = height;

        cover = new int[Width, Height];
        
        damageMapGenerator = new DamageMapGenerator(width, height);
    }

    internal void AddAgent(int id, int player, int shootCooldown, int optimalRange, int soakingPower, int splashBombs)
    {
        if (player == MyId)
        {
            playerAgents.Add(new Agent(id, player, shootCooldown, optimalRange, soakingPower, splashBombs));
        }
        else
        {
            opponentAgents.Add(new Agent(id, player, shootCooldown, optimalRange, soakingPower, splashBombs));
        }
    }

    internal void MarkAllAgentsForCulling()
    {
        foreach (var agent in playerAgents)
        {
            agent.InGame = false;
        }

        foreach (var agent in opponentAgents)
        {
            agent.InGame = false;
        }
    }

    internal void DestroyMarkedAgents()
    {
        playerAgents.RemoveAll(agent => !agent.InGame);
        opponentAgents.RemoveAll(agent => !agent.InGame);
    }

    internal void UpdateAgent(int agentId, int x, int y, int cooldown, int splashBombs, int wetness)
    {
        Agent agent = playerAgents.FirstOrDefault(a => a.Id == agentId) 
            ?? opponentAgents.FirstOrDefault(a => a.Id == agentId);

        if (agent != null)
        {
            agent.UpdatePosition(x, y);
            agent.ShootCooldown = cooldown;
            agent.SplashBombs = splashBombs;
            agent.Wetness = wetness;
            agent.InGame = true;
        }
    }

    private int GetWettestOpponentId()
    {
        int wettestOpponentId = -1;
        int maxWetness = -1;

        foreach (var agent in opponentAgents)
        {
            if (agent.Wetness > maxWetness)
            {
                maxWetness = agent.Wetness;
                wettestOpponentId = agent.Id;
            }
        }

        return wettestOpponentId;
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
        coverMapGenerator = new CoverMapGenerator(cover);
        coverHillMap = CoverHillMapGenerator.CreateMap(cover);

        _aStar = new AStar(cover);
    }
}
