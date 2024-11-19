namespace SpringChallenge2020;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
internal class Game
{
    private const int pelletValue = 1;
    private const int superPelletValue = 10;

    private Point startPos = new Point(-1, -1);
    
    public List<Pac> PlayerPacs { get; private set; }
    
    public List<Pac> OpponentPacs { get; private set; }

    public List<Pellet> Pellets { get; private set; }

    internal void SetPlayerPacs(List<Pac> playerPacs) => PlayerPacs = playerPacs;

    internal void SetOpponentPacs(List<Pac> opponentPacs) => OpponentPacs = opponentPacs;

    internal void SetPellets(List<Pellet> pellets) => Pellets = pellets;

    // MOVE <pacId> <x> <y> | MOVE <pacId> <x> <y>
    internal string GetCommand()
    {
        // Save for emergencies
        if (startPos.X == -1)
        {
            startPos = new Point(PlayerPacs[0].Position.X, PlayerPacs[0].Position.Y);
        }

        List<string> commands = new List<string>();

        List<Pellet> superPellets = Pellets.Where(p => p.Value == superPelletValue).ToList();

        // -----------------------
        // Prioritise Scissors paper stone
        foreach (Pac pac in PlayerPacs)
        {
            if (pac.AbilityCooldown > 0)
                break;

            foreach (Pac opponentPac in OpponentPacs)
            {
                if (GetDistance(pac.Position, opponentPac.Position) <= 2.0)
                {
                    string switchTo = string.Empty;

                    switch (opponentPac.TypeId)
                    {
                        case "ROCK":
                            switchTo = "PAPER";
                            break;
                        case "PAPER":
                            switchTo = "SCISSORS";
                            break;
                        case "SCISSORS":
                            switchTo = "ROCK";
                            break;
                        default:
                            break;
                    }

                    if (switchTo == string.Empty || switchTo == pac.TypeId)
                        break;

                    commands.Add($"SWITCH {pac.Id} {switchTo}");
                    pac.TargetSet = true;

                    break;
                }
            }
        }

        // ------------------------

        foreach (Pac pac in PlayerPacs)
        {
            if (pac.TargetSet)
                continue;

            if (pac.AbilityCooldown == 0)
            {
                commands.Add($"SPEED {pac.Id}");
                pac.TargetSet = true;
            }
        }

        // -------------------------

        List<PelletDistance> pelletDistances = CalculatePelletDistances(superPellets, PlayerPacs).OrderBy(p => p.Distances.Min()).ToList();

        int superPelletsTargeted = 0;

        while (!AreAllTargeted(PlayerPacs) && superPelletsTargeted < superPellets.Count)
        {
            PelletDistance pelletDistance = pelletDistances[0];

            int pacIndex = 0;

            double[] distances = pelletDistance.Distances;

            for (int i = 0; i < distances.Length; i++)
            {
                if (distances[i] < distances[pacIndex])
                    pacIndex = i;
            }

            int pacId = PlayerPacs[pacIndex].Id;

            commands.Add($"MOVE {pacId} {pelletDistance.Position.X} {pelletDistance.Position.Y}");

            PlayerPacs[pacIndex].TargetSet = true;
            superPelletsTargeted++;

            pelletDistances.RemoveAt(0);

            MaxOutAtIndex(pelletDistances, pacIndex);

            pelletDistances = pelletDistances.OrderBy(p => p.Distances.Min()).ToList();
        }

        // Now find close standard pellets for other pacs
        List<Pellet> standardPellets = Pellets.Where(p => p.Value == pelletValue).ToList();
        
        bool[] standardPelletTargeted = new bool[standardPellets.Count];

        if (standardPellets.Count > 0)
        {
            for (int i = 0; i < PlayerPacs.Count; i++)
            {
                if (PlayerPacs[i].TargetSet)
                    continue;

                Pac currentPac = PlayerPacs[i];

                // Get closest standard pellet
                int index = GetClosestPelletIndex(currentPac, standardPellets, standardPelletTargeted);

                // If we can see less pellets than players then let players head to the same place...for now
                if (standardPellets.Count >= PlayerPacs.Count)
                {
                    standardPelletTargeted[index] = true;
                }

                commands.Add($"MOVE {currentPac.Id} {standardPellets[index].Position.X} {standardPellets[index].Position.Y}");
                currentPac.TargetSet = true;
            }
        }

        // If a player hasn't been given a move send them to the start
        foreach (Pac playerPac in PlayerPacs)
        {
            if (!playerPac.TargetSet)
            {
                commands.Add($"MOVE {playerPac.Id} {startPos.X} {startPos.Y}");
            }
        }        

        string command = string.Join(" | ", commands);

        return command;    
    }

    private static void MaxOutAtIndex(List<PelletDistance> pelletDistances, int pacIndex)
    {
        foreach (PelletDistance pelletDistance in pelletDistances) 
        {
            pelletDistance.Distances[pacIndex] = double.MaxValue;
        }
    }

    private List<PelletDistance> CalculatePelletDistances(List<Pellet> superPellets, List<Pac> playerPacs)
    {
        List<PelletDistance> pelletDistances = new List<PelletDistance>();

        foreach (Pellet superPellet in superPellets)
        {
            double [] distances = new double[PlayerPacs.Count];

            for(int i = 0;i < PlayerPacs.Count;i++)
            {
                Pac pac = PlayerPacs[i];

                if (pac.TargetSet)
                    distances[i] = double.MaxValue;
                else
                    distances[i] = GetDistance(superPellet.Position, pac.Position);
            }           

            pelletDistances.Add(new PelletDistance(superPellet.Position, distances));
        }

        return pelletDistances;
    }

    private static bool AreAllTargeted(bool[] targeted)
    {
        for (int i = 0; i < targeted.Length; i++)
        {
            if (!targeted[i])
            {
                return false;
            }
        }

        return true;
    }

    private static bool AreAllTargeted(List<Pac> playerPacs)
    {
        foreach (Pac pac in playerPacs)
        {
            if (!pac.TargetSet)
            {
                return false;
            }
        }
        
        return true;
    }

    private static int GetClosestPelletIndex(Pac pac, List<Pellet> pellets, bool[] targeted)
    {
        double closestDistance = double.MaxValue;
        int closestPellet = -1;

        for (int i = 0; i < pellets.Count; i++)
        {
            if (!targeted[i])
            {
                double distance = GetDistance(pac.Position, pellets[i].Position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPellet = i;
                }
            }
        }

        return closestPellet;
    }

    private static double GetDistance(Point position1, Point position2)
    {
        return Math.Sqrt(Math.Pow((position1.X - position2.X), 2) + Math.Pow((position1.Y - position2.Y), 2));

    }
}