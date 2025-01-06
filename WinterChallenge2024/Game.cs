using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

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
    internal int[,] opponentOrganChildren;

    internal bool[,] opponentTentaclePath;

    private Stopwatch _timer;
    private long _totalTime;

    private List<int> _createdSporer = new List<int>();

    private Dictionary<ActionSource, int> _trackedActions = new Dictionary<ActionSource, int>();

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

    private int _waitCount = 0;

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

            List<Action> tentacleActions = CheckForTentacleAction(organism);

            possibleActions.AddRange(tentacleActions);
            
            DisplayTime($"Checked for tentacle action. {possibleActions.Count} possible actions");
            
            if (possibleActions.Count == 0 && !_createdSporer.Contains(organism.RootId))
            {
                List<Action> actions = GetHarvestAndConsumeActions(organism, maxProteinDistance);
                DisplayTime($"Checked for harvest action. {actions.Count} possible actions");
                Display.Actions(actions);
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

            if (_createdSporer.Contains(organism.RootId) && (HasHarvestedAllProteins() || PlayerOrganisms.Count < 3))
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

                if (sporeAction is not null)
                {
                    possibleActions.Add(sporeAction);
                    if (fireDistance < 10)
                    {
                        _createdSporer.Remove(organism.RootId);
                    }
                }
            }

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
            if (!_createdSporer.Contains(organism.RootId) && (HasHarvestedAllProteins() || PlayerOrganisms.Count < 3))
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

            List<Action> desperateActions = GetDesperateDestructiveMove(organism, GrowStrategy.UNHARVESTED, 4, ActionSource.DESPERATE_DESTRUCTIVE_MOVE);
            DisplayTime($"Checked for desperate actions. {desperateActions.Count} possible actions");
            possibleActions.AddRange(desperateActions);
            
            List<Action> veryDesperateActions = GetDesperateDestructiveMove(organism, GrowStrategy.ALL_PROTEINS,3, ActionSource.VERY_DESPERATE_DESTRUCTIVE_MOVE);
            DisplayTime($"Checked for very desperate actions. {veryDesperateActions.Count} possible actions");
            possibleActions.AddRange(veryDesperateActions);
            
            List<Action> randomActions = GetRandomGrowActions(organism);
            DisplayTime($"Checked for random move action. {randomActions.Count} possible actions");
            //Display.Actions(randomActions);

            possibleActions.AddRange(randomActions);
            
            if (possibleActions.Count == 0)
            {
                // If we've done only WAIT moves for more than 3 moves assume that 
                // We're done and start destroying proteins
                if (_waitCount > 3)
                {
                   List<Action> endGameDestroyMoves = GetEndGameDestroyMoves(organism);
                    possibleActions.AddRange(endGameDestroyMoves);
                    DisplayTime($"Checked for end game destroy moves. {endGameDestroyMoves.Count} possible actions");
                }

                if (possibleActions.Count == 0)
                {
                    Action? waitAction = new Action()
                    {
                        OrganismId = organism.RootId,
                        GoalType = GoalType.WAIT,
                        ActionType = ActionType.WAIT,

                        Source = ActionSource.FINAL_WAIT
                    };
                    possibleActions.Add(waitAction);
                }
            }

            possibleActions = possibleActions.OrderByDescending(p => p.Score).ToList();

            //Display.Actions(possibleActions);
            //Console.Error.WriteLine($"Possible actions: {possibleActions.Count}");

            allPossibleActions.Add(organism.RootId, possibleActions);
        }

        DisplayTime("Done scoring");

        List<Action> chosenActions = PickBestActions(allPossibleActions);

        bool allWait = true;

        foreach (Action action in chosenActions)
        {
            if (action.OrganType == OrganType.SPORER)
            {
                _createdSporer.Add(action.OrganismId);
            }

            if (action.ActionType != ActionType.WAIT)
            {
                allWait = false;
            }


            _trackedActions.TryGetValue(action.Source, out int count);
            _trackedActions[action.Source] = count + 1;
        }

        if (allWait)
        {
            _waitCount++; 
        }
        else
        {
            if (_waitCount <= 3)
                _waitCount = 0;
        }

        DisplayTime("Done picking best actions");

        Display.ActionSources(_trackedActions);

        _timer.Stop();

        return chosenActions;
    }

    private bool HasHarvestedAllProteins()
    {
        if (_harvestedAProteins > 0 && 
            _harvestedBProteins > 0 && 
            _harvestedCProteins > 0 && 
            _harvestedDProteins > 0)
        {
            return true;
        }

        return false;
    }

    private List<Action> GetEndGameDestroyMoves(Organism organism)
    {
        foreach (Protein protein in Proteins)
        {
            foreach (Point direction in _directions)
            {
                Point checkPoint = new Point(protein.Position.X + direction.X,
                                             protein.Position.Y + direction.Y);

                if (CheckBounds(checkPoint))
                {
                    // if organ is on the check point create an action
                    if (organism.Organs.Any(organ => organ.Position == checkPoint))
                    {
                        Organ organ = organism.Organs.Single(organ => organ.Position == checkPoint);

                        Console.Error.WriteLine($"Found organ to destroy: {organ.Position.X},{organ.Position.Y}");
                        // Create 4 grow type actions for tis check point 
                        return CreateGrowActions(organism.RootId, organ.Id, protein.Position, 0, ActionSource.END_GAME_DESTROY).ToList();
                    }
                }
            }
        }

        return new List<Action>();
    }

    private bool CheckBounds(Point checkPoint)
    {
        if (checkPoint.X < 0 || checkPoint.X >= Width || checkPoint.Y < 0 || checkPoint.Y >= Height)
        {
            return false;
        }

        return true;
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
        opponentTentaclePath = new bool[Width, Height];
        opponentOrganChildren = new int[Width, Height];

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

                int childCount = GetChildCount(organism.RootId, organ);
                opponentOrganChildren[organ.Position.X, organ.Position.Y] = childCount;

                // We can't walk on an outward facing tentacle
                // So add these to the isBlocked list and not to the valid edges

                // North
                if (organ.Position.Y - 1 >= 0)
                {
                    opponentOrganEdges[organ.Position.X, organ.Position.Y - 1] = true;

                    if (organ.Type == OrganType.TENTACLE && organ.Direction == OrganDirection.N)
                    {
                        opponentTentaclePath[organ.Position.X, organ.Position.Y - 1] = true;
                    }
                }

                // East
                if (organ.Position.X + 1 < Width)
                {
                    opponentOrganEdges[organ.Position.X + 1, organ.Position.Y] = true;

                    if (organ.Type == OrganType.TENTACLE && organ.Direction == OrganDirection.E)
                    {
                        opponentTentaclePath[organ.Position.X + 1, organ.Position.Y] = true;
                    }
                }

                // South
                if (organ.Position.Y + 1 < Height)
                {
                    opponentOrganEdges[organ.Position.X, organ.Position.Y + 1] = true;

                    if (organ.Type == OrganType.TENTACLE && organ.Direction == OrganDirection.S)
                    {
                        opponentTentaclePath[organ.Position.X, organ.Position.Y + 1] = true;
                    }
                }

                // WEST
                if (organ.Position.X - 1 >= 0)
                {
                    opponentOrganEdges[organ.Position.X - 1, organ.Position.Y] = true;

                    if (organ.Type == OrganType.TENTACLE && organ.Direction == OrganDirection.W)
                    { 
                        opponentTentaclePath[organ.Position.X - 1, organ.Position.Y] = true;
                    }
                }
            }   
        }
    }

    private int GetChildCount(int organismId, Organ organ)
    {
        int count = 0;
        if (OpponentOrganisms.First(o => o.RootId == organismId).Organs.Any(o => o.ParentId == organ.Id))
        {
            List<Organ> children = OpponentOrganisms.First(o => o.RootId == organismId).Organs.Where(o => o.ParentId == organ.Id).ToList();

            count += children.Count;

            foreach (Organ child in children)
            {
                count += GetChildCount(organismId, child);
            }
        }

        return count;
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
            if (protein.IsHarvested || isBlocked[protein.Position.X, protein.Position.Y] || opponentTentaclePath[protein.Position.X, protein.Position.Y])
            {
                continue;
            }

            foreach (var organ in organism.Organs)
            {
                int manhattanDistance = MapChecker.CalculateManhattanDistance(organ.Position, protein.Position);

                if (manhattanDistance > maxDistance)
                {
                    continue;
                }

                List<Point> path = aStar.GetShortestPath(organ.Position, protein.Position, maxDistance, growStrategy, false);

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

    private List<Action> CheckForTentacleAction(Organism organism)
    {
        Console.Error.WriteLine("Checking for tentacle action");
        List<Action> tentacleActions = new List<Action>();

        if (CostCalculator.CanProduceOrgan(OrganType.TENTACLE, PlayerProteinStock))
        {
            List<Action> twoMoveActions = GetShortestPathToOpponent(organism, 2, 2, GrowStrategy.ALL_PROTEINS, false, "Two ply search");
            tentacleActions.AddRange(twoMoveActions);

            // If we didn't find a path check we're just not seeing it because it's too close
            // Note these ones are always 2 long
            foreach (Organ organ in organism.Organs)
            {
                foreach (Point dir in _directions)
                {
                    Point checkPoint = new Point(organ.Position.X + dir.X, organ.Position.Y + dir.Y);

                    if (CheckBounds(checkPoint) && MapChecker.CanGrowOn(checkPoint, this, GrowStrategy.ALL_PROTEINS, false))
                    {
                        List<Action> tooShortActions = GetShortestPathToOpponent(checkPoint, 2, 2, GrowStrategy.ALL_PROTEINS, organism.RootId, organ.Id, "Next door search");

                        tentacleActions.AddRange(tooShortActions);
                    }
                }
            }

            if (tentacleActions.Count == 0)
            {
                List<Action> threeMoveActions = GetShortestPathToOpponent(organism, 3, 3, GrowStrategy.ALL_PROTEINS, true, "Three ply search");
                tentacleActions.AddRange(threeMoveActions);
            }

            if (tentacleActions.Count == 0)
            {
                List<Action> fourMoveActions = GetShortestPathToOpponent(organism, 4, 4, GrowStrategy.ALL_PROTEINS, true, "Four ply search");
                tentacleActions.AddRange(fourMoveActions);
            }
        }

        return tentacleActions;
    }

    private List<Action> GetShortestPathToOpponent(Organism organism, 
                                                    int minDistance, 
                                                    int maxDistance, 
                                                    GrowStrategy growStrategy, 
                                                    bool canWalkOnOpponentTentaclePaths,
                                                    string source)
    {
        List<Action> actions = new List<Action>();

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

                    List<Point> path = aStar.GetShortestPath(organ.Position, opponentOrgan.Position, maxDistance, growStrategy, canWalkOnOpponentTentaclePaths);
                    
                    if (path.Count >= minDistance && path.Count <= maxDistance && !opponentTentaclePath[path[0].X, path[0].Y])
                    {
                        OrganDirection? direction = null;

                        // If it's a direct attack then face it. Otherwise get the direction right
                        if (path.Count == 2 || path.Count == 3)
                        {
                            direction = _directionCalculator.GetDirection(path[0], path[1]);
                        }
                        else
                        { 
                            direction = _directionCalculator.CalculateClosestOpponentDirection(path[0], path[path.Count - 1]);
                        }

                        Point target = path[path.Count - 1];
                        int childCount = opponentOrganChildren[target.X, target.Y];

                        actions.Add(new Action()
                        {
                            OrganismId = organism.RootId,
                            ActionType = ActionType.GROW,
                            OrganId = organ.Id,
                            TargetPosition = path[0],
                            OrganType = OrganType.TENTACLE,
                            OrganDirection = direction,
                            Score = 500 + childCount,
                            Source = ActionSource.CHECK_FOR_TENTACLES
                        });
                    }   
                }
            }
        }

        return actions;
    }

    private List<Action> GetShortestPathToOpponent(Point point, int minDistance, int maxDistance, GrowStrategy growStrategy, int organismId, int organId, string source)
    {
        List<Action> actions = new List<Action>();

        AStar aStar = new AStar(this);

        foreach (Organism opponentOrganism in OpponentOrganisms)
        {
            foreach (Organ opponentOrgan in opponentOrganism.Organs)
            {
                int manhattanDistance = MapChecker.CalculateManhattanDistance(point, opponentOrgan.Position);

                if (manhattanDistance > maxDistance)
                {
                    continue;
                }

                List<Point> path = aStar.GetShortestPath(point, opponentOrgan.Position, maxDistance, growStrategy, false);

                if (path.Count >= minDistance && path.Count <= maxDistance && opponentTentaclePath[path[0].X, path[0].Y])
                {
                    OrganDirection? direction = null;


                    // If it's a direct attack then face it. Otherwise get the direction right
                    if (path.Count == 2 || path.Count == 3)
                    {
                        direction = _directionCalculator.GetDirection(path[0], path[1]);
                    }
                    else
                    {
                        direction = _directionCalculator.CalculateClosestOpponentDirection(path[0], path[path.Count - 1]);
                    }

                    actions.Add(new Action()
                    {
                        OrganismId = organismId,
                        ActionType = ActionType.GROW,
                        OrganId = organId,
                        TargetPosition = path[0],
                        OrganType = OrganType.TENTACLE,
                        OrganDirection = direction,
                        Score = 500, // Tentacle moves are higher than the rest by default
                        Source = ActionSource.CHECK_FOR_TENTACLES
                    });
                }
            }
        }

        return actions;
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
        int noStockScore = 48;
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

                    if (PlayerProteinStock.A <= 1)
                    {
                        proteinAction.Score += noStockScore;
                    }
                }
                else if (proteinAction.GoalProteinType == ProteinType.B && _harvestedBProteins < 1)
                {
                    proteinAction.Score += notHarvestingScore;

                    if (PlayerProteinStock.B <= 1)
                    {
                        proteinAction.Score += noStockScore;
                    }
                }
                else if (proteinAction.GoalProteinType == ProteinType.C && _harvestedCProteins < 1)
                {
                    proteinAction.Score += notHarvestingScore;
                    proteinAction.Score += harvesterProducingProteinScore;

                    if (PlayerProteinStock.C <= 1)
                    {
                        proteinAction.Score += noStockScore;
                        proteinAction.BlockC = true;
                    }
                }
                else if (proteinAction.GoalProteinType == ProteinType.D && _harvestedDProteins < 1)
                {
                    proteinAction.Score += notHarvestingScore;
                    proteinAction.Score += harvesterProducingProteinScore;

                    if (PlayerProteinStock.D <= 1)
                    {
                        proteinAction.Score += noStockScore;
                        proteinAction.BlockD = true;
                    }
                }

            }
            else if (proteinAction.GoalType == GoalType.CONSUME)
            {
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

                    if (CheckBounds(checkPoint) == false)
                    {
                        break;
                    }

                    if (distance >= minRootSporerDistance)
                    {
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

                    if (!MapChecker.CanGrowOn(checkPoint, this, GrowStrategy.ALL_PROTEINS, false))
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
                    OrganType = OrganType.ROOT,

                    Source = ActionSource.CHECK_FOR_ROOT
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

                if (organPoint.Y+1 < Height)
                {
                    directions.Add(new Point(0, 1));
                }

                if (organPoint.Y > 0)
                {
                    directions.Add(new Point(0, -1));
                }

                if (organPoint.X+1 < Width)
                {
                    directions.Add(new Point(1, 0));
                }

                if (organPoint.X > 0)
                {
                    directions.Add(new Point(-1, 0));
                }

                // Check the four points around the organ
                foreach (Point side in directions)
                {
                    Point sporerPoint = new Point(organPoint.X + side.X,
                                                  organPoint.Y + side.Y);

                    if (!MapChecker.CanGrowOn(sporerPoint, this, GrowStrategy.NO_PROTEINS, false))
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

                            if (!MapChecker.CanGrowOn(checkPoint, this, GrowStrategy.ALL_PROTEINS, false))
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
                    Source = ActionSource.CHECK_FOR_SPORER
                };
            }
        }

        return null;
    }

    private List<Action> GetDesperateDestructiveMove(Organism organism, GrowStrategy growStrategy, int score, ActionSource actionSource)
    {
        List<Action> possibleActions = new List<Action>();

        (int closestOrgan, List<Point> shortestPath) = GetShortestPathToProtein(organism, Proteins, 1, 5, growStrategy);

        if (closestOrgan != -1)
        {
            if (!(hasHarvestedProtein[shortestPath[0].X, shortestPath[0].Y] && !CanFloodFillTo(shortestPath[0], 5)))
            {
                possibleActions.AddRange(CreateGrowActions(organism.RootId,
                                                       closestOrgan,
                                                       shortestPath[0],
                                                       score,
                                                       actionSource));
            }
        }
        return possibleActions;
    }

    private bool CanFloodFillTo(Point startPoint, int minAmount)
    {
        var filledCount = 0;
        var visited = new bool[Width, Height];
        var queue = new Queue<Point>();
        queue.Enqueue(startPoint);
        visited[startPoint.X, startPoint.Y] = true;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            filledCount++;

            if (filledCount >= minAmount)
            {
                return true;
            }

            foreach (Point direction in _directions)
            {
                var nextPoint = new Point(current.X + direction.X, current.Y + direction.Y);

                if (CheckBounds(nextPoint) && !visited[nextPoint.X, nextPoint.Y] && 
                    !isBlocked[nextPoint.X, nextPoint.Y] && 
                    !opponentTentaclePath[nextPoint.X, nextPoint.Y])
                {
                    queue.Enqueue(nextPoint);
                    visited[nextPoint.X, nextPoint.Y] = true;
                }
            }
        }

        return false;
    }

    private IEnumerable<Action> CreateGrowActions(int rootId, int closestOrgan, Point point, int score, ActionSource actionSource)
    {
        List<Action> actions = new List<Action>();

        OrganDirection? closestRootDirection = _directionCalculator.CalculateClosestOpponentDirection(point);

        if (CostCalculator.CanProduceOrgan(OrganType.BASIC, PlayerProteinStock))
        {
            actions.Add(CreateGrowAction(rootId, OrganType.BASIC, closestOrgan, point, null, score + 3, actionSource));
        }

        if (CostCalculator.CanProduceOrgan(OrganType.TENTACLE, PlayerProteinStock))
        {
            actions.Add(CreateGrowAction(rootId, OrganType.TENTACLE, closestOrgan, point, closestRootDirection, score + 2, actionSource));
        }

        if (CostCalculator.CanProduceOrgan(OrganType.SPORER, PlayerProteinStock))
        {
            actions.Add(CreateGrowAction(rootId, OrganType.SPORER, closestOrgan, point, closestRootDirection, score + 1, actionSource));
        }

        if (CostCalculator.CanProduceOrgan(OrganType.HARVESTER, PlayerProteinStock))
        {
            actions.Add(CreateGrowAction(rootId, OrganType.HARVESTER, closestOrgan, point, closestRootDirection, score, actionSource));
        }

        return actions;
    }

    private Action CreateGrowAction(int organismRootId, 
                                    OrganType? organType, 
                                    int organId, 
                                    Point targetPosition, 
                                    OrganDirection? closestRootDirection, 
                                    int score,
                                    ActionSource source)
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
            if (possibleActions.Count > 15)
            {
                break;
            }

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

                if (MapChecker.CanGrowOn(checkPoint, this, GrowStrategy.UNHARVESTED, false))
                {
                    if (!(hasHarvestedProtein[checkPoint.X, checkPoint.Y] && !CanFloodFillTo(checkPoint, 5)))
                    {
                        possibleActions.AddRange(CreateGrowActions(organism.RootId, current.Id, checkPoint, 2, ActionSource.RANDOM_GROW_ACTIONS));
                    }
                }

                if (MapChecker.CanGrowOn(checkPoint, this, GrowStrategy.ALL_PROTEINS, false))
                {
                    foreach (Point d in _directions)
                    {
                        if (MapChecker.CanGrowOn(new Point(checkPoint.X + d.X, checkPoint.Y + d.Y),
                                                this,
                                                GrowStrategy.ALL_PROTEINS,
                                                false))
                        {
                            if (!(hasHarvestedProtein[checkPoint.X, checkPoint.Y] && !CanFloodFillTo(checkPoint, 5)))
                            {
                                possibleActions.AddRange(CreateGrowActions(organism.RootId, current.Id, checkPoint, 1, ActionSource.RANDOM_GROW_ACTIONS));

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
        OrganDirection? direction = _directionCalculator.CalculateClosestOpponentDirection(point);


        bool hasProtein = hasAnyProtein[point.X, point.Y];
        if (CostCalculator.CanProduceOrgan(OrganType.BASIC, PlayerProteinStock))
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
        ProteinStock tempProteinStock = new ProteinStock(PlayerProteinStock.A,
                                                         PlayerProteinStock.B,
                                                         PlayerProteinStock.C,
                                                         PlayerProteinStock.D);

        List<Action> chosenActions = new List<Action>();

        bool[] chosen = new bool[PlayerOrganisms.Count];

        List<Point> targetPositions = new List<Point>();
        List<Point> harvestTargetPositions = new List<Point>();

        bool allChosen = false;

        bool blockC = false;
        bool blockD = false;

        bool madeSporer = false;

        List<Point> goalPositions = new List<Point>();

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

                            if (canCreate)
                            {
                                if (targetPositions.Contains(checkAction.TargetPosition))
                                {
                                    canCreate = false;
                                }

                                if (checkAction.OrganType == OrganType.HARVESTER)
                                {
                                    Point delta = _directionCalculator.GetDelta(checkAction.OrganDirection.Value);

                                    if (harvestTargetPositions.Contains(new Point(checkAction.TargetPosition.X + delta.X,
                                                                                  checkAction.TargetPosition.Y + delta.Y)))
                                    {
                                        canCreate = false;
                                    }
                                }

                                if (blockC)
                                {
                                    if (checkAction.OrganType == OrganType.ROOT ||
                                        checkAction.OrganType == OrganType.HARVESTER ||
                                        checkAction.OrganType == OrganType.TENTACLE)
                                    {
                                        canCreate = false;
                                    }
                                }

                                if (blockD)
                                {
                                    if (checkAction.OrganType == OrganType.ROOT ||
                                        checkAction.OrganType == OrganType.HARVESTER ||
                                        checkAction.OrganType == OrganType.SPORER)
                                    {
                                        canCreate = false;
                                    }
                                }

                                if (checkAction.OrganType == OrganType.SPORER && madeSporer)
                                {
                                    canCreate = false; 
                                }

                                if (checkAction.GoalPosition != new Point(-1, -1) && goalPositions.Contains(checkAction.GoalPosition))
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
                        Console.Error.WriteLine("ERROR: No possible actions for organism");
                        // THIS SHOULD NEVER HAPPEN. MAYBE THROW A WAIT IN JUST IN CASE
                    }
                }   
            }

            chosen[highestOganismIndex] = true;

            Action chosenAction = allPossibleActions[highestScoreIndex][highestActionIndex];
            
            chosenActions.Add(chosenAction);
            targetPositions.Add(chosenAction.TargetPosition);
            if (chosenAction.OrganType == OrganType.HARVESTER)
            {
                // We don't want to harvest the same protein
                Point delta = _directionCalculator.GetDelta(chosenAction.OrganDirection.Value);
                harvestTargetPositions.Add(new Point(chosenAction.TargetPosition.X + delta.X, 
                                                     chosenAction.TargetPosition.Y + delta.Y));

                // We don't want to land on a harvested protein
                targetPositions.Add(new Point(chosenAction.TargetPosition.X + delta.X,
                                             chosenAction.TargetPosition.Y + delta.Y));
            }

            goalPositions.Add(chosenAction.GoalPosition);

            if (chosenAction.BlockC)
            {
                blockC = true;
            }

            if (chosenAction.BlockD)
            {
                blockD = true;
            }

            if (chosenAction.OrganType == OrganType.SPORER)
            {
                madeSporer = true;
            }

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