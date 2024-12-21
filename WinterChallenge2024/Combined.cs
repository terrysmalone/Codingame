/**************************************************************
  This file was generated by FileConcatenator.
  It combined all classes in the project to work in Codingame.
***************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Threading.Channels;
using static System.Collections.Specialized.BitVector32;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;

internal sealed partial class AStar
{
    private readonly Game _game;

    private List<Node> _nodes = new List<Node>();

    internal AStar(Game game)
    {
        _game = game;
    }

    internal List<Point> GetShortestPath(Point startPoint, Point targetPoint)
    {
        //if (targetPoint.Y == 5)
        //{
        //    Console.Error.WriteLine("------------------------------------");
        //    Console.Error.WriteLine($"From ({startPoint.X},{startPoint.Y}) to ({targetPoint.X},{targetPoint.Y})");
        //    Console.Error.WriteLine("-------------------------------------");
        //}
        _nodes = new List<Node>();

        // Create a node for the start Point
        Node currentNode = new Node(startPoint);
        _nodes.Add(currentNode);

        bool targetFound = false;

        // while target not hit
        while (!targetFound) 
        {
            //if (targetPoint.Y == 5)
            //{
                //Console.Error.WriteLine("-------------------------------------");
                //Console.Error.WriteLine($"CurrentNode:({currentNode.Position.X},{currentNode.Position.Y})");
                //Console.Error.WriteLine("-------------------------------------");
                //Display.Nodes(_nodes);
            //}

            Point[] pointsToCheck = new Point[4];

            pointsToCheck[0] = new Point(currentNode.Position.X, currentNode.Position.Y+1);
            pointsToCheck[1] = new Point(currentNode.Position.X+1, currentNode.Position.Y);
            pointsToCheck[2] = new Point(currentNode.Position.X, currentNode.Position.Y - 1);
            pointsToCheck[3] = new Point(currentNode.Position.X - 1, currentNode.Position.Y);

            bool somethingChecked = false;

            // for each adjacent square
            foreach (Point pointToCheck in pointsToCheck)
            {
                Node? existingNode = _nodes.SingleOrDefault(n => n.Position == pointToCheck);

                // If a node doesnt exists  
                if (existingNode == null)
                {
                    // Create a node if the position is walkable (No wall. No harvested protein)
                    if (IsWalkable(pointToCheck))
                    {
                        Node node = new Node(pointToCheck);

                        node.Parent = currentNode.Position;

                        // Calculate F = G + H
                        node.G = currentNode.G + 1;
                        node.H = (Math.Abs(targetPoint.X - pointToCheck.X) + Math.Abs(targetPoint.Y - pointToCheck.Y));
                        node.F = node.G + node.F;

                        _nodes.Add(node);
                        somethingChecked = true;
                    }
                }
                else
                {
                    if (!existingNode.Closed)
                    {
                        int g = currentNode.G + 1;

                        if (g < existingNode.G)
                        {
                            existingNode.G = g;
                            existingNode.F = existingNode.G = existingNode.H;

                            existingNode.Parent = currentNode.Position;
                        }

                        somethingChecked = true;
                    }
                }
            }

            if (_nodes.Count(n => n.Closed == false) == 0)
            {
                return new List<Point>();
            }

            // Close the current square
            currentNode.Closed = true;

            //    If this is target 
            if (currentNode.Position == targetPoint)
            {
                targetFound = true;
            }
            else
            {
                // Sort nodes
                _nodes =  _nodes.OrderBy(n => n.Closed == true).ThenBy(n => n.F).ToList();

                currentNode = _nodes.First();
            }
        }

        int numberOfSteps = currentNode.G;

        List<Point> shortestPath = new List<Point>();
        shortestPath.Add(currentNode.Position);

        bool atStart = false;

        while (!atStart)
        {
            currentNode = _nodes.Single(n => n.Position == currentNode.Parent);

            if (currentNode.Position == startPoint)
            {
                atStart = true;
            }
            else
            {
                shortestPath.Insert(0, currentNode.Position);
            }
        }

        return shortestPath;
    }

    private bool IsWalkable(Point pointToCheck)
    {
        // Not walkable if player organ on that spot
        if (_game.PlayerOrganism.Organs.Any(o => o.Position == pointToCheck))
        {
            return false;
        }

        // Not walkable if opponent organ on that spot
        if (_game.OpponentOrganism.Organs.Any(o => o.Position == pointToCheck))
        {
            return false;
        }

        // Not walkable player harvested protein on that spot
        if (_game.Proteins.Any(p => p.IsHarvested && p.Position == pointToCheck))
        {
            return false;
        }

        // Not walkable if wall on that spot
        if (_game.Walls.Any(w => w == pointToCheck))
        {
            return false;
        }

        return true;
    }
}


internal static class Display
{
    internal static void Summary(Game game)
    {
        Console.Error.WriteLine($"PROTEINS");
        Proteins(game.Proteins);
        Console.Error.WriteLine("==================================");

        Console.Error.WriteLine($"ORGANISMS");
        Console.Error.WriteLine("----------------------------------");
        Console.Error.WriteLine($"Player organism");
        Organism(game.PlayerOrganism);
        Console.Error.WriteLine("----------------------------------");
        Console.Error.WriteLine($"Opponent organism");
        Organism(game.OpponentOrganism);
        Console.Error.WriteLine("==================================");

        Console.Error.WriteLine($"PROTEIN STOCK");
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
            Console.Error.WriteLine($"Type:{protein.Type} - Position:({protein.Position.X},{protein.Position.Y}) - BeingHarvested:{protein.IsHarvested}");
        }
    }

    internal static void Organism(Organism organism)
    {
        foreach (Organ organ in organism.Organs)
        {
            if (organ.Type == OrganType.ROOT)
            {
                Console.Error.WriteLine($" ID:{organ.Id} - Type:ROOT - Position:({organ.Position.X},{organ.Position.Y})");
            }
            if (organ.Type == OrganType.BASIC)
            {
                Console.Error.WriteLine($" ID:{organ.Id} - Type:BASIC - Position:({organ.Position.X},{organ.Position.Y})");
            }
            else if (organ.Type == OrganType.HARVESTER)
            {
                Console.Error.WriteLine($" ID:{organ.Id} - Type:HARVESTER - Position:({organ.Position.X},{organ.Position.Y}) - Direction:{organ.Direction.ToString()}");
            }
        }
    }

    internal static void Nodes(List<Node> nodes)
    {
        foreach(Node node in nodes)
        {
            Console.Error.WriteLine($"Position:({node.Position.X},{node.Position.Y}) - Closed:{node.Closed}");
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
        CheckForHarvestedProtein();

        string action = string.Empty;

        //if (CanProduceHarvester() && Proteins.Exists(p => p.Type == ProteinType.A && p.IsHarvested == false))
        //{
        //    List<Protein> harvestableProteins = Proteins.Where(p => p.Type == ProteinType.A && p.IsHarvested == false).ToList();

        //    (int closestOrgan, Point closestPoint) = HeadToNearestProtein(harvestableProteins);

        //    if (closestOrgan != -1)
        //    {
        //        action = $"GROW {closestOrgan} {closestPoint.X} {closestPoint.Y} HARVESTER";
        //    }
        //}

        //if (string.IsNullOrEmpty(action))
        //{
        //Display.Organism(PlayerOrganism);
        //Display.Organism(OpponentOrganism);
        Display.Proteins(Proteins);

        (int closestOrgan, List<Point> shortestPath) = GetShortestPathToProtein(Proteins);

        if (closestOrgan != -1)
        {
            if (CanProduceHarvester() && Proteins.Exists(p => p.Type == ProteinType.A && p.IsHarvested == false))
            {
                if (shortestPath.Count == 2)
                {
                    string dir = "N";

                    // Get direction to protein 
                    if (shortestPath[0].X < shortestPath[1].X)
                    {
                        dir = "E";
                    }
                    else if (shortestPath[0].X > shortestPath[1].X)
                    {
                        dir = "W";
                    }
                    else if (shortestPath[0].Y > shortestPath[1].Y)
                    {
                        dir = "S";
                    }

                    action = $"GROW {closestOrgan} {shortestPath[0].X} {shortestPath[0].Y} HARVESTER {dir}";
                }
            }

            if (string.IsNullOrEmpty(action))
            {
                action = $"GROW {closestOrgan} {shortestPath[0].X} {shortestPath[0].Y} BASIC";
            }
        }
        // }

        if (string.IsNullOrEmpty(action))
        {
            Console.Error.WriteLine("NO PROTEIN. Move randomly");
            // There was no protein to head to. Focus on expanding the 
            // organism

            // For now just be a bit random
            for (int i = PlayerOrganism.Organs.Count-1; i >= 0; i--)
            {
                Organ current = PlayerOrganism.Organs[i];

                Console.Error.WriteLine($"Checking {current.Id}");
                { }
                if (CanMoveTo(new Point(current.Position.X+1, current.Position.Y)))
                {
                    action = $"GROW {current.Id} {current.Position.X + 1} {current.Position.Y} BASIC";
                    break;
                }

                if (CanMoveTo(new Point(current.Position.X, current.Position.Y + 1)))
                {
                    action = $"GROW {current.Id} {current.Position.X} {current.Position.Y + 1} BASIC";
                    break;
                }

                if (CanMoveTo(new Point(current.Position.X, current.Position.Y - 1)))
                {
                    action = $"GROW {current.Id} {current.Position.X} {current.Position.Y - 1} BASIC";
                    break;
                }

                if (CanMoveTo(new Point(current.Position.X - 1, current.Position.Y)))
                {
                    action = $"GROW {current.Id} {current.Position.X - 1} {current.Position.Y} BASIC";
                    break;
                }
            }
        }

        if (string.IsNullOrEmpty(action))
        {
            action = "WAIT";
        }

        return new List<string>() { action };
    }

    // Check to see if any protein is being harvested and mark it as such
    private void CheckForHarvestedProtein()
    {
        foreach(Organ organ in PlayerOrganism.Organs)
        {
            if (organ.Type == OrganType.HARVESTER)
            {
                Console.Error.WriteLine("HARVESTER found");
                Point harvestedPosition = GetHarvestedPosition(organ);

                Protein havestedProtein = Proteins.Single(p => p.Position.X == harvestedPosition.X && p.Position.Y == harvestedPosition.Y);

                havestedProtein.IsHarvested = true;
            }
        }

        // We don't care about enemy harvested proteins because
        // we're still happy to consume them.
    }

    private static Point GetHarvestedPosition(Organ organ)
    {
        switch (organ.Direction)
        {
            case OrganDirection.N:
                return new Point(organ.Position.X, organ.Position.Y+1);
            case OrganDirection.E:
                return new Point(organ.Position.X+1, organ.Position.Y);
            case OrganDirection.S:
                return new Point(organ.Position.X, organ.Position.Y-1);
            case OrganDirection.W:
                return new Point(organ.Position.X-1, organ.Position.Y);
        }

        return new Point(-1,-1);
    }

    private bool CanProduceHarvester()
    {
        if (PlayerProteinStock.C >= 1 && PlayerProteinStock.D >= 1)
        {
            return true;
        }

        return false;
    }

    private (int,List<Point>) GetShortestPathToProtein(List<Protein> proteins)
    {
        string action = string.Empty;

        int shortest = int.MaxValue;
        int closestId = -1;
        List<Point> shortestPath = new List<Point>();

        AStar aStar = new AStar(this);

        // Get the closest A protein to Organs
        foreach (Protein protein in proteins)
        {
            if (protein.Type == ProteinType.A && !protein.IsHarvested)
            {
                foreach (var organ in PlayerOrganism.Organs)
                {
                    List<Point> path = aStar.GetShortestPath(organ.Position, protein.Position);
                    
                    if (path.Count < shortest && path.Count != 0)
                    {
                        shortest = path.Count;
                        shortestPath = new List<Point>(path);

                        closestId = organ.Id;
                    }
                }
            }
        }

        //Console.Error.WriteLine($"Shortest path is to organ {closestId} and is {shortestPath.Count} steps ");

        //if (closestId == -1)
        //{
        //    return (closestId, new Point(-1,-1));
        //}

        return (closestId, shortestPath);
    }

    private static double CalculateDistance(Point pointA, Point pointB)
    {
        return Math.Sqrt(Math.Pow(pointA.X - pointB.X, 2) + Math.Pow(pointA.Y - pointB.Y, 2));
    }

    private bool CanMoveTo(Point pointToCheck)
    {
        Console.Error.WriteLine($"Checking {pointToCheck.X}, {pointToCheck.Y}");
        // Not walkable if player organ on that spot
        if (PlayerOrganism.Organs.Any(o => o.Position == pointToCheck))
        {
            Console.Error.WriteLine("False at PlayerOrganism");
            return false;
        }

        // Not walkable if opponent organ on that spot
        if (OpponentOrganism.Organs.Any(o => o.Position == pointToCheck))
        {
            Console.Error.WriteLine("False at OpponentOrganism");
            return false;
        }

        // Not walkable player harvested protein on that spot
        if (Proteins.Any(p => p.IsHarvested && p.Position == pointToCheck))
        {
            Console.Error.WriteLine("False at Protein");

            Display.Proteins(Proteins);
            return false;
        }

        // Not walkable if wall on that spot
        if (Walls.Any(w => w == pointToCheck))
        {
            Console.Error.WriteLine("False at Wall");

            return false;
        }

        Console.Error.WriteLine("True");


        return true;
    }
}


internal sealed class Node
{
    public Point Position { get; set; }

    public Point Parent { get; set; }

    public int G { get; set; }
    public int H { get; set; }
    public int F { get; set; }

    public bool Closed { get; set; }

    public Node(Point position)
    {
            Position = position;
    }
}


internal struct Organ
{
    internal int Id { get; private set; }

    public OrganType Type { get; set; }

    internal Point Position { get; private set; }

    internal OrganDirection Direction { get; private set; }

    public Organ(int id, OrganType type, Point position) : this()
    {
        Id = id;
        Type = type;
        Position = position;
    }

    public Organ(int id, OrganType type, Point position, OrganDirection direction) : this(id, type, position)
    {
        Direction = direction;
    }
}

internal enum OrganDirection
{
    N,
    E,
    S,
    W
}

internal struct Organism
{
    internal List<Organ> Organs { get; private set; }

    public Organism()
    {
        Organs = new List<Organ>();
    }

    internal void AddRoot(int id, Point root)
    {
        Organs.Add(new Organ(id, OrganType.ROOT, root));
    }

    internal readonly void AddBasicOrgan(int organId, Point point)
    {
        Organs.Add(new Organ(organId, OrganType.BASIC, point));
    }

    internal readonly void AddHarvesterOrgan(int organId, Point point, OrganDirection direction)
    {
        Organs.Add(new Organ(organId, OrganType.HARVESTER, point, direction));
    }
}


internal enum OrganType
{
    BASIC,
    HARVESTER,
    ROOT
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
                            playerOrganism.AddRoot(organId, new Point(x, y));
                        } 
                        else if (owner == 0)
                        {
                            opponentOrganism.AddRoot(organId, new Point(x, y));
                        }
                        break;
                    case "BASIC":
                        if (owner == 1)
                        {
                            playerOrganism.AddBasicOrgan(organId, new Point(x, y));
                        }
                        else if (owner == 0)
                        {
                            opponentOrganism.AddBasicOrgan(organId, new Point(x, y));
                        }
                        break;
                    case "HARVESTER":

                        OrganDirection dirEnum;
                        if (Enum.TryParse(organDir, out dirEnum))
                        {
                            if (owner == 1)
                            {
                                playerOrganism.AddHarvesterOrgan(organId, new Point(x, y), dirEnum);
                            }
                            else if (owner == 0)
                            {
                                opponentOrganism.AddHarvesterOrgan(organId, new Point(x, y), dirEnum);
                            }
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

internal class Protein
{
    internal ProteinType Type { get; private set; }
    internal Point Position { get; private set; }

    internal bool IsHarvested { get; set; }

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

