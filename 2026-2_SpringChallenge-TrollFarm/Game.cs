using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SpringChallenge2026;

internal class Game
{
    private readonly int _width;
    private readonly int _height;

    private List<Point> _playerShacks = new List<Point>();
    private List<Point> _enemyShacks = new List<Point>();

    private Inventory _playerInventory;
    private Inventory _enemyInventory;

    private List<Troll> _playerTrolls = new List<Troll>();
    private List<Troll> _enemyTrolls = new List<Troll>();
    private List<Tree> _trees = new List<Tree>();

    private PathFinder _pathFinder;

    private int _round = 0;

    public Game(int width, int height)
    {
        _width = width;
        _height = height;

        _pathFinder = new PathFinder(width, height);
    }

    internal List<string> GetActions()
    {
        List<Point> excludedTrees = new List<Point>();

        Logger.Inventory("Player inventory", _playerInventory);
        Logger.Inventory("Enemy inventory", _enemyInventory);

        _round++;

        List<string> actions = new List<string>();

        // If we're on the first round make the best troll we can
        if (_round == 1)
        {
            (int plums, int lemons, int apples) = TrainingUtil.GetBestTrollTraining(_playerTrolls.Count, _playerInventory);

            Logger.Message($"Training troll with {plums} plums, {lemons} lemons and {apples} apples");
            if (plums > 0 && lemons > 0 && apples > 0)
            {
                actions.Add($"TRAIN {plums} {lemons} {apples} 0");
            }
        }

        List<Tree> treesWithFruit = _trees.FindAll(tree => tree.Fruits > 0);

        foreach (Troll troll in _playerTrolls)
        {
            Logger.Troll(troll);

            if (troll.TotalCarry >= troll.CarryCapacity)
            {
                if (IsNextToShack(troll.Position))
                {
                    actions.Add($"DROP {troll.Id}");
                }
                else
                {
                    List<Point> pathToClosestShack = GetPathToClosestShack(troll.Position);
                    
                    if (pathToClosestShack.Count > 0)
                    {
                        Point nextPosition = pathToClosestShack[0];
                        actions.Add($"MOVE {troll.Id} {nextPosition.X} {nextPosition.Y}");
                    }
                }
            }
            else
            {
                if (IsAtTree(troll.Position, treesWithFruit))
                {
                    actions.Add($"HARVEST {troll.Id}");
                }
                else
                {
                    List<Point> pathToClosestTree = GetPathToClosestTree(troll.Position, treesWithFruit, excludedTrees);
                    actions.Add($"MOVE {troll.Id} {pathToClosestTree[0].X} {pathToClosestTree[0].Y}");
                    excludedTrees.Add(pathToClosestTree.Last());
                }
            }
            
        }

        return actions;
    }

    private bool IsAtTree(Point position, List<Tree> treesWithFruit)
    {
        return treesWithFruit.Any(tree => tree.Position == position);
    }

    private bool IsNextToShack(Point position)
    {
        foreach (Point shack in _playerShacks)
        {
            if (CalculationUtil.GetManhattanDistance(position, shack) == 1)
            {
                return true;
            }
        }
        return false;
    }

    private List<Point> GetPathToClosestShack(Point position)
    {
        int closestDistance = int.MaxValue;
        List<Point> closestPath = new List<Point>();

        foreach (Point shack in _playerShacks)
        {
            List<Point> path = _pathFinder.GetShortestPath(position, shack);

            if (path.Count < closestDistance)
            {
                closestDistance = path.Count;
                closestPath = path;
            }
        }

        return closestPath;
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

            List<Point> path = _pathFinder.GetShortestPath(position, tree.Position);

            //Logger.Path($"Tree {tree.Position}", path);

            if (path.Count < closestDistance)
            {
                closestDistance = path.Count;
                closestPath = path;
            }
        }

        return closestPath;
    }

    internal void AddPlayerShack(int x, int y)
    {
        _playerShacks.Add(new Point(x, y));
    }

    internal void AddEnemyShack(int x, int y)
    {
        _enemyShacks.Add(new Point(x, y));
    }

    internal void SetEnemyInventory(Inventory inventory)
    {
        _playerInventory = inventory;
    }

    internal void SetPlayerInventory(Inventory inventory)
    {
        _enemyInventory = inventory;
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

    internal void AddPlayerTroll(Troll troll)
    {
        _playerTrolls.Add(troll);
    }

    internal void AddEnemyTroll(Troll troll)
    {
        _enemyTrolls.Add(troll);
    }
}