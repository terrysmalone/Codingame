

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
                move = GetThrowMove(agent, splashDamageMap);
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
                move = $"{agent.Id}; MOVE {closestEnemyPosition.X} {closestEnemyPosition.Y}; HUNKER_DOWN";
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

    private string GetThrowMove(Agent agent, int[,] splashDamageMap)
    {
        // Get position of highest value in splashDamageMap
        int bestX = -1;
        int bestY = -1;
        int bestValue = -1;

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (splashDamageMap[x, y] > bestValue)
                {
                    bestValue = splashDamageMap[x, y];
                    bestX = x;
                    bestY = y;
                }
            }
        }

        // Get distance from bestX, bestY to agent's position
        int distance = Math.Abs(agent.Position.X - bestX) + Math.Abs(agent.Position.Y - bestY);

        //if (distance > 4)
        //{
        //    // If the distance is greater than 4, we need to move closer
        //    var moveX = agent.Position.X < bestX ? agent.Position.X + 1 : agent.Position.X - 1;
        //    var moveY = agent.Position.Y < bestY ? agent.Position.Y + 1 : agent.Position.Y - 1;
        //    return $"{agent.Id}; THROW {bestX} {bestY}";
        //}
        if (distance <= 4)
        {
            // If we are close enough, throw the splash bomb
            return $"{agent.Id}; THROW {bestX} {bestY}";
        }

        return "";
    }

    private string GetRunAndGunMove(Agent agent)
    {
        // Get highest adjacent protection
        Point bestProtection = GetBestAdjacentProtection(agent.Position);

        var bestAttack = 0.0;
        var bestAttackId = -1;

        foreach (var enemy in opponentAgents)
        {
            var damage = CalculateDamage(
                bestProtection.X,
                bestProtection.Y,
                agent.OptimalRange,
                agent.SoakingPower,
                enemy);

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

        return $"{agent.Id}; MOVE {bestProtection.X} {bestProtection.Y}; SHOOT {bestAttackId}";
    }

    private double CalculateDamage(int x, int y, int optimalRange, int soakingPower, Agent enemy)
    {
        double[,] map = coverMap.CreateCoverMap(enemy.Position.X, enemy.Position.Y, cover);

        var damageMultiplier = map[x, y];
        var baseDamage = soakingPower * damageMultiplier;

        int manhattanDistance = Math.Abs(enemy.Position.X - x) + Math.Abs(enemy.Position.Y - y);

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

    // Get the compass pooint square (N,E,S,W) with the best cover
    private Point GetBestAdjacentProtection(Point position)
    {
        var north = GetHighestCover(position.X, position.Y - 1);

        var best = north;
        var bestX = position.X;
        var bestY = position.Y - 1;

        var south = GetHighestCover(position.X, position.Y + 1);

        if (south > best)
        {
            best = south;
            bestX = position.X;
            bestY = position.Y + 1;
        }

        var east = GetHighestCover(position.X + 1, position.Y);

        if (east > best)
        {
            best = east;
            bestX = position.X + 1;
            bestY = position.Y;
        }

        var west = GetHighestCover(position.X - 1, position.Y);

        if (west > best)
        {
            best = west;
            bestX = position.X - 1;
            bestY = position.Y;
        }

        return new Point(bestX, bestY);
    }

    private int GetHighestCover(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            // Out of bounds
            return -1; 
        }

        var best = 0;

        // Check North
        if (y - 1 >= 0 && cover[x, y - 1] > best)
        {
            best = cover[x, y - 1];
        }

        // Check South
        if (y + 1 < Height && cover[x, y + 1] > best)
        {
            best = cover[x, y + 1];
        }

        // Check east
        if (x + 1 < Width && cover[x + 1, y] > best)
        {
            best = cover[x + 1, y];
        }

        // Check west   
        if (x - 1 >= 0 && cover[x - 1, y] > best)
        {
            best = cover[x - 1, y];
        }

        return best;
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
