
namespace SummerChallenge2025_SoakOverflow;

class Game
{
    public int Width { get; private set; }
    public int Height { get; private set; }

    public int MyId { get; private set; }

    List<Agent> playerAgents = new List<Agent>();
    List<Agent> opponentAgents = new List<Agent>();

    public Game(int myId)
    {
        MyId = myId;
    }

    public void SetGameSize(int width, int height)
    {
        Width = width;
        Height = height;
    }

    internal void AddAgent(int id, int player, int shootCooldown, int optimalRange, int soakingPower, int splashBombs)
    {
        if (player == MyId)
        {
            Console.Error.WriteLine($"Adding player agent {id} with cooldown {shootCooldown}, range {optimalRange}, soaking power {soakingPower}, splash bombs {splashBombs}");
            playerAgents.Add(new Agent(id, player, shootCooldown, optimalRange, soakingPower, splashBombs));
        }
        else
        {
            Console.Error.WriteLine($"Adding opponent agent {id} with cooldown {shootCooldown}, range {optimalRange}, soaking power {soakingPower}, splash bombs {splashBombs}");
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
        Console.Error.WriteLine("Destroying marked agents...");
        Console.Error.WriteLine($"Player agents before culling: {playerAgents.Count}");
        Console.Error.WriteLine($"Opponent agents before culling: {opponentAgents.Count}");
        playerAgents.RemoveAll(agent => !agent.InGame);
        opponentAgents.RemoveAll(agent => !agent.InGame);
        Console.Error.WriteLine($"Player agents after culling: {playerAgents.Count}");
        Console.Error.WriteLine($"Opponent agents after culling: {opponentAgents.Count}");
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
        Console.Error.WriteLine($"Player agent count: {playerAgents.Count}");
        Console.Error.WriteLine($"Opponent agent count: {opponentAgents.Count}");

        Console.Error.WriteLine("In get moves");
        List<string> moves = new List<string>();

        int wettestOpponent = GetWettestOpponentId();

        foreach (var agent in playerAgents)
        {
            // Example action: Move towards a fixed position
            if (agent.Id == playerAgents[0].Id)
            {
                moves.Add($"{agent.Id};SHOOT {wettestOpponent}");
            }
            else
            {
                moves.Add($"{agent.Id};SHOOT {wettestOpponent}");
            }

            // To debug: Console.Error.WriteLine("Debug messages...");
        }

        return moves;
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
}