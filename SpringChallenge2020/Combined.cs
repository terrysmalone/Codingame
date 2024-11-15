/**************************************************************
  This file was generated by FileConcatenator.
  It combined all classes in the project to work in Codingame.
***************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;

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

internal struct Pac
{
    internal int Id;
    internal int X;
    internal int Y;

    internal int Value;

    public Pac(int id, int x, int y)
    {
        Id = id;
        X = x;
        Y = y;
    }
}


internal struct Pellet
{
    internal int X;
    internal int Y;

    internal int Value;

    public Pellet(int x, int y, int value)
    {
        X = x;
        Y = y;
        Value = value;
    }
}

class Player
{
    static void Main(string[] args)
    {
        Game game = new Game();

        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        int width = int.Parse(inputs[0]); // size of the grid
        int height = int.Parse(inputs[1]); // top left corner is (x=0, y=0)
        for (int i = 0; i < height; i++)
        {
            string row = Console.ReadLine(); // one line of the grid: space " " is floor, pound "#" is wall
        }

        // game loop
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            int myScore = int.Parse(inputs[0]);
            int opponentScore = int.Parse(inputs[1]);


            // Get the Pacs
            List<Pac> playerPacs = new List<Pac>();
            List<Pac> opponentPacs = new List<Pac>();

            int visiblePacCount = int.Parse(Console.ReadLine()); // all your pacs and enemy pacs in sight
            for (int i = 0; i < visiblePacCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int pacId = int.Parse(inputs[0]); // pac number (unique within a team)
                bool mine = inputs[1] != "0"; // true if this pac is yours
                int x = int.Parse(inputs[2]); // position in the grid
                int y = int.Parse(inputs[3]); // position in the grid
                string typeId = inputs[4]; // unused in wood leagues
                int speedTurnsLeft = int.Parse(inputs[5]); // unused in wood leagues
                int abilityCooldown = int.Parse(inputs[6]); // unused in wood leagues

                if (mine)
                {
                    playerPacs.Add(new Pac(pacId, x, y));
                }
                else
                {
                    opponentPacs.Add(new Pac(pacId, x, y));
                }
            }

            game.SetPlayerPacs(playerPacs);
            game.SetOpponentPacs(opponentPacs);

            // Get the pellets
            List<Pellet> pellets = new List<Pellet>();

            int visiblePelletCount = int.Parse(Console.ReadLine()); // all pellets in sight
            for (int i = 0; i < visiblePelletCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int x = int.Parse(inputs[0]);
                int y = int.Parse(inputs[1]);
                int value = int.Parse(inputs[2]); // amount of points this pellet is worth

                pellets.Add(new Pellet(x, y, value)); 
            }

            game.SetPellets(pellets);

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");

            string command = game.GetCommand();

            Console.WriteLine(command);          
        }
    }
}

