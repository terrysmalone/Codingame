using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Xml.Linq;

namespace WinterChallenge2024;

internal sealed class Game
{
    internal int Width { get; private set; }
    internal int Height { get; private set; }

    internal List<Organism> PlayerOrganisms { get; private set; }
    internal List<Organism> OpponentOrganisms { get; private set; }

    internal ProteinStock PlayerProteinStock { get; private set; }
    internal ProteinStock OpponentProteinStock { get; private set; }
    
    public bool[,] Walls { get; private set; }
    public List<Protein> Proteins { get; private set; }

    private ActionFinder _pathFinder;
    private DirectionCalculator _directionCalculator;

    private bool[,] _sporerPoints;

    internal bool[,] isBlocked;
    internal bool[,] hasAnyProtein;
    internal ProteinType[,] proteinTypes;
    internal bool[,] hasHarvestedProtein;
    internal bool[,] opponentOrgans;
    internal bool[,] opponentOrganEdges;

    private Stopwatch _timer;
    private long _totalTime;

    private List<int> _createdSporer = new List<int>();
    
    private readonly List<Point> _directions = new List<Point>
    {
        new Point(0, 1),
        new Point(0, -1),
        new Point(1, 0),
        new Point(-1, 0)
    };

    private int _harvestedAProteins;
    private int _harvestedBProteins;
    private int _harvestedCProteins;
    private int _harvestedDProteins;

    internal Game(int width, int height)
    {
        Width = width;
        Height = height;

        PlayerOrganisms = new List<Organism>();
        OpponentOrganisms = new List<Organism>();

        Walls = new bool[Width, Height];
        Proteins = new List<Protein>();
    }

    internal void SetPlayerProteinStock(ProteinStock playerProteins) => PlayerProteinStock = playerProteins;

    internal void SetOpponentProteinStock(ProteinStock opponentProteins) => OpponentProteinStock = opponentProteins;

    internal void SetPlayerOrganisms(List<Organism> playerOrganisms) => PlayerOrganisms = playerOrganisms;

    internal void SetOpponentOrganisms(List<Organism> opponentOrganisms) => OpponentOrganisms = opponentOrganisms;

    internal void SetWalls(bool[,] walls) => Walls = walls;

    internal void SetProteins(List<Protein> proteins) => Proteins = proteins;

