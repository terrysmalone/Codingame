using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.WebSockets;

namespace SpringChallenge2026;

internal class Game
{
    internal int Turn { get; private set ;} = 0;

    private readonly int _width;
    private readonly int _height;

    private Point _playerShack;
    private Point _enemyShack;

    private Inventory _playerInventory;
    private Inventory _enemyInventory;

    private List<Troll> _playerTrolls = new List<Troll>();
    private List<Troll> _enemyTrolls = new List<Troll>();
    private List<Tree> _trees = new List<Tree>();
    private List<Point> _iron = new List<Point>();

    private NeedsManager _needsManager;
    private PositionUtil _positionUtil;

    private bool[,] _isWalkable;
    private bool[,] _isWater;

    private HashSet<int> _assigned;

    public Game(int width, int height)
    {
        _width = width;
        _height = height;
        
        _isWalkable = new bool[height, width];
        _isWater = new bool[height, width];

        _assigned = new HashSet<int>();
    }

    internal void Initialise()
    {
        _positionUtil = new PositionUtil(this, new PathFinder(_width, _height, _isWalkable));
        _positionUtil.InitialiseBestGrowSpots();

        _needsManager = new NeedsManager(this, _positionUtil);
    }

    internal List<string> GetActions()
    {
        SetAllTrollsToUnassigned();

        _needsManager.SetPriorities();

        List<Point> excludedTrees = new List<Point>();

        // Logger.Inventory("Player inventory", _playerInventory);
        // Logger.Inventory("Enemy inventory", _enemyInventory);

        Turn++;

        List<string> actions = new List<string>();

        Inventory usableInventory = _playerInventory;


        // Try to assign all priorities until we're out of trolls
        List<Need> priorities = _needsManager.GetPriorities();

        Logger.Prioirities(priorities);

        // Logger.Trolls(_playerTrolls);

        foreach (Need need in priorities)
        {
            if (_assigned.Count >= _playerTrolls.Count)
            {
                break;
            }

            // Find all trolls that can meet the need
            // Choose the best one
            // Mark it as assigned
            
            if (need == Need.HarvestAnyWood)
            {
                // if there are no trees near my shack
                // and I have fruit in my shack
                // Plant tree near shack
                Tree? closestTree = _trees.Where(t => t.Fruits > 0).OrderBy(t => _positionUtil.GetShortestPath(_playerShack, t.Position).Count).FirstOrDefault();

                if (closestTree != null)
                {
                    int distance = _positionUtil.GetShortestPath(_playerShack, closestTree.Value.Position).Count;

                    if (distance > 3)
                    {
                        // Get closest troll to shack
                       
                        Troll? closestTroll = _playerTrolls.Where(t => !_assigned.Contains(t.Id)).OrderBy(t => _positionUtil.GetShortestPath(t.Position, _playerShack).Count).FirstOrDefault();

                        if (closestTroll != null)
                        {
                            if (_positionUtil.IsAdjacentToShack(closestTroll.Position))
                            {
                                if(_trees.Any(t => t.Position == closestTroll.Position))
                                {
                                    actions.Add($"CHOP {closestTroll.Id}");
                                    AssignTroll(closestTroll.Id);
                                    continue;
                                }
                                if (!closestTroll.IsCarryingAnyFruit())
                                {
                                    if (closestTroll.IsCarryingWood())
                                    {
                                        actions.Add($"DROP {closestTroll.Id}");
                                        AssignTroll(closestTroll.Id);
                                        continue;
                                    }

                                    var fruitType = InventoryUtil.GetAnyFruitType(_playerInventory);
                                    actions.Add($"PICK {closestTroll.Id} {fruitType.ToString()}");
                                    AssignTroll(closestTroll.Id);
                                    usableInventory = InventoryUtil.ChangeInventory(usableInventory, fruitType, -1);
                                    continue;
                                }
                                else
                                {
                                    ResourceType fruitType = closestTroll.CarryingFruitType();
                                    
                                    actions.Add($"PLANT {closestTroll.Id} {fruitType}");
                                    AssignTroll(closestTroll.Id);
                                    continue;
                                }
                            }
                        
                            actions.Add($"MOVE {closestTroll.Id} {_playerShack.X} {_playerShack.Y}");
                            AssignTroll(closestTroll.Id);
                            continue;
                        }
                    }
                }

                var freeTrolls = _playerTrolls.Where(t => !_assigned.Contains(t.Id)).ToList();

                if (freeTrolls != null && freeTrolls.Count > 0)
                {
                    // if an unassigned troll is carrying anything take it to the shack
                    var troll = freeTrolls.FirstOrDefault(t => !t.CanCarry());

                    if (troll != null)
                    {
                        if (_positionUtil.IsAdjacentToShack(troll.Position) || troll.Position == _playerShack)
                        {
                            actions.Add($"DROP {troll.Id}");
                            AssignTroll(troll.Id);
                            continue;
                        }
                        // Head for the shack
                        List<Point> shortestPath = _positionUtil.GetShortestPath(troll.Position, _playerShack);
                        if (shortestPath.Count > 0)
                        {
                            Logger.Message($"Closest troll to move for need {need} is {troll.Id} at position {troll.Position} with next move {shortestPath[0]}");
                            Point nextMove = shortestPath[0];
                            actions.Add($"MOVE {troll.Id} {nextMove.X} {nextMove.Y}");
                            AssignTroll(troll.Id);
                            continue;
                        }
                    }

                    // if a troll has space and is at a tree chop it
                    var trollAtTree = freeTrolls.FirstOrDefault(t => t.CanCarry() && IsAtTree(t.Position, _trees));

                    if (trollAtTree != null)
                    {
                        actions.Add($"CHOP {trollAtTree.Id}");
                        AssignTroll(trollAtTree.Id);
                        continue;
                    }

                    // Move a free troll towards the closest tree
                    List<Troll> availableTrolls = freeTrolls.Where(t => t.CanCarry()).ToList();

                    if (availableTrolls.Count > 0)
                    {
                        (Troll? closestTroll, List<Point> path) = _positionUtil.GetClosestTrollToTargets(availableTrolls, _trees.Select(t => t.Position).ToList());
                        if (closestTroll != null && path.Count > 0)
                        {
                            AssignTroll(closestTroll.Id);
                            actions.Add($"MOVE {closestTroll.Id} {path[0].X} {path[0].Y}");
                            continue;
                        }
                    }
                }
            }

            // If a need can't be met log it and remove it
            if (need == Need.AttackEnemy)
            {
                string attackMove = CalculateAtackMove();

                if (!string.IsNullOrEmpty(attackMove))
                {
                    actions.Add(attackMove);
                    continue;
                }
            }

            if (NeedsManager.IsGrowNeed(need))
            {
                Logger.Message($"Trying to meet grow need {need}");

                ResourceType fruitType = NeedsManager.GetTreeType(need);

                // if a troll has a fruit of this type
                if (_playerTrolls.Any(t => t.IsCarryingFruit(fruitType) > 0))
                {
                    Logger.Message("Troll is carrying fruit" + fruitType + " and trying to find grow spot");
                    // find closest plant spot for this fruit
                    Point growSpot = _positionUtil.GetBestGrowSpot();

                    // Get all trolls that are carrying this fruit and are not assigned yet
                    List<Troll> candidateTrolls = _playerTrolls
                        .Where(t => !_assigned.Contains(t.Id) && t.IsCarryingFruit(fruitType) > 0)
                        .ToList();

                    Logger.Message($"Found grow spot at {growSpot} and {candidateTrolls.Count} candidate trolls to plant with");

                    (Troll? closestTroll, List<Point> shortestPath) = _positionUtil.GetClosestTrollToTarget(candidateTrolls, growSpot);

                    if (closestTroll != null)
                    {
                        if (closestTroll.Position == growSpot)
                        {
                            actions.Add($"PLANT {closestTroll.Id} {fruitType.ToString()}");
                            AssignTroll(closestTroll.Id);
                            usableInventory = InventoryUtil.ChangeInventory(usableInventory, fruitType, -1);
                            continue;
                        }
                        else
                        {
                            Logger.Message($"Closest troll to move for need {need} is {closestTroll.Id} at position {closestTroll.Position} with next move {shortestPath[0]}");
                            actions.Add($"MOVE {closestTroll.Id} {shortestPath[0].X} {shortestPath[0].Y}");
                            AssignTroll(closestTroll.Id);
                            continue;
                        }
                    }
                    else
                    {
                        Logger.Error($"No close troll can be found with {fruitType}");
                    }
                }
                else
                {
                    (Troll? closestTroll, Point nextMove) = GetClosestTrollMove(usableInventory, fruitType);

                    Logger.Message($"Closest troll to move for need {need} is {closestTroll?.Id} at position {closestTroll?.Position} with next move {nextMove}");

                    if (closestTroll != null)
                    {
                        if (closestTroll.Position == nextMove)
                        {
                            // It's on the target, either harvest or pick
                            if (_positionUtil.IsTreeAtPosition(closestTroll.Position, fruitType))
                            {
                                actions.Add($"HARVEST {closestTroll.Id}");
                                AssignTroll(closestTroll.Id);
                                continue;
                            }
                            else
                            {
                                actions.Add($"PICK {closestTroll.Id} {fruitType.ToString()}");
                                usableInventory = InventoryUtil.ChangeInventory(usableInventory, fruitType, -1);
                                AssignTroll(closestTroll.Id);
                                continue;
                            }
                        }
                        else
                        {
                            actions.Add($"MOVE {closestTroll.Id} {nextMove.X} {nextMove.Y}");
                            AssignTroll(closestTroll.Id);
                            continue;
                        }
                    }
                }
            }
            
            if (NeedsManager.IsharvestNeed(need))
            {
                ResourceType fruitType = NeedsManager.GetTreeType(need);

                // if a troll has a fruit of this type
                if (_playerTrolls.Any(t => !_assigned.Contains(t.Id) && t.IsCarryingFruit(fruitType) > 0))
                {
                    Logger.Message($"Troll is carrying fruit {fruitType} and trying to find shack to drop off");
                    var troll = _playerTrolls.First(t => !_assigned.Contains(t.Id) && t.IsCarryingFruit(fruitType) > 0);

                    if (_positionUtil.IsAdjacentToShack(troll.Position) || troll.Position == _playerShack)
                    {
                        actions.Add($"DROP {troll.Id}");
                        AssignTroll(troll.Id);
                        usableInventory = InventoryUtil.ChangeInventory(usableInventory, fruitType, 1);
                        continue;
                    }

                    // Head for the shack
                    List<Point> shortestPath = _positionUtil.GetShortestPath(troll.Position, _playerShack);

                    if (shortestPath.Count > 0)
                    {
                        Point nextMove = shortestPath[0];
                        actions.Add($"MOVE {troll.Id} {nextMove.X} {nextMove.Y}");
                        AssignTroll(troll.Id);
                        continue;
                    }
                }
                else
                {
                    // Find the nearest unassigned troll, with free inventory space, to a tree of this type and send it to harvest
                    List<Troll> candidateTrolls = _playerTrolls.Where(t => !_assigned.Contains(t.Id) && t.CanCarry()).ToList();

                    List<Point> candidatePoints = new List<Point>();

                    if (need == Need.HarvestIron)
                    {
                        candidatePoints = _iron;
                    }
                    else
                    {
                        candidatePoints = _trees.Where(t => t.Type == fruitType && t.Fruits > 0).Select(t => t.Position).ToList();
                    }

                    // If we can harvest or mine then do it
                    string getAction = CanWeGetIt(candidateTrolls, candidatePoints, need);

                    if (!string.IsNullOrEmpty(getAction))
                    {
                        actions.Add(getAction);
                        continue;
                    }

                    Logger.Message("HERE");
                    Logger.Message($"Fruit type: {fruitType}");

                    foreach (var point in candidatePoints)
                    {
                        Logger.Message($"Candidate point: {point}");
                    }

                    (Troll? closestTroll, List<Point> shortestPath) = _positionUtil.GetClosestTrollToTargets(candidateTrolls, candidatePoints, 8);

                    if (closestTroll != null && shortestPath.Count > 0)
                    {
                        actions.Add($"MOVE {closestTroll.Id} {shortestPath[0].X} {shortestPath[0].Y}");
                        AssignTroll(closestTroll.Id);
                        continue;
                    }
                }

                // TODO: Deal with wood later
            }

            Logger.Error("No need could be met for " + need.ToString());
        }

        foreach (Troll troll in _playerTrolls.Where(t => !t.CanCarry() && !_assigned.Contains(t.Id)))
        {
            if (_positionUtil.IsAdjacentToShack(troll.Position) || troll.Position == _playerShack)
            {
                actions.Add($"DROP {troll.Id}");
                AssignTroll(troll.Id);
            }
            else
            {
                actions.Add($"MOVE {troll.Id} {_playerShack.X} {_playerShack.Y}");
                AssignTroll(troll.Id);
            }
            
            
        }

        if (priorities.Contains(Need.TrainTroll))
        {
            Logger.Inventory("Usable inventory for training", usableInventory);
            (int plums, int lemons, int apples, int iron) = TrainingUtil.GetBestTrollTraining(_playerTrolls.Count, usableInventory);
                        
            if (plums > 0 && lemons > 0 && apples > 0 && iron > 0)
            {
                Logger.Message($"Training troll with {plums} plums, {lemons} lemons, {apples} apples and {iron} iron");
                actions.Add($"TRAIN {plums} {lemons} {apples} {iron}");
            }
        }

        return actions;
    }

