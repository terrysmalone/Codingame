using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Dynamic;
using System.Linq;
using System.Xml.Linq;

namespace SpringChallenge2026;

internal sealed class NeedsManager
{
    private const int EARLY_GAME_END = 100;
    private const int MID_GAME_END = 175;

    private readonly Game _game;
    private readonly PositionUtil _positionUtil;

    private List<Need> _priorities = new List<Need>();

    public NeedsManager(Game game, PositionUtil positionUtil)
    {
        _game = game;
        _positionUtil = positionUtil;
    }

    internal void SetPriorities()
    {
        _priorities.Clear();

        if (_game.Turn < EARLY_GAME_END || _game.Turn < MID_GAME_END)
        {

            CheckAndAddGrowPriorities();
            CheckAndAddHarvestPriorities();
            CheckAndAddHarvestPriorities(); // Add more as a fall back. No harm in harvesting more if I have a lot of trolls
            _priorities.Add(Need.AttackEnemy);
            _priorities.Add(Need.AttackEnemy);
            _priorities.Add(Need.AttackEnemy);

            _priorities.Add(Need.TrainTroll);
        }
        else
        {
            _priorities.Add(Need.HarvestAnyWood);
            _priorities.Add(Need.HarvestAnyWood);
            _priorities.Add(Need.HarvestAnyWood);
            _priorities.Add(Need.HarvestAnyWood);
            _priorities.Add(Need.HarvestAnyWood);
            _priorities.Add(Need.HarvestAnyWood);
            _priorities.Add(Need.HarvestAnyWood);
            _priorities.Add(Need.HarvestAnyWood);
        }
    }

    private void CheckAndAddHarvestPriorities()
    {
        Logger.Inventory("Player inventory", _game.GetPlayerInventory());
        List<(int, ResourceType)> priorities = new List<(int, ResourceType)>();

        int plumCount = InventoryUtil.GetCount(_game.GetPlayerInventory(), ResourceType.PLUM);
        priorities.Add((plumCount, ResourceType.PLUM));

        int lemonCount = InventoryUtil.GetCount(_game.GetPlayerInventory(), ResourceType.LEMON);
        priorities.Add((lemonCount, ResourceType.LEMON));

        int appleCount = InventoryUtil.GetCount(_game.GetPlayerInventory(), ResourceType.APPLE);
        priorities.Add((appleCount, ResourceType.APPLE));

        int bananaCount = InventoryUtil.GetCount(_game.GetPlayerInventory(), ResourceType.BANANA);
        priorities.Add((bananaCount, ResourceType.BANANA));

        // Don't prioritise iron if we have 4 trolls. We'll still add it, just as a much lower priority later
        if (_game.GetPlayerInventory().Iron < 10 && _game.GetPlayerTrollCount() < 4)
        {
            int ironCount = InventoryUtil.GetCount(_game.GetPlayerInventory(), ResourceType.IRON);
            priorities.Add((ironCount, ResourceType.IRON));
        }

        if (!InventoryUtil.AllFruitAbove(_game.GetPlayerInventory(), 9) && _game.GetPlayerTrollCount() < 4)
        {
            priorities.Sort((a, b) => a.Item1.CompareTo(b.Item1));
        }

        foreach ((int count, ResourceType type) in priorities)
        {
            if (type == ResourceType.PLUM)
            {
                _priorities.Add(Need.HarvestPlum);
            }
            else if (type == ResourceType.LEMON)
            {
                _priorities.Add(Need.HarvestLemon);
            }
            else if (type == ResourceType.APPLE)
            {
                _priorities.Add(Need.HarvestApple);
            }
            else if (type == ResourceType.BANANA)
            {
                _priorities.Add(Need.HarvestBanana);
            }
            else if (type == ResourceType.IRON)
            {
                _priorities.Add(Need.HarvestIron);
            }
        }
    }