    internal List<Action> GetActions()
    {
        _directionCalculator = new DirectionCalculator(this);
        _pathFinder = new ActionFinder(this, _directionCalculator);

        _totalTime = 0;
        _timer = new Stopwatch();
        _timer.Start();

        _sporerPoints = new bool[Width, Height];

        CheckForHarvestedProtein();
        DisplayTime("Updated check for harvested protein");

        UpdateMaps();
        DisplayTime("Updated maps");

        List<Action> actions = new List<Action>();

        int maxProteinDistance = 5;
        int minRootSporerDistance = 4;

        if (PlayerOrganisms.Count < 2)
        {
            maxProteinDistance = 1;
            minRootSporerDistance = 3;
        }
        else if (PlayerOrganisms.Count < 3)
        {
            maxProteinDistance = 2;
        }
        
        // TODO: This might be redundant soon
        int maxPathSearch = 10;

        int organCount = PlayerOrganisms.SelectMany(o => o.Organs).Count();
        if (organCount > 30)
        {
            maxPathSearch = 5;
        }

        foreach (Organism organism in PlayerOrganisms)
        {   
            Console.Error.WriteLine("-------------------------------------");
            Console.Error.WriteLine($"Checking organism: {organism.RootId}");
            Action? action = null;

            if (action is null)
            {
                action = CheckForTentacleAction(organism);
                DisplayTime("Checked for tentacle action");
            }

            List<Action> proteinActions = 
                _pathFinder.GetProteinActions(organism, Proteins, maxPathSearch);

            DisplayTime("Checked for protein actions");
            
            if (action is null && !_createdSporer.Contains(organism.RootId))
            {
                action = CheckForHarvestOrConsumeAction(proteinActions, maxProteinDistance);
                DisplayTime("Checked for harvest action");
            }

            if (action is null)
            {
                UpdateSporerSpawnPoints();
                DisplayTime("Updated sporer spawn points");
                (action, int fireDistance) = CheckForSporeRootAction(organism, minRootSporerDistance);
                DisplayTime("Checked for spore root action");

                // If we did a root action we can remove this... 
                // unless we fired really far, then give it a chance to do another
                if (action is not null && fireDistance < 10)
                {
                    _createdSporer.Remove(organism.RootId);
                }
            }

            // We skipped this earlier to check for a sporer action.
            // We obviously didn't find one. Try it now.
            if (action is null && _createdSporer.Contains(organism.RootId))
            {
                action = CheckForHarvestOrConsumeAction(proteinActions, maxProteinDistance);
                DisplayTime("Checked for harvest action (later than usual)");

                // We hit this if we couldn't get a spore root action, even though 
                // we prioritised it. There's no point trying again.
                if (action is null)
                {
                    _createdSporer.Remove(organism.RootId);
                }
            }

            if (action is null)
            {
                action = CheckForSporerAction(organism, minRootSporerDistance);
                DisplayTime("Checked for sporer action");
            }

            if (action is null)
            {
                if(proteinActions.Count > 0)
                {
                    proteinActions = proteinActions.OrderByDescending(p => p.Score).ToList();
                    action = proteinActions[0];
                }
                Console.Error.WriteLine("Using proteinAction that was rejected by CheckForHarvestOrConsumeAction");
            }

            //if (action is null)
            //{
            //    // We've already pretty much tried this as part of the 
            //    // Harvester check but do it again now since we're willing 
            //    // to go a further now
            //    action = CheckForMovementAction(proteinActions);
            //    DisplayTime("Checked for movement action");
            //}

            // If we've gotten this far without getting a move things are 
            // desterate. We're either truly blocked or we've blocked ourselves
            // by not wanting to grow over proteins. Try that now
            if (action is null)
            {
                action = GetDesperateDestructiveMove(organism, GrowStrategy.UNHARVESTED);
                DisplayTime("Checked for desperate action");
            }

            // We're even more desperate now. Lets consider growing on harvested 
            // proteins
            //if (string.IsNullOrEmpty(action))
            //{
            //    action = GetDesperateDestructiveMove(organism, GrowStrategy.ALL_PROTEINS);
            //    DisplayTime("Checked for very desperate action");
            //}

            // If there wasn't a protein to go to just spread randomly...for now
            if (action is null)
            {                
                action = GetRandomGrow(organism);
                DisplayTime("Checked for random move action");
            }

            if (action is null)
            {
                action = new Action()
                {
                    ActionType = ActionType.WAIT
                };
            }

            actions.Add(action);
        }

        DisplayTime("Done");

        _timer.Stop();

        return actions;
    }

