

using System.Drawing;
using System.Runtime.CompilerServices;

namespace SummerChallenge2025_SoakOverflow;

class Game
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

    public Game(int myId)
    {
        MyId = myId;
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

    // One line per agent: <agentId>;<action1;action2;...> actions are "MOVE x y | SHOOT id | THROW x y | HUNKER_DOWN | MESSAGE text"
    internal List<string> GetMoves()
    {
        int[,] splashMap = CreateSplashMap();

        // Creates a map for each agent, player and opponent, showing how much cover 
        // they have from being attacked from any cell
        Dictionary<int, double[,]> coverMaps = CreateCoverMaps();

        List<string> moves = new List<string>();

        foreach (var agent in playerAgents)
        {
            string move = $"{agent.Id}; ";

            // Calculate Agent attack map - How much damage he can do at every position on the map
            // based on current enemy positions
            double[,] agentDamageMap = damageMapGenerator.CreateDamageMap(agent, opponentAgents, splashMap, coverMaps, cover);

            // If the nearest enemy is more than two times the agent's optimal range away
            // then we should move towards the nearest high damage spot
            (_, var closestEnemyDistance) = GetClosestEnemyPosition(agent);

            // If closestEnemyDistance > agent.OptimalRange * 2 we want to move to the best shoot point and then hunker down
            // If closestEnemyDistance > agent.OptimalRange we want to move to the best shoot point and then see if we can attack
            // else we want to move to the best cover point and then see if we can attack

            var nextMove = new Point(-1, -1);

            if (closestEnemyDistance > agent.OptimalRange)
            {
                (Point bestAttackPoint, _) = ClosestPeakFinder.FindClosestPeak(
                    agent.Position,
                    agentDamageMap);

                    move += $"MOVE {bestAttackPoint.X} {bestAttackPoint.Y}; ";
                    nextMove = bestAttackPoint;
                    Console.Error.WriteLine($"Agent {agent.Id} move source - further than optimal range");
            }

            if (nextMove == new Point(-1, -1))
            {
                (var coverMove, nextMove) = GetClosestCoverMove(agent, coverHillMap);
                move += coverMove;
            }


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
            

            moves.Add(move);

            // To debug: Console.Error.WriteLine("Debug messages...");
        }

        return moves;
    }

    private (string, Point) GetClosestCoverMove(Agent agent, int[,] coverHillMap)
    {
        Console.Error.WriteLine($"Agent {agent.Id} cover hill map");
        Display.Map(coverHillMap);

        var move = new Point(-1, -1);

        (var closestEnemyPosition, var closestEnemyDistance) = GetClosestEnemyPosition(agent);
        Console.Error.WriteLine($"Current cover score: {coverHillMap[agent.Position.X, agent.Position.Y]}");

        // Get range to check
        int minX = Math.Max(0, agent.Position.X - 1);
        int maxX = Math.Min(Width - 1, agent.Position.X + 1);
        int minY = Math.Max(0, agent.Position.Y - 1);
        int maxY = Math.Min(Height - 1, agent.Position.Y + 1);

        // We want to check closer to the opponent
        if (agent.Position.X < closestEnemyPosition.X)
        {
            minX = (Math.Max(minX, agent.Position.X));
        }
        else if (agent.Position.X > closestEnemyPosition.X)
        {
            maxX = (Math.Min(maxX, agent.Position.X));
        }

        if (agent.Position.Y < closestEnemyPosition.Y)
        {
            minY = (Math.Max(minY, agent.Position.Y));
        }
        else if (agent.Position.Y > closestEnemyPosition.Y)
        {
            maxY = (Math.Min(maxY, agent.Position.Y));
        }

        double max = 0.0;

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                if (coverHillMap[x, y] > max)
                {
                    max = coverHillMap[x, y];
                    move = new Point(x, y);
                }
            }
        }

        if (move != agent.Position)
        {
            Console.Error.WriteLine($"Target cover score: {coverHillMap[move.X, move.Y]}");


            Console.Error.WriteLine($"Agent {agent.Id} move source - close - looking for cover");
            return ($"MOVE {move.X} {move.Y}; ", move);
        }

        return ("", move);
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

    private string GetThrowAction(Agent agent, Point movePoint, int[,] splashDamageMap)
    {
        int bestX = -1;
        int bestY = -1;
        int bestValue = 0;

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
                    Console.Error.WriteLine($"Skipping throw action at {x}, {y} because it's too close to move point {movePoint.X}, {movePoint.Y}");
                    continue;
                }

                var manhattanDistance = CalculationUtil.GetManhattanDistance(
                    agent.Position, new Point(x, y));

                
                    
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

    private double CalculateReceivingPlayerDamage(int x, int y)
    {
        // Check bounds
        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return Double.MaxValue;
        }

        var stationaryReceivingDamage = 0.0;
        foreach (var enemy in opponentAgents)
        {
            if (enemy.ShootCooldown <= 0)
            {
                stationaryReceivingDamage += CalculateDamage(
                    enemy.Position.X,
                    enemy.Position.Y,
                    enemy.OptimalRange,
                    enemy.SoakingPower,
                    x,
                    y);
            }
        }

        return stationaryReceivingDamage;
    }

    private double CalculateDamage(int fromX, int fromY, int optimalRange, int soakingPower, int targetX, int targetY)
    {
        double[,] map = coverMapGenerator.CreateCoverMap(targetX, targetY);
        // Display.CoverMap(map);

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
    }
}
