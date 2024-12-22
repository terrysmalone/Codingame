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
using static System.Collections.Specialized.BitVector32;
using static System.Formats.Asn1.AsnWriter;
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
        _nodes = new List<Node>();

        // Create a node for the start Point
        Node currentNode = new Node(startPoint);
        _nodes.Add(currentNode);

        bool targetFound = false;

        while (!targetFound)
        {
            Point[] pointsToCheck = new Point[4];

            pointsToCheck[0] = new Point(currentNode.Position.X, currentNode.Position.Y + 1);
            pointsToCheck[1] = new Point(currentNode.Position.X + 1, currentNode.Position.Y);
            pointsToCheck[2] = new Point(currentNode.Position.X, currentNode.Position.Y - 1);
            pointsToCheck[3] = new Point(currentNode.Position.X - 1, currentNode.Position.Y);

            // for each adjacent square
            foreach (Point pointToCheck in pointsToCheck)
            {
                Node? existingNode = _nodes.SingleOrDefault(n => n.Position == pointToCheck);

                // If a node doesnt exists  
                if (existingNode == null)
                {
                    // Create a node if the position is walkable (No wall. No harvested protein)
                    if (MapChecker.CanGrowOn(pointToCheck, _game))
                    {
                        Node node = new Node(pointToCheck);

                        node.Parent = currentNode.Position;

                        node.G = currentNode.G + 1;
                        node.H = (Math.Abs(targetPoint.X - pointToCheck.X) + Math.Abs(targetPoint.Y - pointToCheck.Y));
                        node.F = node.G + node.F;

                        _nodes.Add(node);
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
                    }
                }
            }

            if (_nodes.Count(n => n.Closed == false) == 0)
            {
                return new List<Point>();
            }

            currentNode.Closed = true;

            if (currentNode.Position == targetPoint)
            {
                targetFound = true;
            }
            else
            {
                // Sort nodes
                _nodes = _nodes.OrderBy(n => n.Closed == true).ThenBy(n => n.F).ToList();

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
}


internal static class CostCalculator
{
    internal static bool CanProduceOrgan(OrganType organ, ProteinStock proteinStock)
    {
        switch (organ)
        {
            case OrganType.BASIC:
                if (proteinStock.A >= 1)
                {
                    return true;
                }
                return false;
            case OrganType.HARVESTER:
                if (proteinStock.C >= 1 &&
                    proteinStock.D >= 1)
                {
                    return true;
                }
                return false;
            case OrganType.ROOT:
                if (proteinStock.A >= 1 &&
                    proteinStock.B >= 1 &&
                    proteinStock.C >= 1 &&
                    proteinStock.D >= 1)
                {
                    return true;
                }
                return false;
            case OrganType.SPORER:
                if (proteinStock.B >= 1 &&
                    proteinStock.D >= 1)
                {
                    return true;
                }
                return false;
            case OrganType.TENTACLE:
                if (proteinStock.B >= 1 &&
                    proteinStock.C >= 1)
                {
                    return true;
                }
                return false;
        }

        return false;
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
        Console.Error.WriteLine($"Player organisms");
        Organisms(game.PlayerOrganisms);
        Console.Error.WriteLine("----------------------------------");
        Console.Error.WriteLine($"Opponent organisms");
        Organisms(game.OpponentOrganisms);
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

        proteins.ForEach(p => 
            Console.Error.WriteLine($"Type:{p.Type} - Position:({p.Position.X},{p.Position.Y}) - BeingHarvested:{p.IsHarvested}"));
    }

    internal static void Organisms(List<Organism> organisms)
    {
        foreach (Organism organism in organisms)
        {
            Organism(organism);
            Console.Error.WriteLine("-----------------------------------");
        }
    }

    internal static void Organism(Organism organism)
    {
        foreach (Organ organ in organism.Organs)
        {
            switch (organ.Type)
            {
                case OrganType.BASIC:
                case OrganType.ROOT:
                    Console.Error.WriteLine($" ID:{organ.Id} - Type:{organ.Type.ToString()} - Position:({organ.Position.X},{organ.Position.Y})");
                    break;

                case OrganType.HARVESTER:
                case OrganType.SPORER:
                    Console.Error.WriteLine($" ID:{organ.Id} - Type:{organ.Type.ToString()} - Position:({organ.Position.X},{organ.Position.Y}) - Direction:{organ.Direction.ToString()}");
                    break;
            }
        }
    }

    internal static void Nodes(List<Node> nodes)
    {
        nodes.ForEach(n => 
            Console.Error.WriteLine($"Position:({n.Position.X},{n.Position.Y}) - Closed:{n.Closed}"));
    }

    internal static void Map(Game game)
    {
        string[,] map = new string[game.Width, game.Height];

        for (int y = 0; y < game.Height; y++)
        {
            for (int x = 0; x < game.Width; x++)
            {
                map[x, y] = " ";
            }
        }

        foreach (Point wall in game.Walls)
        {
            map[wall.X, wall.Y] = "X";
        }

        foreach (Protein protein in game.Proteins)
        {
            map[protein.Position.X, protein.Position.Y] = protein.Type.ToString();
        }

        foreach (Organism organism in game.PlayerOrganisms)
        {
            foreach (Organ organ in organism.Organs)
            {
                map[organ.Position.X, organ.Position.Y] = "O";
            }
        }

        foreach (Organism organism in game.OpponentOrganisms)
        {
            foreach (Organ organ in organism.Organs)
            {
                map[organ.Position.X, organ.Position.Y] = "o";
            }
        }

        for (int y = 0; y < game.Height; y++)
        {
            string row = string.Empty;

            for (int x = 0; x < game.Width; x++)
            {
                row += map[x, y];
            }

            Console.Error.WriteLine(row);
        }
    }
}


internal sealed class Game
{
    internal int Width { get; private set; }
    internal int Height { get; private set; }

    internal List<Organism> PlayerOrganisms { get; private set; }
    internal List<Organism> OpponentOrganisms { get; private set; }

    internal ProteinStock PlayerProteinStock { get; private set; }
    internal ProteinStock OpponentProteinStock { get; private set; }
    
    public List<Point> Walls { get; private set; }
    public List<Protein> Proteins { get; private set; }

    internal Game(int width, int height)
    {
        Width = width;
        Height = height;
    }

    internal void SetPlayerProteinStock(ProteinStock playerProteins) => PlayerProteinStock = playerProteins;

    internal void SetOpponentProteinStock(ProteinStock opponentProteins) => OpponentProteinStock = opponentProteins;

    internal void SetPlayerOrganisms(List<Organism> playerOrganisms) => PlayerOrganisms = playerOrganisms;

    internal void SetOpponentOrganisms(List<Organism> opponentOrganisms) => OpponentOrganisms = opponentOrganisms;

    internal void SetWalls(List<Point> walls) => Walls = walls;

    internal void SetProteins(List<Protein> proteins) => Proteins = proteins;

    int turn = 0;
    internal List<string> GetActions()
    {
        // TODO: Add an Action struct to prioritise different actions and choose
        // between them

        CheckForHarvestedProtein();
        
        List<string> actions = new List<string>();

        foreach (Organism organism in PlayerOrganisms)
        {
            string action = CheckForSporerAction(organism);

            if (string.IsNullOrEmpty(action) &&
                organism.Organs.Any(o => o.Type == OrganType.SPORER) &&
                CostCalculator.CanProduceOrgan(OrganType.ROOT, PlayerProteinStock))
            {
                action = CheckForSporeRootAction(organism);
            }

            if (string.IsNullOrEmpty(action))
            {
                action = CheckForHarvestOrBasicAction(organism);
            }

            // If there wasn't a protein to go to just spread randomly...for now
            if (string.IsNullOrEmpty(action) &&
                CostCalculator.CanProduceOrgan(OrganType.BASIC, PlayerProteinStock))
            {
                action = GetRandomBasicGrow(organism);
            }

            if (string.IsNullOrEmpty(action))
            {
                action = "WAIT";
            }

            actions.Add(action);
        }

        return actions;
    }

    // Check to see if any protein is being harvested and mark it as such
    private void CheckForHarvestedProtein()
    {
        foreach (Organism organism in PlayerOrganisms)
        {
            foreach (Organ organ in organism.Organs)
            {
                if (organ.Type == OrganType.HARVESTER)
                {
                    Point harvestedPosition = GetHarvestedPosition(organ);

                    Protein havestedProtein = Proteins.Single(p => p.Position == harvestedPosition);

                    havestedProtein.IsHarvested = true;
                }
            }
        }

        // We don't care about enemy harvested proteins because
        // we're still happy to consume them.
    }

    private string CheckForSporerAction(Organism organism)
    {
        string action = string.Empty;

        int minRootSporerDistance = 5;

        if (CostCalculator.CanProduceOrgan(OrganType.ROOT, PlayerProteinStock) &&
            CostCalculator.CanProduceOrgan(OrganType.SPORER, PlayerProteinStock) &&
            !organism.Organs.Any(o => o.Type == OrganType.SPORER))
        {
            List<Protein> unharvestedByMeProteins = Proteins.Where(p => !p.IsHarvested).ToList();

            int leastStepsToProtein = int.MaxValue;
            int quickestOrganId = -1;
            Point quickestPoint = new Point(-1, -1);

            foreach (Protein protein in unharvestedByMeProteins)
            {
                Console.Error.WriteLine($"Checking protein ({protein.Position.X},{protein.Position.Y})");
                List<Point> possibleRootPoints = MapChecker.GetRootPoints(protein.Position, this);

                // TODO: order by closest to enemy (i.e. We want to be able to block and
                //       destroy the enemy before they can get to the protein

                // TODO: This is very intensive. Maybe add a cutoff for the 
                //       AStar search so that it doesn't keep searching
                foreach (Point rootPoint in possibleRootPoints)
                {
                    Console.Error.WriteLine($"   Checking rootPoint ({rootPoint.X},{rootPoint.Y})");
                    // Draw a line towards the organism
                    // West
                    bool canStillMove = true;
                    int distanceFromRootPoint = 1;

                    while (canStillMove)
                    {
                        Point currentPoint = new Point(rootPoint.X - distanceFromRootPoint, rootPoint.Y);

                        Console.Error.WriteLine($"      Checking currentPoint ({currentPoint.X},{currentPoint.Y})");
                        // If we can't grow here we've hit an obstacle. Don't check further
                        if (!MapChecker.CanGrowOn(currentPoint, this))
                        {
                            canStillMove = false;
                            continue;
                        }

                        // This is too close to bother spawning. Carry on 
                        // checking further
                        if (distanceFromRootPoint >= minRootSporerDistance)
                        {
                            distanceFromRootPoint++;
                            continue;
                        }

                        foreach (Organ organ in organism.Organs)
                        {
                            AStar aStar = new AStar(this);
                            List<Point> path = aStar.GetShortestPath(organ.Position, currentPoint);

                            if (path.Count < leastStepsToProtein)
                            {
                                leastStepsToProtein = path.Count;
                                quickestOrganId = organ.Id;
                                quickestPoint = path[0];
                            }          
                        }

                        distanceFromRootPoint++;
                    }
                }
            }

            if (quickestOrganId != -1)
            {
                return $"GROW {quickestOrganId} {quickestPoint.X} {quickestPoint.Y} SPORER E";
            }
        }

        return string.Empty;
    }

    private string CheckForSporeRootAction(Organism organism)
    {
        Console.Error.WriteLine("CheckForSporeRootAction");

        // This assumes that an organism only has one sporer. That may not always be the case
        Organ sporer = organism.Organs.Single(o => o.Type == OrganType.SPORER);

        // foreach protein
        foreach (Protein protein in Proteins)
        {
            Console.Error.WriteLine($"Checking protein ({protein.Position.X},{protein.Position.Y})");
            List<Point> possibleRootPoints = MapChecker.GetRootPoints(protein.Position, this);
            // TODO: order by closest to enemy (i.e. We want to be able to block and
            //       destroy the enemy before they can get to the protein

            foreach (Point possibleRootPoint in possibleRootPoints)
            {
                Console.Error.WriteLine($"   Checking rootPoint ({possibleRootPoint.X},{possibleRootPoint.Y})");
                Point checkPoint = new Point(possibleRootPoint.X, possibleRootPoint.Y);
                int xDelta = 0;
                int yDelta = 0;

                switch (sporer.Direction)
                {
                    case OrganDirection.N:
                        xDelta = 0;
                        yDelta = 1;
                        break;
                    case OrganDirection.E:
                        xDelta = -1;
                        yDelta = 0;
                        break;
                    case OrganDirection.S:
                        xDelta = 0;
                        yDelta = -1;
                        break;
                    case OrganDirection.W:
                        xDelta = 1;
                        yDelta = 0;
                        break;
                }

                bool canStillMove = true;

                while(canStillMove)
                {
                    Console.Error.WriteLine($"      Checking currentPoint ({checkPoint.X},{checkPoint.Y})");

                    if (!MapChecker.CanGrowOn(checkPoint, this))
                    {
                        canStillMove = false;
                        continue;
                    }

                    // Return the first valid spore we can. It shouldn't make a difference
                    if (checkPoint == new Point(sporer.Position.X - xDelta, sporer.Position.Y - yDelta))
                    {
                        return $"SPORE {sporer.Id} {possibleRootPoint.X} {possibleRootPoint.Y}";
                    }

                    checkPoint = new Point(checkPoint.X + xDelta, checkPoint.Y + yDelta);
                }
            }
        }

        return string.Empty;
    }

    private string CheckForHarvestOrBasicAction(Organism organism)
    {
        string action = string.Empty;

        (int closestOrgan, List<Point> shortestPath) = GetShortestPathToProtein(organism, Proteins);

        if (closestOrgan != -1)
        {
            // See if we can make a harvester
            if (CostCalculator.CanProduceOrgan(OrganType.HARVESTER, PlayerProteinStock) &&
                Proteins.Exists(p => p.Type == ProteinType.A &&
                p.IsHarvested == false))
            {
                if (shortestPath.Count == 2)
                {
                    string dir = GetDirection(shortestPath[0], shortestPath[1]);

                    action = $"GROW {closestOrgan} {shortestPath[0].X} {shortestPath[0].Y} HARVESTER {dir}";
                }
            }

            // If not, just grow towards the nearest A protein
            if (string.IsNullOrEmpty(action) &&
                CostCalculator.CanProduceOrgan(OrganType.BASIC, PlayerProteinStock))
            {
                action = $"GROW {closestOrgan} {shortestPath[0].X} {shortestPath[0].Y} BASIC";
            }
        }

        return action;
    }

    private static Point GetHarvestedPosition(Organ organ)
    {
        switch (organ.Direction)
        {
            case OrganDirection.N:
                return new Point(organ.Position.X, organ.Position.Y-1);
            case OrganDirection.E:
                return new Point(organ.Position.X+1, organ.Position.Y);
            case OrganDirection.S:
                return new Point(organ.Position.X, organ.Position.Y+1);
            case OrganDirection.W:
                return new Point(organ.Position.X-1, organ.Position.Y);
        }

        return new Point(-1,-1);
    }

    private string GetDirection(Point from, Point to)
    {
        string dir = "N";

        if (from.X < to.X)
        {
            dir = "E";
        }
        else if (from.X > to.X)
        {
            dir = "W";
        }
        else if (from.Y < to.Y)
        {
            dir = "S";
        }

        return dir;
    }

    private (int,List<Point>) GetShortestPathToProtein(Organism organism, List<Protein> proteins)
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
                foreach (var organ in organism.Organs)
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

        return (closestId, shortestPath);
    }

    private string GetRandomBasicGrow(Organism organism)
    {
        string action = string.Empty;

        for (int i = organism.Organs.Count - 1; i >= 0; i--)
        {
            Organ current = organism.Organs[i];

            if (MapChecker.CanGrowOn(new Point(current.Position.X + 1, current.Position.Y), this))
            {
                action = $"GROW {current.Id} {current.Position.X + 1} {current.Position.Y} BASIC";
                break;
            }

            if (MapChecker.CanGrowOn(new Point(current.Position.X, current.Position.Y + 1), this))
            {
                action = $"GROW {current.Id} {current.Position.X} {current.Position.Y + 1} BASIC";
                break;
            }

            if (MapChecker.CanGrowOn(new Point(current.Position.X, current.Position.Y - 1), this))
            {
                action = $"GROW {current.Id} {current.Position.X} {current.Position.Y - 1} BASIC";
                break;
            }

            if (MapChecker.CanGrowOn(new Point(current.Position.X - 1, current.Position.Y), this))
            {
                action = $"GROW {current.Id} {current.Position.X - 1} {current.Position.Y} BASIC";
                break;
            }
        }

        return action;
    }

}