    private string CalculateAtackMove()
    {
        //Get a list of candidate enemy trees
        Point enemyShack = _enemyShack;

        List<Point> candidateTrees = new List<Point>();

        for (int y = enemyShack.Y - 2; y <= enemyShack.Y + 2; y++)
        {
            for (int x = enemyShack.X - 2; x <= enemyShack.X + 2; x++)
            {
                if (enemyShack.X == x && enemyShack.Y == y)
                {
                    continue;
                }

                if (GetManhattanDistance(new Point(x, y), _playerShack) <= 2)
                {
                    continue;
                }

                Point checkPoint = new Point(x, y);
                if (IsInBounds(checkPoint) && _trees.Any(t => t.Position == checkPoint))
                {
                    candidateTrees.Add(checkPoint);                    
                }
            }
        }

        // If any troll is on any candidate square, chop it
        foreach (Troll troll in _playerTrolls.Where(t => !_assigned.Contains(t.Id)))
        {
            if (candidateTrees.Any(t => t == troll.Position))
            {
                AssignTroll(troll.Id);
                return $"CHOP {troll.Id}";
            }
        }


        (Troll? closestTroll, List<Point> path) = _positionUtil.GetClosestTrollToTargets(_playerTrolls.Where(t => !_assigned.Contains(t.Id)).ToList(), candidateTrees);

        if (closestTroll != null && path.Count > 0)
        {
            Logger.Message($"Moving troll {closestTroll.Id} towards enemy for attack with next move {path[0]}");
            AssignTroll(closestTroll.Id);
            return $"MOVE {closestTroll.Id} {path[path.Count-1].X} {path[path.Count - 1].Y}";
        }

        return string.Empty;
    }

