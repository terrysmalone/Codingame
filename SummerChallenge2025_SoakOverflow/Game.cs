

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

    private CoverMap coverMap;

    public Game(int myId)
    {
        MyId = myId;

        coverMap = new CoverMap();
    }

    public void SetGameSize(int width, int height)
    {
        Width = width;
        Height = height;

        cover = new int[Width, Height];
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
            agent.InGame = true; // Mark it as still in game
        }
    }

    // One line per agent: <agentId>;<action1;action2;...> actions are "MOVE x y | SHOOT id | THROW x y | HUNKER_DOWN | MESSAGE text"
    internal List<string> GetMoves()
    {
        SplashMap splashMap = new SplashMap(Width, Height, playerAgents, opponentAgents);
        int[,] splashDamageMap = splashMap.CreateSplashMap();

        List<string> moves = new List<string>();

        foreach (var agent in playerAgents)
        {
            string move = "";

            // If we can hit with a splash bomb without moving, do that
            if (agent.SplashBombs > 0)
            {
                move = GetBestThrowMove(agent, splashDamageMap);
            }

            if (move== "")
            {
                if (agent.ShootCooldown <= 0)
                {
                    move = GetRunAndGunMove(agent);
                }
            }

            if (move == "")
            {
                // If we can't shoot or throw, we need to move
                Point closestEnemyPosition = GetClosestEnemyPosition(agent);

                var distanceToClosestEnemy = Math.Abs(agent.Position.X - closestEnemyPosition.X) 
                        + Math.Abs(agent.Position.Y - closestEnemyPosition.Y);

                if (distanceToClosestEnemy < agent.OptimalRange * 2)
                {
                    // TODO: Change this so that the agent just moves towards cover. 
                    // Cover should be more of a priority.
                    // If it gets to this stage, look in a widening circle for nearby cover. 
                    // If it finds cover, make sure the cover won't take it too far from the enemy.               
                    var bestAttackPoint = GetBestAttackPoint(agent);
                    Console.Error.WriteLine($"Best attack point for agent {agent.Id} is {bestAttackPoint.X}, {bestAttackPoint.Y}");

                    move = $"{agent.Id}; MOVE {bestAttackPoint.X} {bestAttackPoint.Y}; HUNKER_DOWN";
                }
                else
                {
                    move = $"{agent.Id}; MOVE {closestEnemyPosition.X} {closestEnemyPosition.Y}; HUNKER_DOWN";
                }
            }

            moves.Add(move);

            // To debug: Console.Error.WriteLine("Debug messages...");
        }

        return moves;
    }

    private Point GetClosestEnemyPosition(Agent agent)
    {
        Point closestEnemyPosition = new Point(-1, -1);
        int closestDistance = int.MaxValue;

        foreach (var enemy in opponentAgents)
        {
            int distance = Math.Abs(agent.Position.X - enemy.Position.X) + Math.Abs(agent.Position.Y - enemy.Position.Y);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemyPosition = enemy.Position;
            }
        }

        return closestEnemyPosition;
    }

    private string GetBestThrowMove(Agent agent, int[,] splashDamageMap)
    {
        int bestX = -1;
        int bestY = -1;
        int bestValue = 0;

        // Get highest score from splashDamageMap within 4 
        // Manhattan distance from agent's position
        int minX = Math.Max(0, agent.Position.X - 4);
        int maxX = Math.Min(Width - 1, agent.Position.X + 4);
        int minY = Math.Max(0, agent.Position.Y - 4);
        int maxY = Math.Min(Height - 1, agent.Position.Y + 4);

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                var manhattanDistance = Math.Abs(agent.Position.X - x) + Math.Abs(agent.Position.Y - y);
                if (manhattanDistance <= 4 && splashDamageMap[x, y] > bestValue)
                {
                    bestValue = splashDamageMap[x, y];
                    bestX = x;
                    bestY = y;
                }
            }
        }

        if (bestValue > 30)
        {
            return $"{agent.Id}; THROW {bestX} {bestY}";
        }

        return "";
    }

    private string GetRunAndGunMove(Agent agent)
    {
        Point attackPoint = GetBestAttackPoint(agent);

        // Get target
        var bestAttack = 0.0;
        var bestAttackId = -1;

        foreach (var enemy in opponentAgents)
        {
            var damage = CalculateDamage(
                attackPoint.X,
                attackPoint.Y,
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
            // No valid attack found, return empty move
            return "";
        }

        if (attackPoint == agent.Position)
        {
            return $"{agent.Id}; SHOOT {bestAttackId}";
        }

        return $"{agent.Id}; MOVE {attackPoint.X} {attackPoint.Y}; SHOOT {bestAttackId}";
    }

    private Point GetBestAttackPoint(Agent agent)
    {
        // get best protection (including current space)
        double stationaryReceivingDamage = CalculateReceivingPlayerDamage(agent.Position.X, agent.Position.Y);

        double northReceivingDamage = CalculateReceivingPlayerDamage(agent.Position.X, agent.Position.Y - 1);

        var minReceivingDamage = northReceivingDamage;
        var minReceivingDamagePosition = new Point(agent.Position.X, agent.Position.Y - 1);

        double southReceivingDamage = CalculateReceivingPlayerDamage(agent.Position.X, agent.Position.Y + 1);
        if (southReceivingDamage < minReceivingDamage)
        {
            minReceivingDamage = southReceivingDamage;
            minReceivingDamagePosition = new Point(agent.Position.X, agent.Position.Y + 1);
        }

        double eastReceivingDamage = CalculateReceivingPlayerDamage(agent.Position.X + 1, agent.Position.Y);
        if (eastReceivingDamage < minReceivingDamage)
        {
            minReceivingDamage = eastReceivingDamage;
            minReceivingDamagePosition = new Point(agent.Position.X + 1, agent.Position.Y);
        }

        double westReceivingDamage = CalculateReceivingPlayerDamage(agent.Position.X - 1, agent.Position.Y);
        if (westReceivingDamage < minReceivingDamage)
        {
            minReceivingDamage = westReceivingDamage;
            minReceivingDamagePosition = new Point(agent.Position.X - 1, agent.Position.Y);
        }

        var attackPoint = new Point(-1, -1);

        if (stationaryReceivingDamage <= minReceivingDamage)
        {
            // Stay in place
            attackPoint = new Point(agent.Position.X, agent.Position.Y);
        }
        else
        {
            // Move to the position with the least damage
            attackPoint = minReceivingDamagePosition;
        }

        return attackPoint;
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
        double[,] map = coverMap.CreateCoverMap(targetX, targetY, cover);
        // Display.CoverMap(map);

        var damageMultiplier = map[fromX, fromY];
        var baseDamage = soakingPower * damageMultiplier;

        int manhattanDistance = Math.Abs(targetX - fromX) + Math.Abs(targetY - fromY);

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
}