internal static class MapChecker
{
    internal static bool CanGrowOn(Point pointToCheck, Game game)
    {
        if (pointToCheck.X < 0 || 
            pointToCheck.Y < 0 || 
            pointToCheck.X >= game.Width || 
            pointToCheck.Y >= game.Height) 
        { 
            return false; 
        }
        // Not walkable if player organ on that spot
        foreach (Organism organism in game.PlayerOrganisms)
        {
            if (organism.Organs.Any(o => o.Position == pointToCheck))
            {
                return false;
            }
        }

        // Not walkable if opponent organ on that spot
        foreach (Organism organism in game.OpponentOrganisms)
        {
            if (organism.Organs.Any(o => o.Position == pointToCheck))
            {
                return false;
            }
        }

        // Not walkable player harvested protein on that spot
        if (game.Proteins.Any(p => p.IsHarvested && p.Position == pointToCheck))
        {
            return false;
        }

        // Not walkable if wall on that spot
        if (game.Walls.Any(w => w == pointToCheck))
        {
            return false;
        }

        return true;
    }

    internal static List<Point> GetRootPoints(Point position, Game game)
    {
        List<Point> rootPoints = new List<Point>();

        Point[] pointsToCheck = new Point[] {
                new Point(position.X - 1, position.Y - 1),
                new Point(position.X - 1, position.Y + 1),
                new Point(position.X + 1, position.Y - 1),
                new Point(position.X + 1, position.Y + 1),
                new Point(position.X - 2, position.Y),
                new Point(position.X + 2, position.Y),
                new Point(position.X, position.Y - 2),
                new Point(position.X, position.Y + 2),
            };

        foreach (Point point in pointsToCheck)
        {
            if (CanGrowOn(point, game))
            {
                rootPoints.Add(point);
            }
        }

        return rootPoints;
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
    internal int RootId { get; private set; }

    public OrganType Type { get; set; }

    internal Point Position { get; private set; }

    internal OrganDirection Direction { get; private set; }

    public Organ(int id, int rootId, OrganType type, Point position) : this()
    {
        Id = id;
        RootId = rootId;
        Type = type;
        Position = position;
    }

    public Organ(int id, int rootId, OrganType type, Point position, OrganDirection direction) : this(id, rootId, type, position)
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

internal class Organism
{
    internal int RootId { get; private set; }

    internal List<Organ> Organs { get; private set; }

    internal Organism(int rootId)
    {
        Organs = new List<Organ>();
        RootId = rootId;
    }

    internal void AddOrgan(Organ organ)
    {
        Organs.Add(organ);
    }


}


internal enum OrganType
{
    BASIC,
    HARVESTER,
    ROOT,
    SPORER,
    TENTACLE,
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
            List<Organ> unsortedPlayerOrgans = new List<Organ>();
            List<Organ> unsortedOpponentOrgans = new List<Organ>();
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

                OrganType organTypeEnum;
                if (Enum.TryParse(type, out organTypeEnum))
                {
                    switch (type)
                    {
                        case "BASIC":
                        case "ROOT":
                            if (owner == 1)
                            {
                                unsortedPlayerOrgans.Add(
                                    CreateOrgan(
                                        organId,
                                        organRootId,
                                        organTypeEnum,
                                        new Point(x, y)));
                            }
                            else if (owner == 0)
                            {
                                unsortedOpponentOrgans.Add(
                                    CreateOrgan(
                                        organId,
                                        organRootId,
                                        organTypeEnum,
                                        new Point(x, y)));
                            }

                            break;

                        case "HARVESTER":
                        case "SPORER":
                            OrganDirection dirEnum;
                            if (Enum.TryParse(organDir, out dirEnum))
                            {
                                if (owner == 1)
                                {
                                    unsortedPlayerOrgans.Add(
                                        CreateDirectionOrgan(
                                            organId,
                                            organRootId,
                                            organTypeEnum,
                                            new Point(x, y),
                                            dirEnum));
                                }
                                else if (owner == 0)
                                {
                                    unsortedOpponentOrgans.Add(
                                        CreateDirectionOrgan(
                                            organId,
                                            organRootId,
                                            organTypeEnum,
                                            new Point(x, y),
                                            dirEnum));
                                }
                            }
                            break;
                    }
                }
                else
                {
                    switch (type)
                    {
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
                        case "WALL":
                            walls.Add(new Point(x, y));
                            break;
                    }
                }
            }

            List<Organism> playerOrganisms = SortOrgans(unsortedPlayerOrgans);
            game.SetPlayerOrganisms(playerOrganisms);
            List<Organism> opponentOrganisms = SortOrgans(unsortedOpponentOrgans);
            game.SetOpponentOrganisms(opponentOrganisms);

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

    private static Organ CreateOrgan(int organId, int rootId, OrganType organType, Point point)
    {
        return new Organ(organId, rootId, organType, point);
    }

    private static Organ CreateDirectionOrgan(int organId, int rootId, OrganType organType, Point point, OrganDirection direction)
    {
        return new Organ(organId, rootId, organType, point, direction);
    }

    private static List<Organism> SortOrgans(List<Organ> unsortedOrgans)
    {
        List<Organism> organisms = new List<Organism>();

        unsortedOrgans = unsortedOrgans.OrderBy(o => o.Type != OrganType.ROOT).ToList();

        foreach (Organ organ in unsortedOrgans)
        {
            if (organ.Type == OrganType.ROOT)
            {
                Organism organism = new Organism(organ.Id);
                organism.AddOrgan(organ);
                organisms.Add(new Organism(organ.Id));
            }

            organisms.Single(o => o.RootId == organ.RootId).AddOrgan(organ);
        }

        return organisms;
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