    private void CheckAndAddGrowPriorities()
    {
        int neededDist = 3;

        List<(int, ResourceType)> priorities = new List<(int, ResourceType)>();

        // Number of trees within 3 of shack
        int plumTreeCount = TreeCountWithinDistOfShack(ResourceType.PLUM, neededDist);
        int lemonTreeCount = TreeCountWithinDistOfShack(ResourceType.LEMON, neededDist);
        int appleTreeCount = TreeCountWithinDistOfShack(ResourceType.APPLE, neededDist);
        int bananaTreeCount = TreeCountWithinDistOfShack(ResourceType.BANANA, neededDist);

        // Number of fruit in inventory
        int plumCount = InventoryUtil.GetCount(_game.GetPlayerInventory(), ResourceType.PLUM);
        int lemonCount = InventoryUtil.GetCount(_game.GetPlayerInventory(), ResourceType.LEMON);
        int appleCount = InventoryUtil.GetCount(_game.GetPlayerInventory(), ResourceType.APPLE);
        int bananaCount = InventoryUtil.GetCount(_game.GetPlayerInventory(), ResourceType.BANANA);

        int plumPriority = CalculatePriority(plumCount, plumTreeCount);
        int lemonPriority = CalculatePriority(lemonCount, lemonTreeCount);
        int applePriority = CalculatePriority(appleCount, appleTreeCount);
        int bananaPriority = CalculatePriority(bananaCount, bananaTreeCount);

        Logger.Message($"Grow priorities - Plum: {plumPriority} (Count: {plumCount}, Trees: {plumTreeCount}), Lemon: {lemonPriority} (Count: {lemonCount}, Trees: {lemonTreeCount}), Apple: {applePriority} (Count: {appleCount}, Trees: {appleTreeCount}), Banana: {bananaPriority} (Count: {bananaCount}, Trees: {bananaTreeCount})");
        if (plumPriority > 0)
        {
            priorities.Add((plumPriority, ResourceType.PLUM));
        }

        if(lemonPriority > 0)
        { 
            priorities.Add((lemonPriority, ResourceType.LEMON));
        }

        if(applePriority > 0)
        {
            priorities.Add((applePriority, ResourceType.APPLE));
        }

        if(bananaPriority > 0)
        {
            priorities.Add((bananaPriority, ResourceType.BANANA));
        }             

        priorities.Sort((a, b) => b.Item1.CompareTo(a.Item1));

        foreach ((int count, ResourceType type) in priorities)
        {
            if (type == ResourceType.PLUM)
            {
                _priorities.Add(Need.GrowPlum);
            }
            else if (type == ResourceType.LEMON)
            {
                _priorities.Add(Need.GrowLemon);
            }
            else if (type == ResourceType.APPLE)
            {
                _priorities.Add(Need.GrowApple);
            }
            else if (type == ResourceType.BANANA)
            {
                _priorities.Add(Need.GrowBanana);
            }
        }
    }

    private int CalculatePriority(int inventoryCount, int treeCount)
    {
        int priority = -1;
        
        if (treeCount == 0)
        {
            priority += 100;
        }
        else if (treeCount == 1)
        {
            priority += 50;
        }
        

        // We want ones with a smaller inventory to be prioritised
        if (priority >= 0)
        {
            priority += (40 - inventoryCount);
        }

        return priority;
    }

    private int TreeCountWithinDistOfShack(int neededDist)
    {
        int count = 0;

        foreach (Tree tree in _game.GetTrees().OrderBy(t => _positionUtil.CalculateManhattanDistance(_game.GetPlayerShackPosition(), t.Position)).ToList())
        {
            if (_positionUtil.CalculateManhattanDistance(_game.GetPlayerShackPosition(), tree.Position) > neededDist)
            {
                continue;
            }

            if (_positionUtil.GetShortestPath(_game.GetPlayerShackPosition(), tree.Position).Count <= neededDist)
            {
                count++;
            }
        }

        return count;
    }

    private int TreeCountWithinDistOfShack(ResourceType fruitType, int neededDist)
    {
        List<Tree> eligibleTrees = _game.GetTrees(fruitType);

        eligibleTrees = eligibleTrees.OrderBy(t => _positionUtil.CalculateManhattanDistance(_game.GetPlayerShackPosition(), t.Position)).ToList();

        int count = 0;
         
        foreach (Tree tree in eligibleTrees)
        {
            if (_positionUtil.CalculateManhattanDistance(_game.GetPlayerShackPosition(), tree.Position) > neededDist)
            {
                continue;
            }
                
            if(_positionUtil.GetShortestPath(_game.GetPlayerShackPosition(), tree.Position).Count <= neededDist)
            {
                count++;
            }
        }

        return count;
    }

    internal List<Need> GetPriorities()
    {
        return _priorities;
    }

    internal static bool IsGrowNeed(Need need)
    {
        return need == Need.GrowPlum || need == Need.GrowLemon || need == Need.GrowApple || need == Need.GrowBanana;
    }

    internal static ResourceType GetTreeType(Need need)
    {
        switch (need)
        {
            case Need.GrowPlum:
            case Need.HarvestPlum:
                return ResourceType.PLUM;
            case Need.GrowLemon:
            case Need.HarvestLemon:
                return ResourceType.LEMON;
            case Need.GrowApple:
            case Need.HarvestApple:
                return ResourceType.APPLE;
            case Need.GrowBanana:
            case Need.HarvestBanana:
                return ResourceType.BANANA;
            case Need.HarvestIron:
                return ResourceType.IRON;
            case Need.HarvestEnemyWood:
            case Need.HarvestAnyWood:
                return ResourceType.WOOD;
            default:
                return ResourceType.UNKNOWN;
        }
    }

    internal static bool IsharvestNeed(Need need)
    {
        return need == Need.HarvestPlum || need == Need.HarvestLemon || need == Need.HarvestApple || need == Need.HarvestBanana || need == Need.HarvestIron || need == Need.HarvestEnemyWood || need == Need.HarvestAnyWood;
    }
}