    private int GetManhattanDistance(Point point1, Point point2)
    {
        return Math.Abs(point1.X - point2.X) + Math.Abs(point1.Y - point2.Y);
    }

    private string CanWeGetIt(List<Troll> candidateTrolls, List<Point> candidatePoints, Need need)
    {
        foreach (Troll troll in candidateTrolls)
        {
            if (need == Need.HarvestIron)
            {
                foreach (Point ironSpot in candidatePoints)
                {
                    if (_positionUtil.IsAdjacentTo(troll.Position, ironSpot))
                    {
                        AssignTroll(troll.Id);
                        return $"MINE {troll.Id}";                        
                    }
                }
            }
            else
            {
                foreach (Point point in candidatePoints)
                {
                    if (troll.Position == point)
                    {
                        AssignTroll(troll.Id);
                        return $"HARVEST {troll.Id}";                        
                    }
                }
            }
        }

        return string.Empty;
    }

    private (Troll? closestTroll, Point nextMove) GetClosestTrollMove(Inventory usableInventory, ResourceType fruitType)
    {
        Troll? closestTroll = null;
        Point nextMove = new Point(-1, -1);
        List<Point> closestPath = new List<Point>();

        var eligibleTrolls = _playerTrolls.Where(t => !_assigned.Contains(t.Id) && t.CanCarry()).ToList();

        if (eligibleTrolls.Count == 0)
        {
            return (null, nextMove);
        }

        if (InventoryUtil.DoesContainFruit(usableInventory, fruitType))
        {
            // If a troll is on the shack use that
            closestTroll = eligibleTrolls.FirstOrDefault(t => t.Position == _playerShack || _positionUtil.IsAdjacentToShack(t.Position));

            if (closestTroll != null)
            {
                AssignTroll(closestTroll.Id);
                return (closestTroll, closestTroll.Position);
            }

            (closestTroll, closestPath) = _positionUtil.GetClosestTrollToTarget(eligibleTrolls, _playerShack);

            if (closestTroll.Position == _playerShack || _positionUtil.IsAdjacentToShack(closestTroll.Position) || closestPath.Count == 0)
            {
                nextMove = closestTroll.Position;
            }
            else
            {
                nextMove = closestPath[0];

            }
        }

        // TODO:
        // Get all harvestable trees of this type
        // List<Tree> harvestableTrees = _trees.Where(t => t.Type == fruitType && t.Fruits > 0).ToList();

        return (closestTroll, nextMove);
    }

