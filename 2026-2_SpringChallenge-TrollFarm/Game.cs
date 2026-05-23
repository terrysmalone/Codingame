using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection.Metadata.Ecma335;
using System.Xml.Linq;

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

    private List<Point> _targetedTrees = new List<Point>();

    private HashSet<int> _assigned;
    private List<Point> _excludePoints = new List<Point>();

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
        _targetedTrees.Clear();
        _excludePoints.Clear();

        // Set all exclude points to the current troll positions
        foreach (Troll troll in _playerTrolls)
        {
            _excludePoints.Add(troll.Position);
        }

        ResetTrolls();

        _needsManager.SetPriorities();

        List<Point> excludedTrees = new List<Point>();

        Turn++;

        List<string> actions = new List<string>();

        Inventory usableInventory = _playerInventory;

        // If any troll has a fruit, and space for more, and they're on a tree with 
        // that fruit available, then harvest
        foreach (Troll troll in _playerTrolls)
        {
            if (troll.IsCarryingAnyFruit() && troll.CanCarry())
            {
                ResourceType carriedFruitType = troll.CarryingFruitType();

                if(_trees.Any(t => t.Position == troll.Position 
                              && t.Type == carriedFruitType
                              && t.Fruits > 0))
                {
                    Tree harvestableTree = _trees.First(t => t.Position == troll.Position 
                                                        && t.Type == carriedFruitType
                                                        && t.Fruits > 0);

                    Logger.Assign(troll.Id, $"HARVEST {carriedFruitType} at {troll.Position.X},{troll.Position.Y} to PLANT it");
                    actions.Add($"HARVEST {troll.Id}");
                    AssignTroll(troll, troll.Position);                   
                }
            }
        }

        // Try to assign all priorities until we're out of trolls
        List<Need> priorities = _needsManager.GetPriorities();
        Logger.Prioirities(priorities);

        foreach (Need need in priorities)
        {
            if (_assigned.Count >= _playerTrolls.Count)
            {
                break;
            }

            Logger.Message($"Trying to meet need {need}");

            if (need == Need.HarvestAnyWood)
            {
                // Every troll in the game should be in one of the below states. 

                // If there is a nearby tree attack it
                List<Point> orderedTrees = _trees.Where(t => !_targetedTrees.Contains(t.Position)).OrderBy(t => t.Size).ThenBy(t => GetManhattanDistance(t.Position, _playerShack)).Select(t => t.Position).ToList();

                Point closeTree = new Point(-1, -1);

                foreach (Point tree in orderedTrees)
                {
                    if (GetManhattanDistance(tree, _playerShack) > 3)
                    {
                        continue;
                    }

                    int dist = _positionUtil.GetShortestPath(_playerShack, tree, _excludePoints).Count;

                    if (dist <= 3)
                    {
                        closeTree = _trees.First(t => t.Position == tree).Position;
                        break;
                    }
                }

                if (closeTree != new Point(-1, -1))
                {
                    // check if a troll is on the tree
                    Troll? trollOnTree = _playerTrolls.Where(t => !_assigned.Contains(t.Id) && t.CanCarry() && t.Position == closeTree).FirstOrDefault();

                    if (trollOnTree != null)
                    {
                        Logger.Assign(trollOnTree.Id, $"CHOP tree at {closeTree.X},{closeTree.Y}");
                        actions.Add($"CHOP {trollOnTree.Id}");
                        AssignTroll(trollOnTree, trollOnTree.Position);
                        _targetedTrees.Add(closeTree);
                        continue;
                    }
                    else
                    {
                        // Get closest troll to tree
                        Troll? closestTroll = _playerTrolls.Where(t => !_assigned.Contains(t.Id) && t.CanCarry()).OrderBy(t => _positionUtil.GetShortestPath(t.Position, closeTree, _excludePoints).Count).FirstOrDefault();

                        if (closestTroll != null)
                        {
                            Point? nextMoveToSpot = FindNextMoveToPoint(closestTroll, closeTree);
                            if (nextMoveToSpot != null)
                            {
                                Logger.Assign(closestTroll.Id, $"MOVE to tree at {nextMoveToSpot.Value.X},{nextMoveToSpot.Value.Y} to chop");
                                actions.Add($"MOVE {closestTroll.Id} {nextMoveToSpot.Value.X} {nextMoveToSpot.Value.Y}");
                                AssignTroll(closestTroll, nextMoveToSpot.Value);
                                _targetedTrees.Add(closeTree);
                                continue;
                            }
                        }
                    }
                }

                // If a troll is carrying a seed plant
                Troll? carryingSeedTroll = _playerTrolls.Where(t => !_assigned.Contains(t.Id) && t.IsCarryingAnyFruit()).FirstOrDefault();

                if (carryingSeedTroll != null)
                {
                    Logger.Message($"Troll {carryingSeedTroll.Id} is carrying a seed");
                    // plant it where it is if possible
                    if (IsGrowable(carryingSeedTroll.Position) && !HasTree(carryingSeedTroll.Position))
                    {
                        Logger.Assign(carryingSeedTroll.Id, $"PLANT {carryingSeedTroll.CarryingFruitType()} at {carryingSeedTroll.Position.X},{carryingSeedTroll.Position.Y}");
                        actions.Add($"PLANT {carryingSeedTroll.Id} {carryingSeedTroll.CarryingFruitType()}");
                        AssignTroll(carryingSeedTroll, carryingSeedTroll.Position);
                        continue;
                    }
                    else
                    {
                        Logger.Assign(carryingSeedTroll.Id, $"MOVE to grow spot for {carryingSeedTroll.CarryingFruitType()}");
                        Point growSpot = _positionUtil.GetClosestGrowableSpot(carryingSeedTroll.Position);

                        Point? nextMoveToSpot = FindNextMoveToPoint(carryingSeedTroll, growSpot);
                        if (nextMoveToSpot != null)
                        {
                            actions.Add($"MOVE {carryingSeedTroll.Id} {nextMoveToSpot.Value.X} {nextMoveToSpot.Value.Y}");
                            AssignTroll(carryingSeedTroll, nextMoveToSpot.Value);
                            continue;
                        }                        
                    }
                }

                // if a troll is carrying wood
                Troll? carryingWoodTroll = _playerTrolls.Where(t => !_assigned.Contains(t.Id) && t.IsCarryingWood()).FirstOrDefault();

                if (carryingWoodTroll != null)
                {
                    if (IsNextToShack(carryingWoodTroll.Position))
                    {
                        Logger.Assign(carryingWoodTroll.Id, $"DROP wood at shack");
                        actions.Add($"DROP {carryingWoodTroll.Id}");
                        AssignTroll(carryingWoodTroll, carryingWoodTroll.Position);
                        continue;
                    }
                    else
                    {
                        Point nextMoveToShack = FindNextMoveToShack(carryingWoodTroll);

                        Logger.Assign(carryingWoodTroll.Id, $"MOVE to shack at {nextMoveToShack.X},{nextMoveToShack.Y} to drop wood");
                        actions.Add($"MOVE {carryingWoodTroll.Id} {nextMoveToShack.X} {nextMoveToShack.Y}");
                        AssignTroll(carryingWoodTroll, nextMoveToShack);
                        continue;
                    }
                }

                // if a troll is carrying iron
                Troll? carryingironTroll = _playerTrolls.Where(t => !_assigned.Contains(t.Id) && t.IsCarryingIron()).FirstOrDefault();

                if (carryingironTroll != null)
                {
                    if (IsNextToShack(carryingironTroll.Position))
                    {
                        Logger.Assign(carryingironTroll.Id, $"DROP iron at shack");
                        actions.Add($"DROP {carryingironTroll.Id}");
                        AssignTroll(carryingironTroll, carryingironTroll.Position);
                        continue;
                    }
                    else
                    {
                        Point nextMoveToShack = FindNextMoveToShack(carryingironTroll);

                        Logger.Assign(carryingironTroll.Id, $"MOVE to shack to drop iron");
                        actions.Add($"MOVE {carryingironTroll.Id} {nextMoveToShack.X} {nextMoveToShack.Y}");
                        AssignTroll(carryingironTroll, nextMoveToShack);
                        continue;
                    }
                }

                // Go get a seed
                // Get the closest unassigned troll to the shack with inventory space
                if (InventoryUtil.DoesContainFruit(_playerInventory))
                {
                    Troll? closestTroll = _playerTrolls.Where(t => !_assigned.Contains(t.Id) && t.CanCarry()).OrderBy(t => _positionUtil.GetShortestPath(t.Position, _playerShack, _excludePoints).Count).FirstOrDefault();
                    
                    if (closestTroll != null)
                    {
                        if (IsNextToShack(closestTroll.Position))
                        {
                            Logger.Assign(closestTroll.Id, $"PICK any fruit at shack to plant");
                            actions.Add($"PICK {closestTroll.Id} {InventoryUtil.GetAnyFruitType(usableInventory)}");
                            usableInventory = InventoryUtil.ChangeInventory(usableInventory, InventoryUtil.GetAnyFruitType(usableInventory), -1);
                            AssignTroll(closestTroll, closestTroll.Position);
                            continue;
                        }
                        else
                        {
                            Point nextMoveToShack = FindNextMoveToShack(closestTroll);

                            Logger.Assign(closestTroll.Id, $"MOVE to shack to get seed for planting");
                            actions.Add($"MOVE {closestTroll.Id} {nextMoveToShack.X} {nextMoveToShack.Y}");
                            AssignTroll(closestTroll, nextMoveToShack);
                            continue;
                        }
                    }
                }

                // If there is a far away tree attack it
            }

            // If a need can't be met log it and remove it
            if (need == Need.AttackEnemy)
            {
                string attackMove = CalculateAttackMove();

                if (!string.IsNullOrEmpty(attackMove))
                {
                    actions.Add(attackMove);
                    continue;
                }
            }

            if (NeedsManager.IsGrowNeed(need))
            {
                ResourceType fruitType = NeedsManager.GetTreeType(need);

                // if a troll has a fruit of this type
                if (_playerTrolls.Any(t => t.IsCarryingFruit(fruitType) > 0))
                {
                    // find closest plant spot for this fruit
                    Point growSpot = _positionUtil.GetBestGrowSpot();

                    if (growSpot.X == -1 && growSpot.Y == -1)
                    {
                        Logger.Error("No grow spot found for " + fruitType);
                        continue;
                    }

                    // Get all trolls that are carrying this fruit and are not assigned yet
                    List<Troll> candidateTrolls = _playerTrolls
                        .Where(t => !_assigned.Contains(t.Id) && t.IsCarryingFruit(fruitType) > 0)
                        .ToList();

                    (Troll? closestTroll, List<Point> shortestPath) = _positionUtil.GetClosestTrollToTarget(candidateTrolls, growSpot, _excludePoints);

                    if (closestTroll != null)
                    {
                        if (closestTroll.Position == growSpot)
                        {
                            Logger.Assign(closestTroll.Id, $"PLANT {fruitType} at {growSpot.X},{growSpot.Y}");
                            actions.Add($"PLANT {closestTroll.Id} {fruitType.ToString()}");
                            AssignTroll(closestTroll, growSpot);
                            usableInventory = InventoryUtil.ChangeInventory(usableInventory, fruitType, -1);
                            continue;
                        }
                        else
                        {
                            Point? nextMoveToSpot = FindNextMoveToPoint(closestTroll, growSpot);
                            if (nextMoveToSpot != null)
                            {
                                Logger.Assign(closestTroll.Id, $"MOVE to grow spot at {nextMoveToSpot.Value}");
                                actions.Add($"MOVE {closestTroll.Id} {nextMoveToSpot.Value.X} {nextMoveToSpot.Value.Y}");
                                AssignTroll(closestTroll, nextMoveToSpot.Value);

                                continue;
                            }
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

                    if (closestTroll != null)
                    {
                        if (closestTroll.Position == nextMove)
                        {
                            // It's on the target, either harvest or pick
                            if (_positionUtil.IsRipeTreeAtPosition(closestTroll.Position, fruitType))
                            {
                                Logger.Assign(closestTroll.Id, $"HARVEST {fruitType} at {closestTroll.Position.X},{closestTroll.Position.Y} to PLANT it");
                                actions.Add($"HARVEST {closestTroll.Id}");
                                AssignTroll(closestTroll, closestTroll.Position);
                                continue;
                            }
                            else
                            {
                                Logger.Assign(closestTroll.Id, $"PICK {fruitType} at {closestTroll.Position.X},{closestTroll.Position.Y}");
                                actions.Add($"PICK {closestTroll.Id} {fruitType.ToString()}");
                                usableInventory = InventoryUtil.ChangeInventory(usableInventory, fruitType, -1);
                                AssignTroll(closestTroll, closestTroll.Position);
                                continue;
                            }
                        }
                        else
                        {
                            Point? nextMoveToSpot = FindNextMoveToPoint(closestTroll, nextMove);
                            if (nextMoveToSpot != null)
                            {
                                Logger.Assign(closestTroll.Id, $"MOVE towards {fruitType} target at {nextMoveToSpot.Value.X},{nextMoveToSpot.Value.Y}");
                                actions.Add($"MOVE {closestTroll.Id} {nextMoveToSpot.Value.X} {nextMoveToSpot.Value.Y}");
                                AssignTroll(closestTroll, nextMoveToSpot.Value);
                                continue;
                            }
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
                        AssignTroll(troll, troll.Position);
                        usableInventory = InventoryUtil.ChangeInventory(usableInventory, fruitType, 1);
                        continue;
                    }

                    // Head for the shack
                    Point nextMoveToShack = FindNextMoveToShack(troll);

                    Logger.Assign(troll.Id, $"MOVE to shack at {nextMoveToShack.X},{nextMoveToShack.Y}to drop off {fruitType}");
                    actions.Add($"MOVE {troll.Id} {nextMoveToShack.X} {nextMoveToShack.Y}");
                    AssignTroll(troll, nextMoveToShack);
                    continue;                    
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
                        candidatePoints = _trees.Where(t => t.Type == fruitType && t.Fruits > 0).OrderBy(t => GetManhattanDistance(t.Position, _playerShack)).Select(t => t.Position).ToList();
                    }

                    // If we can harvest or mine then do it
                    string getAction = CanWeGetIt(candidateTrolls, candidatePoints, need);

                    if (!string.IsNullOrEmpty(getAction))
                    {
                        actions.Add(getAction);
                        continue;
                    }

                    (Troll? closestTroll, List<Point> shortestPath) = _positionUtil.GetClosestTrollToTargets(candidateTrolls, candidatePoints, _excludePoints, 8);

                    if (closestTroll != null && shortestPath.Count > 0)
                    {
                        Point? nextMoveToSpot = FindNextMoveToPoint(closestTroll, shortestPath[shortestPath.Count - 1]);
                        if (nextMoveToSpot != null)
                        {
                            actions.Add($"MOVE {closestTroll.Id} {nextMoveToSpot.Value.X} {nextMoveToSpot.Value.Y}");
                            AssignTroll(closestTroll, nextMoveToSpot.Value);
                            continue;
                        }
                    }
                }

                // TODO: Deal with wood later
            }

            Logger.Error("No need could be met for " + need.ToString());
        }

        foreach (Troll troll in _playerTrolls.Where(t => !_assigned.Contains(t.Id)))
        {
            if (!troll.CanCarry())
            {
                if (_positionUtil.IsAdjacentToShack(troll.Position) || troll.Position == _playerShack)
                {
                    actions.Add($"DROP {troll.Id}");
                    AssignTroll(troll, troll.Position);
                }
                else
                {
                    Point adjacentShack = FindNextMoveToShack(troll);
                    Logger.Message($"Moving to shack via {adjacentShack.X} {adjacentShack.Y}");
                    actions.Add($"MOVE {troll.Id} {adjacentShack.X} {adjacentShack.Y}");
                    AssignTroll(troll, troll.Position);
                }
            }
            else
            {


                // Get the nearest tree that isnt targeted with fruit
                List<Tree> fruitBearingTrees = _trees.Where(t => t.Fruits > 0 && !_targetedTrees.Contains(t.Position))
                                                     .OrderBy(t => GetManhattanDistance(t.Position, troll.Position)).ToList();

                if (fruitBearingTrees.Any(t => t.Position == troll.Position))
                {
                    Logger.Assign(troll.Id, $"HARVEST tree at {troll.Position.X},{troll.Position.Y} to PLANT it");
                    actions.Add($"HARVEST {troll.Id}");
                    AssignTroll(troll, troll.Position);
                    continue;
                }

                List<Point> path = GetPathToClosestTree(troll.Position, fruitBearingTrees, _excludePoints);

                if (path.Count > 0)
                {
                    Point? nextMove = FindNextMoveToPoint(troll, path[path.Count - 1]);

                    if (nextMove != null)
                    {
                        Point nextMoveToTree = path[0];
                        Logger.Assign(troll.Id, $"MOVE towards tree at {nextMove.Value.X},{nextMove.Value.Y} to harvest");
                        actions.Add($"MOVE {troll.Id} {nextMove.Value.X} {nextMove.Value.Y}");
                        AssignTroll(troll, nextMoveToTree);
                        continue;
                    }
                }
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

    private Point FindNextMoveToShack(Troll troll)
    {
        // Find a spot adjacent to a shack that is empty or that we know will be empty next move.
        List<Point> shackAdjacentPoints = _positionUtil.GetAdjacentToShackPoints();
        List<Point> validAdjacentPoints = new List<Point>();

        foreach (Point adjacent in shackAdjacentPoints)
        {
            if (_isWalkable[adjacent.Y, adjacent.X] && WillBeFreeNextTurn(adjacent))
            {
                validAdjacentPoints.Add(adjacent);
            }
        }

        Logger.Message($"validAdjacentPoints after checking what's free: {string.Join(", ", validAdjacentPoints.Select(p => $"({p.X},{p.Y})"))}");

        // If there are no valid adjacent points loosen the criteria
        if (validAdjacentPoints.Count == 0)
        {
            foreach (Point adjacent in shackAdjacentPoints)
            {
                if (_isWalkable[adjacent.Y, adjacent.X])
                {
                    validAdjacentPoints.Add(adjacent);
                }
            }
        }

        // Just path to the shack
        if (validAdjacentPoints.Count == 0)
        {
            List<Point> path = _positionUtil.GetShortestPath(troll.Position, _playerShack, _excludePoints);

            if (path.Count == 0 || path.Count <= troll.MovementSpeed)
            {
                return _playerShack;
            }
            else
            {
                return path[troll.MovementSpeed - 1];
            }
        }

        // Order by shortest distance to the troll
        validAdjacentPoints = validAdjacentPoints.OrderBy(p => GetManhattanDistance(p, troll.Position)).ToList();

        foreach (Point adjacent in validAdjacentPoints)
        {
            Point? nextMove = FindNextMoveToPoint(troll, adjacent);

            if (nextMove == null)
            {
                continue;
            }

            return nextMove.Value;
        }

        return _playerShack;
    }

    private Point? FindNextMoveToPoint(Troll troll, Point target)
    {
        // Pathfind to it
        List<Point> path = _positionUtil.GetShortestPath(troll.Position, target, _excludePoints);

        if (path.Count == 0)
        {
            return null;
        }

        if (path.Count <= troll.MovementSpeed)
        {
            return target;
        }
        else
        {
            Point nextMove = path[troll.MovementSpeed - 1];

            if (WillBeFreeNextTurn(nextMove))
            {
                return nextMove;
            }
        }

        return null;
    }

    private bool WillBeFreeNextTurn(Point point)
    {
        foreach (Troll troll in _playerTrolls)
        {
            // If we don't know a trolls next move use the current one, 
            // otherwise use the next move to check if the point will be free
            if (troll.NextMove == null && troll.Position == point)
            {
                return false;
            }

            if (troll.NextMove != null && troll.NextMove == point)
            {
                return false;
            }
        }

        return true;
    }

    private string CalculateAttackMove()
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
                AssignTroll(troll, troll.Position);
                return $"CHOP {troll.Id}";
            }
        }


        (Troll? closestTroll, List<Point> path) = _positionUtil.GetClosestTrollToTargets(_playerTrolls.Where(t => !_assigned.Contains(t.Id)).ToList(), candidateTrees, _excludePoints);

        if (closestTroll != null && path.Count > 0)
        {
            Logger.Message($"Moving troll {closestTroll.Id} towards enemy for attack with next move {path[0]}");
            AssignTroll(closestTroll, path[0]);
            return $"MOVE {closestTroll.Id} {path[^1].X} {path[^1].Y}";
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
                        Logger.Assign(troll.Id, $"MINE iron at {ironSpot.X},{ironSpot.Y}");
                        AssignTroll(troll, troll.Position);
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
                        Logger.Assign(troll.Id, $"HARVEST {point.X},{point.Y}");
                        AssignTroll(troll, troll.Position);
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
                return (closestTroll, closestTroll.Position);
            }

            (closestTroll, closestPath) = _positionUtil.GetClosestTrollToTarget(eligibleTrolls, _playerShack, _excludePoints);

            if (closestTroll.Position == _playerShack || _positionUtil.IsAdjacentToShack(closestTroll.Position) || closestPath.Count == 0)
            {
                nextMove = closestTroll.Position;
            }
            else
            {
                nextMove = _playerShack;

            }
        }

        // TODO:
        // Get all harvestable trees of this type
        // List<Tree> harvestableTrees = _trees.Where(t => t.Type == fruitType && t.Fruits > 0).ToList();

        return (closestTroll, nextMove);
    }

    private void AssignTroll(Troll troll, Point nextPoint)
    {
        _assigned.Add(troll.Id);

        troll.NextMove = nextPoint;
        _excludePoints.Remove(troll.Position);

        _excludePoints.Add(nextPoint);
    }

    private void ResetTrolls()
    {
        _assigned.Clear();

        foreach (Troll troll in _playerTrolls)
        {
            troll.NextMove = null;
        }
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
        List<Point> path = _positionUtil.GetShortestPath(position, _playerShack, _excludePoints);
       
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

            List<Point> path = _positionUtil.GetShortestPath(position, tree.Position, _excludePoints);

            //Logger.Path($"Tree {tree.Position}", path);

            if (path.Count > 0 && path.Count < closestDistance)
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

    internal List<Tree> GetTrees()
    {
        return _trees;
    }
}