namespace SpringChallenge2020; 

using System;
using System.Collections.Generic;
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

        if (superPellets.Count <= PlayerPacs.Count)
        {
            for (int i = 0; i < superPellets.Count; i++)
            {
                commands.Add($"MOVE {PlayerPacs[i].Id} {superPellets[i].X} {superPellets[i].Y}");
            }

            List<Pellet> standardPellets = Pellets.Where(p => p.Value == pelletValue).ToList();

            for (int i = superPellets.Count; i < PlayerPacs.Count; i++)
            {

                commands.Add($"MOVE {PlayerPacs[i].Id} {standardPellets[i].X} {standardPellets[i].Y}");
            }
        }
        else
        {
            for (int i = 0; i < PlayerPacs.Count; i++)
            {
                commands.Add($"MOVE {PlayerPacs[i].Id} {superPellets[i].X} {superPellets[i].Y}");
            }
        }

        string command = string.Join(" | ", commands);

        return command;    
    }
}