    private void AssignTroll(int id)
    {
        Logger.Message($"Assigning troll {id} to a need");
        _assigned.Add(id);
    }

    private void SetAllTrollsToUnassigned()
    {
        _assigned.Clear();
    }

    private bool IsAtTree(Point position, List<Tree> trees)
    {
        return trees.Any(tree => tree.Position == position);
    }

    private bool IsNextToShack(Point position)
    {
        if (CalculationUtil.GetManhattanDistance(position, _playerShack) == 1)
        {
            return true;
        }
        
        return false;
    }

    private List<Point> GetShortestPathToShack(Point position)
    {
        List<Point> path = _positionUtil.GetShortestPath(position, _playerShack);
       
        return path;
    }

    private List<Point> GetPathToClosestTree(Point position, List<Tree> treesWithFruit, List<Point> excludedTrees)
    {
        int closestDistance = int.MaxValue;
        List<Point> closestPath = new List<Point>();

        foreach (Tree tree in treesWithFruit)
        {
            if (excludedTrees.Contains(tree.Position))
            {
                continue;
            }

            List<Point> path = _positionUtil.GetShortestPath(position, tree.Position);

            //Logger.Path($"Tree {tree.Position}", path);

            if (path.Count < closestDistance)
            {
                closestDistance = path.Count;
                closestPath = path;
            }
        }

        return closestPath;
    }

