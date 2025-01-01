using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Xml.Linq;
using static System.Formats.Asn1.AsnWriter;

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

        Dictionary<int, List<Action>> allPossibleActions = new Dictionary<int, List<Action>>();

        int maxProteinDistance = 5;
        int minRootSporerDistance = 4;
        int extraPriorityScore = 0;

        if (PlayerOrganisms.Count < 2)
        {
            maxProteinDistance = 1;
            minRootSporerDistance = 3;
            extraPriorityScore = 50;
        }
        else if (PlayerOrganisms.Count < 3)
        {
            maxProteinDistance = 2;
            extraPriorityScore = 50;
        }

        foreach (Organism organism in PlayerOrganisms)
        {   
            List<Action> possibleActions = new List<Action>();

            Console.Error.WriteLine("-------------------------------------");
            Console.Error.WriteLine($"Checking organism: {organism.RootId}");

            Action? action = CheckForTentacleAction(organism);

            if (action is not null)
            {
                possibleActions.Add(action);
            }
            DisplayTime($"Checked for tentacle action. {possibleActions.Count} possible actions");
            

            // TODO
            // We're happy to just go with these if we can definitely afford 
            // to grow the tentacle. Otherwise we need some other backups.

            if (possibleActions.Count == 0 && !_createdSporer.Contains(organism.RootId))
            {
                List<Action> actions = GetHarvestAndConsumeActions(organism, maxProteinDistance);
                DisplayTime($"Checked for harvest action. {actions.Count} possible actions");

                if (actions.Count > 0)
                {
                    possibleActions.AddRange(actions);
                }
            }

            UpdateSporerSpawnPoints();
            if(_createdSporer.Contains(organism.RootId))
            {
                extraPriorityScore = 0;
            }

            if (_createdSporer.Contains(organism.RootId))
            {
                DisplayTime("Updated sporer spawn points");
                (Action? sporeAction, int fireDistance) = CheckForSporeRootAction(organism, minRootSporerDistance);

                if (sporeAction is null)
                {
                    DisplayTime("Checked for spore root action. No possible actions");
                }
                else 
                { 
                    DisplayTime($"Checked for spore root action. 1 possible action");
                }

                // If we did a root action we can remove this... 
                // unless we fired really far, then give it a chance to do another
                if (sporeAction is not null)
                {
                    possibleActions.Add(sporeAction);
                    if (fireDistance < 10)
                    {
                        _createdSporer.Remove(organism.RootId);
                    }
                }
            }

            // We skipped this earlier to check for a sporer action.
            // We obviously didn't find one. Try it now.
            if (possibleActions.Count == 0 && _createdSporer.Contains(organism.RootId))
            {
                List<Action> actions = GetHarvestAndConsumeActions(organism, maxProteinDistance);
                DisplayTime($"Checked for harvest action (later than usual). {actions.Count} possible actions");

                if (actions.Count > 0)
                {
                    possibleActions.AddRange(actions);
                }

                // We hit this if we couldn't get a spore root action, even though 
                // we prioritised it. There's no point trying again.
                if (possibleActions.Count == 0)
                {
                    _createdSporer.Remove(organism.RootId);
                }
            }

            // Don't create a sporer if we already did
            Console.Error.WriteLine($"Checking for sporer action: {_createdSporer.Contains(organism.RootId)}");
            if (!_createdSporer.Contains(organism.RootId))
            { 
                Action? sporerAction = CheckForSporerAction(organism, minRootSporerDistance, extraPriorityScore);
                if (sporerAction is null)
                {
                    DisplayTime("Checked for sporer action. No possible action");
                }
                else
                {
                    DisplayTime("Checked for sporer action. 1 possible action");
                }

                if (sporerAction is not null)
                {
                    possibleActions.Add(sporerAction);
                }
            }

            if (possibleActions.Count == 0)
            { 
                // Note: This is a horrible hack until i get scoring sorted. All of this has already been done above!
                List<Action> actions = GetHarvestAndConsumeActions(organism, 1000000);
               
                if (actions.Count > 0)
                {
                    possibleActions.AddRange(actions);
                }

                Console.Error.WriteLine($"Checked for Action that was rejected by CheckForHarvestOrConsumeAction. {actions.Count} possible actions");
            }

            List<Action> desperateActions = GetDesperateDestructiveMove(organism, GrowStrategy.UNHARVESTED, 4);
            DisplayTime($"Checked for desperate actions. {desperateActions.Count} possible actions");
            possibleActions.AddRange(desperateActions);
            
            List<Action> veryDesperateActions = GetDesperateDestructiveMove(organism, GrowStrategy.ALL_PROTEINS,3);
            DisplayTime($"Checked for very desperate actions. {veryDesperateActions.Count} possible actions");
            possibleActions.AddRange(veryDesperateActions);
            
            List<Action> randomActions = GetRandomGrowActions(organism);
            DisplayTime($"Checked for random move action. {randomActions.Count} possible actions");

            possibleActions.AddRange(randomActions);
            
            if (possibleActions.Count == 0)
            {
                Action? waitAction = new Action()
                {
                    OrganismId = organism.RootId,
                    GoalType = GoalType.WAIT,
                    ActionType = ActionType.WAIT,

                    Source = "No other actions found"
                };
                possibleActions.Add(waitAction);
            }

            possibleActions = possibleActions.OrderByDescending(p => p.Score).ToList();

            // Display.Actions(possibleActions);
            Console.Error.WriteLine($"Possible actions: {possibleActions.Count}");

            allPossibleActions.Add(organism.RootId, possibleActions);
        }

        DisplayTime("Done scoring");

        List<Action> chosenActions = PickBestActions(allPossibleActions);

        foreach (Action action in chosenActions)
        {
            if (action.OrganType == OrganType.SPORER)
            {
                _createdSporer.Add(action.OrganismId);
            }

            Console.Error.WriteLine($"{action.ToString()} - from {action.Source}");
        }

        DisplayTime("Done picking best actions");

        _timer.Stop();

        return chosenActions;
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

            if (protein.IsHarvested || isBlocked[protein.Position.X, protein.Position.Y])
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
                    OrganismId = organism.RootId,
                    ActionType = ActionType.GROW,

                    OrganId = closestOrganId,
                    TargetPosition = shortestPath[0],
                    OrganType = OrganType.TENTACLE,
                    OrganDirection = direction, 
                    Score = 500, // Tentacle moves are higher than the rest by default

                    Source = "CheckForTentacleAction"

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

    private List<Action> GetHarvestAndConsumeActions(Organism organism, int maxProteinDistance)
    {
        List<Action> proteinActions =
               _pathFinder.GetProteinActions(organism, Proteins);

        if (proteinActions.Count == 0)
        {
            return new List<Action>();
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

                if (proteinAction.GoalProteinType == ProteinType.A && PlayerProteinStock.A <= 0)
                {
                    proteinAction.Score += noStockScore;
                }
                else if (proteinAction.GoalProteinType == ProteinType.B && PlayerProteinStock.B <= 0)
                {
                    proteinAction.Score += noStockScore;
                }
                else if (proteinAction.GoalProteinType == ProteinType.C && PlayerProteinStock.C <= 0)
                {
                    proteinAction.Score += noStockScore;
                }
                else if (proteinAction.GoalProteinType == ProteinType.D && PlayerProteinStock.D <= 0)
                {
                    proteinAction.Score += noStockScore;
                }
            }
        }

        proteinActions = proteinActions.OrderByDescending(p => p.Score).ToList();

        //Display.ProteinActions(proteinActions);

        // TODO: I'll need to implement this by giving spawn scores a boost if the above criterai is met
        if (proteinActions[0].TurnsToGoal > maxProteinDistance && _harvestedCProteins > 0 && _harvestedDProteins > 0)
        {
            return new List<Action>();
        }

        return proteinActions;
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
                Point direction = _directionCalculator.GetDelta(sporer.Direction);

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
                    OrganismId = organism.RootId,
                    ActionType = ActionType.SPORE,
                    GoalType = GoalType.ROOT,
                    GoalOrganType = OrganType.ROOT,
                    OrganId = furthestSporerId,
                    TargetPosition = furthestRootPoint,
                    TurnsToGoal = 1,
                    Score = 200,

                    Source = "CheckForSporeRootAction"
                };

                return (action, furthestDistance);
            }
        }

        return (null, -1);
    }

    private Action? CheckForSporerAction(Organism organism, int minRootSporerDistance, int extraPriorityScore)
    {
        if (CostCalculator.CanProduceOrgans( new List<OrganType> { OrganType.ROOT, OrganType.SPORER }
        ,
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
                if (organPoint.Y+1 < Height)
                {
                    directions.Add(new Point(0, 1));
                }

                // Check North
                if (organPoint.Y > 0)
                {
                    directions.Add(new Point(0, -1));
                }

                // Check East
                if (organPoint.X+1 < Width)
                {
                    directions.Add(new Point(1, 0));
                }

                // Check West
                if (organPoint.X > 0)
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

                            if (checkPoint.X < 0) { break; }

                            if (checkPoint.X >= Width) { break; }

                            if (checkPoint.Y < 0) { break; }

                            if (checkPoint.Y >= Height) { break; }

                            if (distance >= minRootSporerDistance)
                            {
                                //    if it's on a spawn point 
                                if (_sporerPoints[checkPoint.X, checkPoint.Y])
                                {
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
                return new Action()
                {
                    OrganismId = organism.RootId,
                    ActionType = ActionType.GROW,
                    OrganType = OrganType.SPORER,
                    OrganId = furthestOrgan,
                    TargetPosition = furthestSporerPoint,
                    OrganDirection = furthestDirection,
                    GoalType = GoalType.SPORE,
                    TurnsToGoal = 1,
                    Score = 40 + extraPriorityScore,
                    Source = "CheckForSporerAction"
                };
            }
        }

        return null;
    }

    private List<Action> GetDesperateDestructiveMove(Organism organism, GrowStrategy growStrategy, int score)
    {
        List<Action> possibleActions = new List<Action>();

        (int closestOrgan, List<Point> shortestPath) = GetShortestPathToProtein(organism, Proteins, 1, 5, growStrategy);

        if (closestOrgan != -1)
        {
            OrganDirection? closestRootDirection = _directionCalculator.CalculateClosestOpponentDirection(shortestPath[0]);

            if (CostCalculator.CanProduceOrgan(OrganType.TENTACLE, PlayerProteinStock))
            {
                possibleActions.Add(CreateGrowAction(organism.RootId, OrganType.TENTACLE, closestOrgan, shortestPath[0], closestRootDirection, score, "GetDesperateDestructiveMove"));
            }
            
            if (CostCalculator.CanProduceOrgan(OrganType.BASIC, PlayerProteinStock))
            {
                possibleActions.Add(CreateGrowAction(organism.RootId, OrganType.BASIC, closestOrgan, shortestPath[0], null, score, "GetDesperateDestructiveMove"));
            }
            
            if (CostCalculator.CanProduceOrgan(OrganType.SPORER, PlayerProteinStock))
            {
                possibleActions.Add(CreateGrowAction(organism.RootId, OrganType.SPORER, closestOrgan, shortestPath[0], closestRootDirection, score, "GetDesperateDestructiveMove"));
            }
            
            if (CostCalculator.CanProduceOrgan(OrganType.HARVESTER, PlayerProteinStock))
            {
                possibleActions.Add(CreateGrowAction(organism.RootId, OrganType.HARVESTER, closestOrgan, shortestPath[0], closestRootDirection, score, "GetDesperateDestructiveMove"));
            }
        }
        return possibleActions;
    }

    private Action CreateGrowAction(int organismRootId, 
                                    OrganType? organType, 
                                    int organId, 
                                    Point targetPosition, 
                                    OrganDirection? closestRootDirection, 
                                    int score,
                                    string source)
    {
        return new Action()
        {
            OrganismId = organismRootId,
            GoalType = GoalType.GROW,
            OrganType = organType,
            ActionType = ActionType.GROW,
            OrganId = organId,
            TargetPosition = targetPosition,
            OrganDirection = closestRootDirection,
            Score = score,
            Source = source
        };
    }

    private List<Action> GetRandomGrowActions(Organism organism)
    {
        List<Action> possibleActions = new List<Action>();
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
                    OrganDirection? closestRootDirection = _directionCalculator.CalculateClosestOpponentDirection(checkPoint);

                    if (CostCalculator.CanProduceOrgan(OrganType.TENTACLE, PlayerProteinStock))
                    {
                        possibleActions.Add(CreateGrowAction(organism.RootId, OrganType.TENTACLE, current.Id, checkPoint, closestRootDirection, 2, "GetRandomGrowActions"));
                    }

                    if (CostCalculator.CanProduceOrgan(OrganType.BASIC, PlayerProteinStock))
                    {
                        possibleActions.Add(CreateGrowAction(organism.RootId, OrganType.BASIC, current.Id, checkPoint, null, 2, "GetRandomGrowActions"));
                    }

                    if (CostCalculator.CanProduceOrgan(OrganType.SPORER, PlayerProteinStock))
                    {
                        possibleActions.Add(CreateGrowAction(organism.RootId, OrganType.SPORER, current.Id, checkPoint, closestRootDirection, 2, "GetRandomGrowActions"));
                    }

                    if (CostCalculator.CanProduceOrgan(OrganType.HARVESTER, PlayerProteinStock))
                    {
                        possibleActions.Add(CreateGrowAction(organism.RootId, OrganType.HARVESTER, current.Id, checkPoint, closestRootDirection, 2, "GetRandomGrowActions"));
                    }
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
                            OrganDirection? closestRootDirection = _directionCalculator.CalculateClosestOpponentDirection(checkPoint);

                            if (CostCalculator.CanProduceOrgan(OrganType.TENTACLE, PlayerProteinStock))
                            {
                                possibleActions.Add(CreateGrowAction(organism.RootId, OrganType.TENTACLE, current.Id, checkPoint, closestRootDirection, 1, "GetRandomGrowActions"));
                            }

                            if (CostCalculator.CanProduceOrgan(OrganType.BASIC, PlayerProteinStock))
                            {
                                possibleActions.Add(CreateGrowAction(organism.RootId, OrganType.BASIC, current.Id, checkPoint, null, 1, "GetRandomGrowActions"));
                            }

                            if (CostCalculator.CanProduceOrgan(OrganType.SPORER, PlayerProteinStock))
                            {
                                possibleActions.Add(CreateGrowAction(organism.RootId, OrganType.SPORER, current.Id, checkPoint, closestRootDirection, 1, "GetRandomGrowActions"));
                            }

                            if (CostCalculator.CanProduceOrgan(OrganType.HARVESTER, PlayerProteinStock))
                            {
                                possibleActions.Add(CreateGrowAction(organism.RootId, OrganType.HARVESTER, current.Id, checkPoint, closestRootDirection, 1, "GetRandomGrowActions"));
                            }
                        }
                    }
                }
            }
        }

        return possibleActions;
    }

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

    private List<Action> PickBestActions(Dictionary<int, List<Action>> allPossibleActions)
    {
        // Create variables for all protein stock
        ProteinStock tempProteinStock = new ProteinStock(PlayerProteinStock.A,
                                                         PlayerProteinStock.B,
                                                         PlayerProteinStock.C,
                                                         PlayerProteinStock.D);

        // Pick the best action for each organism
        List<Action> chosenActions = new List<Action>();

        bool[] chosen = new bool[PlayerOrganisms.Count];

        List<Point> targetPositions = new List<Point>();
        List<Point> harvestTargetPositions = new List<Point>();

        bool allChosen = false;
        int count = 0;

        while (!allChosen)
        {
            int highestScore = -1;
            int highestScoreIndex = -1;
            int highestActionIndex = -1;
            int highestOganismIndex = -1;

            for (int i = 0; i < PlayerOrganisms.Count; i++)
            {
                Organism organism = PlayerOrganisms[i];

                if (chosen[i])
                {
                    continue;
                }

                int id = organism.RootId;

                if (allPossibleActions.ContainsKey(id))
                {
                    List<Action> possibleActions = allPossibleActions[id];
                    if (possibleActions.Count > 0)
                    {
                        int actionIndex = 0;

                        bool canCreate = false;

                        while (!canCreate)
                        {
                            Action checkAction = possibleActions[actionIndex];
                            if (checkAction.ActionType == ActionType.WAIT)
                            {
                                canCreate = true;
                            }
                            if (checkAction.ActionType == ActionType.GROW)
                            {
                                if (checkAction.OrganType is null)
                                {
                                    Console.Error.WriteLine("ERROR: Organ type is null");

                                }
                                if (CostCalculator.CanProduceOrgan(checkAction.OrganType.Value, tempProteinStock))
                                {
                                    canCreate = true;
                                }
                            }
                            else if (checkAction.ActionType == ActionType.SPORE)
                            {
                                if (CostCalculator.CanProduceOrgan(OrganType.ROOT, tempProteinStock))
                                {
                                    canCreate = true;
                                }
                            }

                            // If another action has used this position move on
                            if (canCreate && targetPositions.Contains(checkAction.TargetPosition))
                            {
                                canCreate = false;
                            }

                            // If another action has harvested on this position move on
                            if (canCreate && checkAction.OrganType == OrganType.HARVESTER)
                            {
                                Point delta = _directionCalculator.GetDelta(checkAction.OrganDirection.Value);

                                if (harvestTargetPositions.Contains(new Point(checkAction.TargetPosition.X + delta.X,
                                                                              checkAction.TargetPosition.Y + delta.Y)))
                                {
                                    canCreate = false;
                                }
                            }

                            if (!canCreate)
                            {
                                actionIndex++;
                                if (actionIndex >= possibleActions.Count)
                                {
                                    actionIndex--;
                                    // We can't create it. Should we do a wait?
                                    break;
                                }
                            }
                        }

                        Action topAction = possibleActions[actionIndex];

                        if (topAction.Score > highestScore)
                        {
                            highestScore = topAction.Score;
                            highestScoreIndex = id;
                            highestActionIndex = actionIndex;
                            highestOganismIndex = i;
                        }
                    }
                    else
                    {
                        // THIS SHOULD NEVER HAPPEN. MAYBE THROW A WAIT IN JUST IN CASE
                    }
                }
                count++;
            }

            chosen[highestOganismIndex] = true;

            Action chosenAction = allPossibleActions[highestScoreIndex][highestActionIndex];

            chosenActions.Add(chosenAction);
            targetPositions.Add(chosenAction.TargetPosition);
            if (chosenAction.OrganType == OrganType.HARVESTER)
            {
                Point delta = _directionCalculator.GetDelta(chosenAction.OrganDirection.Value);
                harvestTargetPositions.Add(new Point(chosenAction.TargetPosition.X + delta.X, 
                                                     chosenAction.TargetPosition.Y + delta.Y));
            }

            // Deduct the cost of the action from the protein stock
            if (chosenAction.ActionType == ActionType.GROW)
            {
                if (chosenAction.OrganType is null)
                {
                    Console.Error.WriteLine("ERROR: Organ type is null");

                }

                switch (chosenAction.OrganType)
                {
                    case OrganType.BASIC:
                        tempProteinStock.A -= 1;
                        break;
                    case OrganType.HARVESTER:
                        tempProteinStock.C -= 1;
                        tempProteinStock.D -= 1;
                        break;
                    case OrganType.SPORER:
                        tempProteinStock.B -= 1;
                        tempProteinStock.D -= 1;
                        break;
                    case OrganType.TENTACLE:
                        tempProteinStock.B -= 1;
                        tempProteinStock.C -= 1;
                        break;
                }

            }
            else if (chosenAction.ActionType == ActionType.SPORE)
            {
                tempProteinStock.A -= 1;
                tempProteinStock.B -= 1;
                tempProteinStock.C -= 1;
                tempProteinStock.D -= 1;
            }

            allChosen = chosen.All(c => c);
        }

        return chosenActions;
    }
}