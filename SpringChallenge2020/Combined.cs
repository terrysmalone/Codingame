/**************************************************************
  This file was generated by FileConcatenator.
  It combined all classes in the project to work in Codingame.
***************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;

internal static class Display
{
    internal static void PelletDistances(List<PelletDistance> pelletDistances)
    {
        foreach (PelletDistance pelletDistance in pelletDistances)
        {
            Console.Error.WriteLine($"Pellet ({pelletDistance.Position.X}, {pelletDistance.Position.Y}): [{string.Join(" ", pelletDistance.Distances)}]");
        }
    }
}

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

        bool[] playerTargetSet = new bool[PlayerPacs.Count];

        // ------------------------

        List<PelletDistance> pelletDistances = CalculatePelletDistances(superPellets).OrderBy(p => p.Distances.Min()).ToList();

        Display.PelletDistances(pelletDistances);
        Console.Error.WriteLine("----------------------------");

        int superPelletsTargeted = 0;

        while (!AreAllTargeted(playerTargetSet) && superPelletsTargeted < superPellets.Count)
        {
            PelletDistance pelletDistance = pelletDistances[0];

            int pacIndex = 0;

            double[] distances = pelletDistance.Distances;

            for (int i = 0; i < distances.Length; i++)
            {
                if (distances[i] < distances[pacIndex])
                    pacIndex = i;
            }

            commands.Add($"MOVE {PlayerPacs[pacIndex].Id} {pelletDistance.Position.X} {pelletDistance.Position.Y}");

            playerTargetSet[pacIndex] = true;
            superPelletsTargeted++;

            pelletDistances.RemoveAt(0);

            MaxOutAtIndex(pelletDistances, pacIndex);

            pelletDistances = pelletDistances.OrderBy(p => p.Distances.Min()).ToList();
            Display.PelletDistances(pelletDistances);
            Console.Error.WriteLine("----------------------------");
        }

        // Now find close standard pellets for other pacs
        List<Pellet> standardPellets = Pellets.Where(p => p.Value == pelletValue).ToList();
        Console.Error.WriteLine($"standardPellets.Count: {standardPellets.Count}");

        bool[] standardPelletTargeted = new bool[standardPellets.Count];

        for (int i = 0; i < PlayerPacs.Count; i++)
        {
            if (playerTargetSet[i])
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
        }
        

        // This is an emergency. Send them to the start
        if (commands.Count == 0)
        {
            foreach (Pac playerPac in PlayerPacs)
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

    private List<PelletDistance> CalculatePelletDistances(List<Pellet> superPellets)
    {
        List<PelletDistance> pelletDistances = new List<PelletDistance>();

        foreach (Pellet superPellet in superPellets)
        {
            double [] distances = new double[PlayerPacs.Count];

            for(int i = 0;i < PlayerPacs.Count;i++)
            {
                Pac pac = PlayerPacs[i];

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

internal struct Pac
{
    internal int Id;
    internal Point Position;

    public Pac(int id, Point position)
    {
        Id = id;
        Position = position;
    }
}


internal struct Pellet(Point position, int value)
{
    internal Point Position = position;

    internal int Value = value;
}


internal struct PelletDistance(Point position, double[] distances)
{
    internal Point Position = position;
 
    internal double[] Distances = distances;
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
                    playerPacs.Add(new Pac(pacId, new Point(x, y)));
                }
                else
                {
                    opponentPacs.Add(new Pac(pacId, new Point(x, y)));
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

                pellets.Add(new Pellet(new Point(x, y), value)); 
            }

            game.SetPellets(pellets);

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");

            string command = game.GetCommand();

            Console.WriteLine(command);          
        }
    }
}