    // Check to see if any protein is being harvested and mark it as such
    private void CheckForHarvestedProtein()
    {
        _harvestedAProteins = 0;
        _harvestedBProteins = 0;
        _harvestedCProteins = 0;
        _harvestedDProteins = 0;

        foreach (Organism organism in PlayerOrganisms)
        {
            foreach (Organ organ in organism.Organs)
            {
                if (organ.Type == OrganType.HARVESTER)
                {
                    Point harvestedPosition = GetHarvestedPosition(organ);

                    if (Proteins.Any(p => p.Position == harvestedPosition))
                    {
                        Protein havestedProtein = Proteins.Single(p => p.Position == harvestedPosition);

                        havestedProtein.IsHarvested = true;

                        if (havestedProtein.Type == ProteinType.A)
                        {
                            _harvestedAProteins++;
                        }
                        else if (havestedProtein.Type == ProteinType.B)
                        {
                            _harvestedBProteins++;
                        }
                        else if (havestedProtein.Type == ProteinType.C)
                        {
                            _harvestedCProteins++;
                        }
                        else if (havestedProtein.Type == ProteinType.D)
                        {
                            _harvestedDProteins++;
                        }
                    }
                }
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
                return new Point(organ.Position.X, organ.Position.Y - 1);
            case OrganDirection.E:
                return new Point(organ.Position.X + 1, organ.Position.Y);
            case OrganDirection.S:
                return new Point(organ.Position.X, organ.Position.Y + 1);
            case OrganDirection.W:
                return new Point(organ.Position.X - 1, organ.Position.Y);
        }

        return new Point(-1, -1);
    }

    internal void UpdateMaps()
    {
        // Reset them all at the start because some of the calculation 
        // will make changes to the others.
        isBlocked = new bool[Width, Height];

        hasHarvestedProtein = new bool[Width, Height];
        hasAnyProtein = new bool[Width, Height];
        proteinTypes = new ProteinType[Width, Height];

        opponentOrgans = new bool[Width, Height];
        opponentOrganEdges = new bool[Width, Height];

        UpdateIsBlocked();
        UpdateHasProteins();
        UpdateOpponentOrgans();
    }

    private void UpdateIsBlocked()
    {
        // Not walkable if player organ on that spot
        foreach (Organism organism in PlayerOrganisms)
        {
            foreach (Organ organ in organism.Organs)
            {
                isBlocked[organ.Position.X, organ.Position.Y] = true;
            }
        }

        // Not walkable if opponent organ on that spot
        foreach (Organism organism in OpponentOrganisms)
        {
            foreach (Organ organ in organism.Organs)
            {
                isBlocked[organ.Position.X, organ.Position.Y] = true;
            }
        }

        // Not walkable if wall on that spot
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (Walls[x, y])
                {
                    isBlocked[x, y] = true;
                }
            }
        }
    }

    private void UpdateHasProteins()
    {
        foreach (Protein protein in Proteins)
        {
            hasAnyProtein[protein.Position.X, protein.Position.Y] = true;

            proteinTypes[protein.Position.X, protein.Position.Y] = protein.Type;

            if (protein.IsHarvested)
            {
                hasHarvestedProtein[protein.Position.X, protein.Position.Y] = true;
            }
        }
    }

    private void UpdateOpponentOrgans()
    {
        foreach (Organism organism in OpponentOrganisms)
        {
            foreach (Organ organ in organism.Organs)
            {
                opponentOrgans[organ.Position.X, organ.Position.Y] = true;

                // We can't walk on an outward facing tentacle
                // So add these to the isBlocked list and not to the valid edges

                // North
                if (organ.Position.Y - 1 >= 0)
                {
                    if (organ.Type == OrganType.TENTACLE && organ.Direction == OrganDirection.N)
                    {
                        isBlocked[organ.Position.X, organ.Position.Y - 1] = true;
                    }
                    else
                    {
                        opponentOrganEdges[organ.Position.X, organ.Position.Y - 1] = true;
                    }
                }

                // East
                if (organ.Position.X + 1 < Width)
                {
                    if (organ.Type == OrganType.TENTACLE && organ.Direction == OrganDirection.E)
                    {
                        isBlocked[organ.Position.X + 1, organ.Position.Y] = true;
                    }
                    else
                    {
                        opponentOrganEdges[organ.Position.X + 1, organ.Position.Y] = true;
                    }
                }

                // South
                if (organ.Position.Y + 1 < Height)
                {
                    if (organ.Type == OrganType.TENTACLE && organ.Direction == OrganDirection.S)
                    {
                        isBlocked[organ.Position.X, organ.Position.Y + 1] = true;
                    }
                    else
                    {
                        opponentOrganEdges[organ.Position.X, organ.Position.Y + 1] = true;
                    }
                }

                // WEST
                if (organ.Position.X - 1 >= 0)
                {
                    if (organ.Type == OrganType.TENTACLE && organ.Direction == OrganDirection.W)
                    {
                        isBlocked[organ.Position.X - 1, organ.Position.Y] = true;
                    }
                    else
                    {
                        opponentOrganEdges[organ.Position.X - 1, organ.Position.Y] = true;
                    }
                }
            }
        }
    }

    private void DisplayTime(string message)
    {
        long segmentTime = _timer.ElapsedTicks;
        _totalTime += segmentTime;
        Display.TimeStamp(_totalTime, segmentTime, message);
        _timer.Restart();
    }

    private (int, List<Point>) GetShortestPathToProtein(Organism organism, List<Protein> proteins, int minDistance, int maxDistance, GrowStrategy growStrategy)
    {
        string action = string.Empty;

        int shortest = int.MaxValue;
        int closestId = -1;
        List<Point> shortestPath = new List<Point>();

        AStar aStar = new AStar(this);

        // Get the closest protein to Organs
        foreach (Protein protein in proteins)
        {
            // Console.Error.WriteLine($"Checking protein: {protein.Position.X},{protein.Position.Y}");

            if (protein.IsHarvested)
            {
                continue;
            }

            foreach (var organ in organism.Organs)
            {
                // Console.Error.WriteLine($"Checking organ: {organ.Position.X},{organ.Position.Y}");

                int manhattanDistance = MapChecker.CalculateManhattanDistance(organ.Position, protein.Position);

                // Console.Error.WriteLine($"Manhattan distance: {manhattanDistance}");
                // Console.Error.WriteLine($"Max distance: {maxDistance}");

                if (manhattanDistance > maxDistance)
                {
                    continue;
                }

                List<Point> path = aStar.GetShortestPath(organ.Position, protein.Position, maxDistance, growStrategy);

                // Console.Error.WriteLine($"Shortest path count: {path.Count}");
                // Display.Path(shortestPath);

                if (path.Count < shortest && path.Count >= minDistance && path.Count != 0)
                {
                    shortest = path.Count;
                    shortestPath = new List<Point>(path);

                    closestId = organ.Id;

                    if (shortest < maxDistance)
                    {
                        maxDistance = shortest;
                    }
                }
            }
        }

        return (closestId, shortestPath);
    }

    private Action? CheckForTentacleAction(Organism organism)
    {
        if (CostCalculator.CanProduceOrgan(OrganType.TENTACLE, PlayerProteinStock))
        {
            (int closestOrganId, OrganDirection? direction, List<Point> shortestPath) = GetShortestPathToOpponent(organism, 2, 4, GrowStrategy.ALL_PROTEINS);

            if (closestOrganId != -1)
            {
                return new Action()
                {
                    ActionType = ActionType.GROW,

                    OrganId = closestOrganId,
                    TargetPosition = shortestPath[0],
                    OrganType = OrganType.TENTACLE,
                    OrganDirection = direction
                };
            }
        }

        return null;
    }

    private (int, OrganDirection?, List<Point>) GetShortestPathToOpponent(Organism organism, int minDistance, int maxDistance, GrowStrategy growStrategy)
    {
        string action = string.Empty;

        int shortest = int.MaxValue;
        int closestId = -1;
        List<Point> shortestPath = new List<Point>();

        AStar aStar = new AStar(this);

        foreach (var organ in organism.Organs)
        {
            foreach (Organism opponentOrganism in OpponentOrganisms)
            {
                foreach (Organ opponentOrgan in opponentOrganism.Organs)
                {

                    int manhattanDistance = MapChecker.CalculateManhattanDistance(organ.Position, opponentOrgan.Position);

                    if (manhattanDistance > maxDistance)
                    {
                        continue;
                    }

                    List<Point> path = aStar.GetShortestPath(organ.Position, opponentOrgan.Position, maxDistance, growStrategy);

                    if (path.Count < shortest && path.Count >= minDistance && path.Count != 0)
                    {
                        shortest = path.Count;
                        shortestPath = new List<Point>(path);

                        closestId = organ.Id;

                        if (shortest < maxDistance)
                        {
                            maxDistance = shortest;
                        }
                    }
                }
            }
        }

        OrganDirection? direction = null;
        
        if (closestId != -1)
        {
            // If it's a direct attack then face it. Otherwise get the direction right
            if (shortestPath.Count == 2)
            {
                direction = _directionCalculator.GetDirection(shortestPath[0], shortestPath[1]);
            }
            else
            {
                direction = _directionCalculator.CalculateClosestOpponentDirection(shortestPath[0], shortestPath[shortestPath.Count-1]);
            }
        }

        return (closestId, direction, shortestPath);
    }

    private Action? CheckForHarvestOrConsumeAction(List<Action> proteinActions, int maxProteinDistance)
    {
        Console.Error.WriteLine($"proteinActions count: {proteinActions.Count}");

        if (proteinActions.Count == 0)
        {
            return null;
        }

        int notHarvestingScore = 28;
        int noStockScore = 18;
        int harvesterProducingProteinScore = 10;

        foreach (Action proteinAction in proteinActions)
        {
            if (proteinAction.GoalType == GoalType.HARVEST)
            {
                switch (proteinAction.TurnsToGoal)
                {
                    case 1:
                        proteinAction.Score += 50;
                        break;
                    case 2:
                        proteinAction.Score += 40;
                        break;
                    case 3:
                        proteinAction.Score += 30;
                        break;
                    case 4:
                        proteinAction.Score += 20;
                        break;
                    case 5:
                        proteinAction.Score += 10;
                        break;
                }

                if (proteinAction.GoalProteinType == ProteinType.A && _harvestedAProteins < 1)
                {
                    proteinAction.Score += notHarvestingScore;

                    if (PlayerProteinStock.A < 0)
                    {
                        proteinAction.Score += noStockScore;
                    }
                }
                else if (proteinAction.GoalProteinType == ProteinType.B && _harvestedBProteins < 1)
                {
                    proteinAction.Score += notHarvestingScore;

                    if (PlayerProteinStock.B < 0)
                    {
                        proteinAction.Score += noStockScore;
                    }
                }
                else if (proteinAction.GoalProteinType == ProteinType.C && _harvestedCProteins < 1)
                {
                    proteinAction.Score += notHarvestingScore;
                    proteinAction.Score += harvesterProducingProteinScore;

                    if (PlayerProteinStock.C < 0)
                    {
                        proteinAction.Score += noStockScore;
                    }
                }
                else if (proteinAction.GoalProteinType == ProteinType.D && _harvestedDProteins < 1)
                {
                    proteinAction.Score += notHarvestingScore;
                    proteinAction.Score += harvesterProducingProteinScore;

                    if (PlayerProteinStock.D < 0)
                    {
                        proteinAction.Score += noStockScore;
                    }
                }

            }
            else if (proteinAction.GoalType == GoalType.CONSUME)
            {
                // Only 1 shot moves consume
                proteinAction.Score += 5;

                
                if (proteinAction.GoalProteinType == ProteinType.C && PlayerProteinStock.C < 0)
                {
                    proteinAction.Score += noStockScore;
                }
                else if (proteinAction.GoalProteinType == ProteinType.D && PlayerProteinStock.D < 0)
                {
                    proteinAction.Score += noStockScore;
                }
            }
        }

        proteinActions = proteinActions.OrderByDescending(p => p.Score).ToList();

        Display.ProteinActions(proteinActions);

        if (proteinActions[0].TurnsToGoal > maxProteinDistance && _harvestedCProteins > 0 && _harvestedDProteins > 0)
        {
            return null;
        }

        return proteinActions[0];
        // As a first pass lets just aim for one of each protein harvested
        // for each action where I can harvest in one move 

        //    if I'm not harvesting one
        //       Harvest it

        // for each action where I can harvest in two moves
        //    if I'm not harvesting one
        //       Harvest it






        // if I can harvest in one move
        //    if it's a priority protein
        //        harvest it
        //    else
        //        see if I have other priorities 

        // Priority list
        // 1. 1 move harvests
        // 2. 2+ move harvests
        // 3. 1 move consumes

    }

    private void UpdateSporerSpawnPoints()
    {
        foreach (Protein protein in Proteins.Where(p => !p.IsHarvested))
        {
            List<Point> possibleRootPoints = MapChecker.GetRootPoints(protein.Position, this);
            foreach (var possPoint in possibleRootPoints)
            {
                int minDistance = 3;

                // If we've just created a sporer there's a chance that it'll be
                // one closer now. Account for this.
                // Note: This is coupled very tightly to the wole issue around
                //       sporer and then sporing a root in the next turn. I 
                //       may have to do something about it...
                if (_createdSporer.Any())
                {
                    minDistance = 2;
                }

                
                if (!MapChecker.HasNearbyOrgan(possPoint, PlayerOrganisms, minDistance))
                {
                    _sporerPoints[possPoint.X, possPoint.Y] = true;
                }
            }
        }
    }

    private (Action?, int) CheckForSporeRootAction(Organism organism, int minRootSporerDistance)
    {
        if (organism.Organs.Any(o => o.Type == OrganType.SPORER) &&
                CostCalculator.CanProduceOrgan(OrganType.ROOT, PlayerProteinStock))
        {
            List<Organ> sporers = organism.Organs.Where(o => o.Type == OrganType.SPORER).ToList();

            int furthestDistance = -1;
            int furthestSporerId = -1;
            Point furthestRootPoint = new Point(0, 0);

            foreach (Organ sporer in sporers)
            {
                Point direction = new Point(0, 0);

                switch (sporer.Direction)
                {
                    case OrganDirection.N:
                        direction = new Point(0, -1);
                        break;
                    case OrganDirection.E:
                        direction = new Point(1, 0);
                        break;
                    case OrganDirection.S:
                        direction = new Point(0, 1);
                        break;
                    case OrganDirection.W:
                        direction = new Point(-1, 0);
                        break;
                }

                if (direction == new Point(0, 0))
                {
                    Console.Error.WriteLine($"ERROR: Couldn't get sporer direction for {sporer.Position.X}{sporer.Position.Y}");
                }

                Point checkPoint = new Point(sporer.Position.X, sporer.Position.Y);

                int distance = 1;
                bool pathClear = true;
                while (pathClear)
                {
                    checkPoint = new Point(checkPoint.X + direction.X,
                                           checkPoint.Y + direction.Y);

                    if (checkPoint.X < 0) { break; }

                    if (checkPoint.X >= Width) { break; }

                    if (checkPoint.Y < 0) { break; }

                    if (checkPoint.Y >= Height) { break; }

                    if (distance >= minRootSporerDistance)
                    {
                        //    if it's on a spawn point 
                        if (_sporerPoints[checkPoint.X, checkPoint.Y])
                        {
                            if (distance > furthestDistance)
                            {
                                furthestDistance = distance;
                                furthestSporerId = sporer.Id;
                                furthestRootPoint = checkPoint;
                            }
                        }
                    }

                    if (!MapChecker.CanGrowOn(checkPoint, this, GrowStrategy.ALL_PROTEINS))
                    {
                        pathClear = false;
                    }

                    distance++;
                }
            }

            if (furthestDistance != -1)
            {
                Action action = new Action(){
                    ActionType = ActionType.SPORE,
                    OrganId = furthestSporerId,
                    TargetPosition = furthestRootPoint,
                };

                return (action, furthestDistance);
            }
        }

        return (null, -1);
    }

    private Action? CheckForSporerAction(Organism organism, int minRootSporerDistance)
    {
        if (CostCalculator.CanProduceOrgans( new List<OrganType> { OrganType.ROOT, OrganType.SPORER },
                                             PlayerProteinStock))
        {
            int furthestDistance = -1;
            int furthestOrgan = -1;
            Point furthestSporerPoint = new Point(0, 0);
            OrganDirection? furthestDirection = null;

            // for each organ
            foreach (Organ organ in organism.Organs)
            {
                Point organPoint = organ.Position;
                List<Point> directions = new List<Point>();

                // Check south
                if (organPoint.Y <= Height - minRootSporerDistance - 1)
                {
                    directions.Add(new Point(0, 1));
                }

                // Check North
                if (organPoint.Y >= minRootSporerDistance)
                {
                    directions.Add(new Point(0, -1));
                }

                // Check East
                if (organPoint.X <= Width - minRootSporerDistance - 1)
                {
                    directions.Add(new Point(1, 0));
                }

                // Check West
                if (organPoint.X >= minRootSporerDistance)
                {
                    directions.Add(new Point(-1, 0));
                }

                // Check the four points around the organ
                foreach (Point side in directions)
                {
                    Point sporerPoint = new Point(organPoint.X + side.X,
                                                  organPoint.Y + side.Y);

                    if (!MapChecker.CanGrowOn(sporerPoint, this, GrowStrategy.NO_PROTEINS))
                    {
                        continue;
                    }

                    // Check in all 4 directions
                    foreach (Point direction in directions)
                    {
                        Point checkPoint = new Point(sporerPoint.X,
                                                     sporerPoint.Y);

                        int distance = 1;
                        bool pathClear = true;
                        while (pathClear)
                        {
                            checkPoint = new Point(checkPoint.X + direction.X,
                                                   checkPoint.Y + direction.Y);

                            // Console.Error.WriteLine($"Checking point {checkPoint.X},{checkPoint.Y}");

                            if (checkPoint.X < 0) { break; }

                            if (checkPoint.X >= Width) { break; }

                            if (checkPoint.Y < 0) { break; }

                            if (checkPoint.Y >= Height) { break; }

                            if (distance >= minRootSporerDistance)
                            {
                                // Console.Error.WriteLine($"Distance viable");
                                //    if it's on a spawn point 
                                if (_sporerPoints[checkPoint.X, checkPoint.Y])
                                {
                                    // Console.Error.WriteLine($"There's a spore point");
                                    OrganDirection? dir = null;

                                    if (direction.X == 1)
                                    {
                                        dir = OrganDirection.E;
                                    }
                                    else if (direction.X == -1)
                                    {
                                        dir = OrganDirection.W;
                                    }
                                    else if (direction.Y == -1)
                                    {
                                        dir = OrganDirection.N;
                                    }
                                    else if (direction.Y == 1)
                                    {
                                        dir = OrganDirection.S;
                                    }

                                    if (distance > furthestDistance)
                                    {
                                        //Console.Error.WriteLine("Added to furthestDistance");
                                        furthestDistance = distance;
                                        furthestOrgan = organ.Id;
                                        furthestSporerPoint = new Point(sporerPoint.X, sporerPoint.Y);
                                        furthestDirection = dir;
                                    }
                                }
                            }

                            if (!MapChecker.CanGrowOn(checkPoint, this, GrowStrategy.ALL_PROTEINS))
                            {
                                pathClear = false;
                            }

                            distance++;
                        }
                    }
                }
            }

            if (furthestDistance != -1)
            {
                _createdSporer.Add(organism.RootId);

                return new Action()
                {
                    ActionType = ActionType.GROW,
                    OrganType = OrganType.SPORER,
                    OrganId = furthestOrgan,
                    TargetPosition = furthestSporerPoint,
                    OrganDirection = furthestDirection,
                };
            }
        }

        return null;
    }

    

    //private Action? CheckForMovementAction(List<Action> actions)
    //{
    //    if (actions.Count > 0)
    //    {
    //        return actions[0];
    //    }

    //    return null;
    //    //string action = string.Empty;

    //    //if (closestOrgan != -1)
    //    //{
    //    //    (OrganType? organType, OrganDirection? direction) = GetOrganAction(shortestPath[0]);

    //    //    if (organType is null)
    //    //    {
    //    //        return null;
    //    //    }

    //    //    return new Action()
    //    //    {
    //    //        ActionType = ActionType.GROW,
    //    //        OrganType = organType,
    //    //        OrganId = closestOrgan,
    //    //        TargetPosition = shortestPath[0],

    //    //        OrganDirection = direction
    //    //    };
    //    //}

    //    //return null;
    //}

    private (OrganType?, OrganDirection?) GetOrganAction(Point point)
    {
        // Grow towards the nearest protein
        OrganDirection? direction = _directionCalculator.CalculateClosestOpponentDirection(point);


        bool hasProtein = hasAnyProtein[point.X, point.Y];
        // If we can make it a tentacle and still have some spare proteins then do it
        if (CostCalculator.CanProduceOrgan(OrganType.TENTACLE, PlayerProteinStock, 3) && !hasProtein)
        {
            return (OrganType.TENTACLE, direction);
        }
        else if (CostCalculator.CanProduceOrgan(OrganType.BASIC, PlayerProteinStock))
        {
            return (OrganType.BASIC, null);
        }
        else if (CostCalculator.CanProduceOrgan(OrganType.SPORER, PlayerProteinStock))
        {
            return (OrganType.SPORER, direction);
        }
        else if (CostCalculator.CanProduceOrgan(OrganType.HARVESTER, PlayerProteinStock))
        {
            return (OrganType.HARVESTER, direction);
        }
        else if (CostCalculator.CanProduceOrgan(OrganType.TENTACLE, PlayerProteinStock))
        {
            return (OrganType.TENTACLE, direction);
        }

        return (null, null);
    }

    private Action? GetDesperateDestructiveMove(Organism organism, GrowStrategy growStrategy)
    {
        (int closestOrgan, List<Point> shortestPath) = GetShortestPathToProtein(organism, Proteins, 1, 10, growStrategy);

        if (closestOrgan != -1)
        {
            OrganType? organType = null;
            
            if (CostCalculator.CanProduceOrgan(OrganType.TENTACLE, PlayerProteinStock))
            {
                organType = OrganType.TENTACLE;
            }
            else if (CostCalculator.CanProduceOrgan(OrganType.BASIC, PlayerProteinStock))
            {
                organType = OrganType.BASIC;
            }
            else if (CostCalculator.CanProduceOrgan(OrganType.SPORER, PlayerProteinStock))
            {
                organType = OrganType.SPORER;
            }
            else if (CostCalculator.CanProduceOrgan(OrganType.HARVESTER, PlayerProteinStock))
            {
                organType = OrganType.HARVESTER;
            }

            if (organType is null)
            {
                return null;
            }

            OrganDirection? closestRootDirection = null;
            if (organType != OrganType.BASIC)
            {
                closestRootDirection = _directionCalculator.CalculateClosestOpponentDirection(shortestPath[0]);

            }

            return new Action()
            {
                OrganType = organType,
                ActionType = ActionType.GROW,
                OrganId = closestOrgan,
                TargetPosition = shortestPath[0],   
                OrganDirection = closestRootDirection
            };
        }

        return null;
    }

    private Action? GetRandomGrow(Organism organism)
    {
        if (organism.RootId == 50)
        {
            Display.Organism(organism);
        }

        for (int i = organism.Organs.Count - 1; i >= 0; i--)
        {
            Organ current = organism.Organs[i];

            foreach (Point direction in _directions)
            {
                Point checkPoint = new Point(current.Position.X + direction.X, 
                                             current.Position.Y + direction.Y);

                if (checkPoint.X < 0 || checkPoint.X >= Width ||
                    checkPoint.Y < 0 || checkPoint.Y >= Height)
                {
                    continue;
                }

                (OrganType? organType, OrganDirection? organDirection) = GetOrganAction(checkPoint);

                if (organType is null)
                {
                    continue;
                }

                // If we can grow on here without destroying a harvester protein 
                // just do it
                if (MapChecker.CanGrowOn(checkPoint, this, GrowStrategy.UNHARVESTED))
                {
                    return new Action()
                    {
                        ActionType = ActionType.GROW,
                        OrganType = organType,
                        OrganDirection = organDirection,
                        OrganId = current.Id,
                        TargetPosition = checkPoint
                    };
                }

                // If we couldn't lets check if we can move by destroying a
                // harvested protein
                if (MapChecker.CanGrowOn(checkPoint, this, GrowStrategy.ALL_PROTEINS))
                {
                    // Check all around it. If there's space let it do it. 
                    // Otherwise save it and hope for better.
                    // TODO: This is a very naieve implementation that only really
                    //       helps if we have one harvested protein sitting on its
                    //       own hoping to be protected
                    foreach (Point d in _directions)
                    {
                        if(MapChecker.CanGrowOn(new Point(checkPoint.X + d.X, checkPoint.Y + d.Y), 
                                                this, 
                                                GrowStrategy.ALL_PROTEINS))
                        {
                            return new Action()
                            {
                                ActionType = ActionType.GROW,
                                OrganType = organType,
                                OrganDirection = organDirection,
                                OrganId = current.Id,
                                TargetPosition = checkPoint
                            };
                        }
                    }
                }
            }
        }

        return null;
    }
}