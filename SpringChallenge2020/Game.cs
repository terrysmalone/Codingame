namespace SpringChallenge2020; 

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
internal class Game
{
    private const int pelletValue = 1;
    private const int superPelletValue = 10;
    
    public List<Pac> PlayerPacs { get; private set; }
    
    public List<Pac> OpponentPacs { get; private set; }

    public List<Pellet> Pellets { get; private set; }

    internal void SetPlayerPacs(List<Pac> playerPacs) => PlayerPacs = playerPacs;

    internal void SetOpponentPacs(List<Pac> opponentPacs) => OpponentPacs = opponentPacs;

    internal void SetPellets(List<Pellet> pellets) => Pellets = pellets;

    // MOVE <pacId> <x> <y> | MOVE <pacId> <x> <y>
    internal string GetCommand()
    {
        List<string> commands = new List<string>();

        List<Pellet> superPellets = Pellets.Where(p => p.Value == superPelletValue).ToList();
        bool[] superPelletTargeted = new bool[superPellets.Count];

        List<Pellet> standardPellets = Pellets.Where(p => p.Value == pelletValue).ToList();
        bool[] standardPelletTargeted = new bool[standardPellets.Count]; 

        // TODO: I should really get the closest Pac to each super pellet rather than the closest super 
        // pellet to each pac
        foreach (Pac pac in PlayerPacs) 
        {        
            if (!AreAllTargeted(superPelletTargeted))
            {
                // Get a super pellet
                int index = GetClosestPelletIndex(pac, superPellets, superPelletTargeted);

                superPelletTargeted[index] = true;

                commands.Add($"MOVE {pac.Id} {superPellets[index].Position.X} {superPellets[index].Position.Y}");
            }
            else
            {
                // Get closest standard pellet
                int index = GetClosestPelletIndex(pac, standardPellets, standardPelletTargeted);

                standardPelletTargeted[index] = true;

                commands.Add($"MOVE {pac.Id} {standardPellets[index].Position.X} {standardPellets[index].Position.Y}");
            }
        }

        string command = string.Join(" | ", commands);

        return command;    
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

    private int GetClosestPelletIndex(Pac pac, List<Pellet> pellets, bool[] targeted)
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