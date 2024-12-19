/**************************************************************
  This file was generated by FileConcatenator.
  It combined all classes in the project to work in Codingame.
***************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Diagnostics;
using System.Net;

internal static class Display
{
    internal static void Summary(Game game)
    {
        Console.Error.WriteLine($"Proteins");
        Proteins(game.Proteins);
        Console.Error.WriteLine("==================================");

        Console.Error.WriteLine($"Protein stock");
        Console.Error.WriteLine("----------------------------------");
        Console.Error.WriteLine($"Player protein stock");
        ProteinStock(game.PlayerProteinStock);
        Console.Error.WriteLine("----------------------------------");
        Console.Error.WriteLine($"Opponent protein stock");
        ProteinStock(game.OpponentProteinStock);
        Console.Error.WriteLine("==================================");
    }

    internal static void ProteinStock(ProteinStock proteinStock)
    {
        Console.Error.WriteLine($"A: {proteinStock.A}");
        Console.Error.WriteLine($"B: {proteinStock.B}");
        Console.Error.WriteLine($"C: {proteinStock.C}");
        Console.Error.WriteLine($"D: {proteinStock.D}");
    }

    internal static void Proteins(List<Protein> proteins)
    {
        Console.Error.WriteLine($"Proteins");

        foreach (Protein protein in proteins)
        {
            Console.Error.WriteLine($"Type:{protein.Type} - Position:({protein.Position.X},{protein.Position.Y})");
        }
    }
}


internal sealed class Game
{  

    internal Organism PlayerOrganism { get; private set; }
    internal Organism OpponentOrganism { get; private set; }

    internal ProteinStock PlayerProteinStock { get; private set; }
    internal ProteinStock OpponentProteinStock { get; private set; }
    
    public List<Point> Walls { get; private set; }
    public List<Protein> Proteins { get; private set; }

    private int _width;
    private int _height;

    internal Game(int width, int height)
    {
        _width = width;
        _height = height;
    }

    internal void SetPlayerProteinStock(ProteinStock playerProteins)
    {
        PlayerProteinStock = playerProteins;
    }

    internal void SetOpponentProteinStock(ProteinStock opponentProteins)
    {
        OpponentProteinStock = opponentProteins;
    }

    internal void SetPlayerOrganism(Organism playerOrganism)
    {
        PlayerOrganism = playerOrganism;
    }

    internal void SetOpponentOrganism(Organism opponentOrganism)
    {
        OpponentOrganism = opponentOrganism;
    }

    internal void SetWalls(List<Point> walls)
    {
        Walls = walls;
    }

    internal void SetProteins(List<Protein> proteins)
    {
        Proteins = proteins;
    }

    internal List<string> GetActions()
    {
        // First pass simple solution. Find the closest A protein.
        string action =  "WAIT";

        if (PlayerOrganism.Organs.Count == 0)
        {
            double closest = double.MaxValue;
            Point closestPoint = new Point();

            // Display.Proteins(Proteins);

            // Get the closes A protein to Root
            foreach (Protein protein in Proteins)
            {
                if (protein.Type == ProteinType.A)
                {
                    double distance = CalculateDistance(protein.Position, PlayerOrganism.Root.Position);

                    if (distance < closest)
                    {
                        closest = distance;
                        closestPoint = new Point(protein.Position.X, protein.Position.Y);    
                    }
                }
            }

            action = $"GROW {PlayerOrganism.Root.Id} {closestPoint.X} {closestPoint.Y} BASIC";
        }
        else
        {
            double closest = double.MaxValue;
            int closestId = -1;
            Point closestPoint = new Point();

            // Get the closest A protein to Organs
            foreach (Protein protein in Proteins)
            {
                if (protein.Type == ProteinType.A)
                {
                    foreach (var organ in PlayerOrganism.Organs)
                    {
                        double distance = CalculateDistance(protein.Position, organ.Position);

                        if (distance < closest)
                        {
                            closest = distance;
                            closestId = organ.Id;
                            closestPoint = new Point(protein.Position.X, protein.Position.Y);
                        }
                    } 
                }
            }

            action = $"GROW {closestId} {closestPoint.X} {closestPoint.Y} BASIC";
        }


        return new List<string>() { action };
    }

    private static double CalculateDistance(Point pointA, Point pointB)
    {
        return Math.Sqrt(Math.Pow(pointA.X - pointB.X, 2) + Math.Pow(pointA.Y - pointB.Y, 2));
    }
}


internal struct Organ
{
    internal int Id { get; private set; }

    internal Point Position { get; private set; }

    public Organ(int id, Point position) : this()
    {
        Id = id;
        Position = position;
    }
}

internal struct Organism
{
    internal Organ Root { get; private set; }
    internal List<Organ> Organs { get; private set; }

    public Organism()
    {
        Organs = new List<Organ>();
    }

    internal void SetRoot(int id, Point root)
    {
        Root = new Organ(id, root);
    }

    internal readonly void AddOrgan(int organId, Point point)
    {
        Organs.Add(new Organ(organId, point));
    }
}


partial class Player
{
    static void Main(string[] args)
    {
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        int width = int.Parse(inputs[0]); // columns in the game grid
        int height = int.Parse(inputs[1]); // rows in the game grid

        Game game = new Game(width, height);

        // game loop
        while (true)
        {
            Organism playerOrganism = new Organism();
            Organism opponentOrganism = new Organism();
            List<Point> walls = new List<Point>();
            List<Protein> proteins = new List<Protein>();

            int entityCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < entityCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int x = int.Parse(inputs[0]);
                int y = int.Parse(inputs[1]); // grid coordinate
                string type = inputs[2]; // WALL, ROOT, BASIC, TENTACLE, HARVESTER, SPORER, A, B, C, D
                int owner = int.Parse(inputs[3]); // 1 if your organ, 0 if enemy organ, -1 if neither
                int organId = int.Parse(inputs[4]); // id of this entity if it's an organ, 0 otherwise
                string organDir = inputs[5]; // N,E,S,W or X if not an organ
                int organParentId = int.Parse(inputs[6]);
                int organRootId = int.Parse(inputs[7]);

                switch (type)
                {
                    case "WALL":
                        walls.Add(new Point(x, y));
                        break;
                    case "ROOT":
                        if (owner == 1)
                        {
                            playerOrganism.SetRoot(organId, new Point(x, y));
                        } 
                        else if (owner == 0)
                        {
                            opponentOrganism.SetRoot(organId, new Point(x, y));
                        }
                        break;
                    case "BASIC":
                        if (owner == 1)
                        {
                            playerOrganism.AddOrgan(organId, new Point(x, y));
                        }
                        else if (owner == 0)
                        {
                            opponentOrganism.AddOrgan(organId, new Point(x, y));
                        }
                        break;
                    case "A":
                        proteins.Add(new Protein(ProteinType.A, new Point(x, y)));
                        break;
                    case "B":
                        proteins.Add(new Protein(ProteinType.B, new Point(x, y)));
                        break;
                    case "C":
                        proteins.Add(new Protein(ProteinType.C, new Point(x, y)));
                        break;
                    case "D":
                        proteins.Add(new Protein(ProteinType.D, new Point(x, y)));
                        break;
                }
            }

            game.SetPlayerOrganism(playerOrganism);
            game.SetOpponentOrganism(opponentOrganism);

            game.SetWalls(walls);
            game.SetProteins(proteins);

            ProteinStock playerProteins = GetProteins();
            game.SetPlayerProteinStock(playerProteins);

            ProteinStock opponentProteins = GetProteins();
            game.SetOpponentProteinStock(opponentProteins);

            List<string> actions = game.GetActions();

            int requiredActionsCount = int.Parse(Console.ReadLine()); // your number of organisms, output an action for each one in any order
            for (int i = 0; i < requiredActionsCount; i++)
            {
                Console.WriteLine(actions[i]);

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");

                // Console.WriteLine("WAIT");
            }
        }
    }

    private static ProteinStock GetProteins()
    {
        string[] inputs = Console.ReadLine().Split(' ');
        int proteinA = int.Parse(inputs[0]);
        int proteinB = int.Parse(inputs[1]);
        int proteinC = int.Parse(inputs[2]);
        int proteinD = int.Parse(inputs[3]);

        ProteinStock proteins = new ProteinStock(proteinA, proteinB, proteinC, proteinD);

        return proteins;
    }
}

internal struct Protein
{
    internal ProteinType Type { get; private set; }
    internal Point Position { get; private set; }


    internal Protein(ProteinType type, Point position)
    {
        Type = type;
        Position = position;
    }
}


internal struct ProteinStock(int a, int b, int c, int d)
{
    public int A { get; private set; } = a;
    public int B { get; private set; } = b;
    public int C { get; private set; } = c;
    public int D { get; private set; } = d;
}

internal enum ProteinType
{
    A,
    B,
    C,
    D
}