    internal void SetPlayerShack(int x, int y)
    {
        _playerShack = new Point(x, y);
    }

    internal void SetEnemyShack(int x, int y)
    {
        _enemyShack = new Point(x, y);
    }

    internal void SetEnemyInventory(Inventory inventory)
    {
        _enemyInventory = inventory;
    }

    internal void SetPlayerInventory(Inventory inventory)
    {
        _playerInventory = inventory;
    }

    internal void ClearTrees()
    {
        _trees.Clear();
    }

    internal void AddTree(Tree tree)
    {
        _trees.Add(tree);
    }

    internal void ClearTrolls()
    {
        _playerTrolls.Clear();
        _enemyTrolls.Clear();
    }

    internal int GetPlayerTrollCount()
    {
        return _playerTrolls.Count;
    }

    internal List<Tree> GetTrees(ResourceType fruitType)
    {
        return _trees.Where(tree => tree.Type == fruitType).ToList();
    }

    internal Point GetPlayerShackPosition()
    {
        return _playerShack;
    }

    internal void AddOrUpdateTroll(
        int id, 
        int player, 
        Point position, 
        int movementSpeed, 
        int carryCapacity, 
        int harvestPower, 
        int chopPower, 
        int carryPlum, 
        int carryLemon, 
        int carryApple, 
        int carryBanana, 
        int carryIron, 
        int carryWood)
    {
        if (player == 0)
        {
            if (_playerTrolls.Any(t => t.Id == id))
            {
                var troll = _playerTrolls.First(t => t.Id == id);
                troll.CarryPlum = carryPlum;
                troll.CarryLemon = carryLemon;
                troll.CarryApple = carryApple;
                troll.CarryBanana = carryBanana;
                troll.CarryWood = carryWood;
                troll.CarryIron = carryIron;
            }
            else
            {
                Troll troll = new Troll(id, position, movementSpeed, carryCapacity, harvestPower, chopPower);
                troll.CarryPlum = carryPlum;
                troll.CarryLemon = carryLemon;
                troll.CarryApple = carryApple;
                troll.CarryBanana = carryBanana;
                troll.CarryWood = carryWood;
                troll.CarryIron = carryIron;

                _playerTrolls.Add(troll);
            }
        }
        else
        {
            if (_enemyTrolls.Any(t => t.Id == id))
            {
                var troll = _enemyTrolls.First(t => t.Id == id);
                troll.CarryPlum = carryPlum;
                troll.CarryLemon = carryLemon;
                troll.CarryApple = carryApple;
                troll.CarryBanana = carryBanana;
                troll.CarryWood = carryWood;
                troll.CarryIron = carryIron;
            }
            else
            {
                Troll troll = new Troll(id, position, movementSpeed, carryCapacity, harvestPower, chopPower);
                troll.CarryPlum = carryPlum;
                troll.CarryLemon = carryLemon;
                troll.CarryApple = carryApple;
                troll.CarryBanana = carryBanana;
                troll.CarryWood = carryWood;
                troll.CarryIron = carryIron;
                _enemyTrolls.Add(troll);
            }
        }
    }

    internal bool IsInBounds(Point spot)
    {
        if(spot.X < 0 || spot.X >= _width || spot.Y < 0 || spot.Y >= _height)
        {
            return false;
        }

        return true;

    }

    internal bool IsGrowable(Point spot)
    {
        if (_isWalkable[spot.Y, spot.X] == true)
        {
            return true;
        }

        return false;
    }

    internal void SetIsWalkable(int x, int y, bool isWalkable)
    {
        _isWalkable[y, x] = isWalkable;
    }

    internal void SetIsWater(int x, int y, bool isWater)
    {
        _isWater[y, x] = isWater;
    }

    internal bool IsWater(Point spot)
    {
        return _isWater[spot.Y, spot.X];
    }

    internal bool HasTree(Point growSpot)
    {
        return _trees.Any(t => t.Position == growSpot);
    }

    internal void AddIron(int x, int y)
    {
        _iron.Add(new Point(x, y));
    }

    internal Inventory GetPlayerInventory()
    {
        return _playerInventory;
    }

    internal List<Point> GetIronPositions()
    {
        return _iron;
    }
}