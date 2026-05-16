using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SpringChallenge2026;

internal sealed class NeedsManager
{
    private const int EARLY_GAME_END = 100;
    private const int MID_GAME_END = 200;

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

        // For now, lets just get one of each tree beside our base
        if (_game.Turn < EARLY_GAME_END)
        {
            if (_game.GetPlayerTrollCount() >= 3)
            {
                _priorities.Add(Need.AttackEnemy);
            }
            if (_game.GetPlayerTrollCount() >= 6)
            {
                _priorities.Add(Need.AttackEnemy);
            }

            CheckAndAddGrowPriorities();
            CheckAndAddHarvestPriorities();

            _priorities.Add(Need.TrainTroll);
        }
        else if (_game.Turn < MID_GAME_END)
        {
            if (_game.GetPlayerTrollCount() >= 3)
            {
                _priorities.Add(Need.AttackEnemy);
            }
            if (_game.GetPlayerTrollCount() >= 6)
            {
                _priorities.Add(Need.AttackEnemy);
            }

            CheckAndAddGrowPriorities();
            CheckAndAddHarvestPriorities();
            _priorities.Add(Need.TrainTroll);
        }
        else
        {
            // If all 4 adjacent squares are blocked attack the enemy
            _priorities.Add(Need.HarvestAnyWood);
            _priorities.Add(Need.HarvestAnyWood);
            _priorities.Add(Need.HarvestAnyWood);
            _priorities.Add(Need.HarvestAnyWood);
            _priorities.Add(Need.AttackEnemy);
            _priorities.Add(Need.AttackEnemy);
            _priorities.Add(Need.AttackEnemy);
            _priorities.Add(Need.AttackEnemy);
            _priorities.Add(Need.AttackEnemy);
            _priorities.Add(Need.AttackEnemy);
            _priorities.Add(Need.AttackEnemy);
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

        int ironCount = InventoryUtil.GetCount(_game.GetPlayerInventory(), ResourceType.IRON);
        priorities.Add((ironCount, ResourceType.IRON));

        priorities.Sort((a, b) => a.Item1.CompareTo(b.Item1));

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
        List<(int, ResourceType)> priorities = new List<(int, ResourceType)>();

        if (TreeCount(ResourceType.PLUM))
        {
            int plumCount = InventoryUtil.GetCount(_game.GetPlayerInventory(), ResourceType.PLUM);
            priorities.Add((plumCount, ResourceType.PLUM));
        }

        if (TreeCount(ResourceType.LEMON))
        {
            int lemonCount = InventoryUtil.GetCount(_game.GetPlayerInventory(), ResourceType.LEMON);
            priorities.Add((lemonCount, ResourceType.LEMON));
        }

        if (TreeCount(ResourceType.APPLE))
        {
            int appleCount = InventoryUtil.GetCount(_game.GetPlayerInventory(), ResourceType.APPLE);
            priorities.Add((appleCount, ResourceType.APPLE));
        }

        if (TreeCount(ResourceType.BANANA))
        {
            int bananaCount = InventoryUtil.GetCount(_game.GetPlayerInventory(), ResourceType.BANANA);
            priorities.Add((bananaCount, ResourceType.BANANA));
        }       

        priorities.Sort((a, b) => a.Item1.CompareTo(b.Item1));

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

    private bool TreeCount(ResourceType fruitType)
    {
        int neededDist = 3;
        int closest = _positionUtil.GetClosestTreeToShack(fruitType);

        if (closest <= neededDist)
        {
            return false;
        }

        return true;
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
