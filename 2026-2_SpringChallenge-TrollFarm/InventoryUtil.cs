using System;
using System.Collections.Concurrent;

namespace SpringChallenge2026;

internal static class InventoryUtil
{
    internal static bool DoesContainFruit(Inventory playerInventory)
    {
        return playerInventory.Plum > 0 || playerInventory.Lemon > 0 || playerInventory.Apple > 0 || playerInventory.Banana > 0;
    }

    internal static bool DoesContainFruit(Inventory inventory, ResourceType fruitType)
    {
        if (fruitType == ResourceType.PLUM)
        {
            return inventory.Plum > 0;
        }
        else if (fruitType == ResourceType.LEMON)
        {
            return inventory.Lemon > 0;
        }
        else if (fruitType == ResourceType.APPLE)
        {
            return inventory.Apple > 0;
        }
        else if (fruitType == ResourceType.BANANA)
        {
            return inventory.Banana > 0;
        }
        else if (fruitType == ResourceType.IRON)
        {
            return inventory.Iron > 0;
        }
        else if (fruitType == ResourceType.WOOD)
        {
            return inventory.Wood > 0;
        }
        else
        {
            throw new Exception("Invalid fruit type");
        }
    }

    internal static int GetCount(Inventory inventory, ResourceType fruitType)
    {
        if (fruitType == ResourceType.PLUM)
        {
            return inventory.Plum;
        }
        else if (fruitType == ResourceType.LEMON)
        {
            return inventory.Lemon;
        }
        else if (fruitType == ResourceType.APPLE)
        {
            return inventory.Apple;
        }
        else if (fruitType == ResourceType.BANANA)
        {
            return inventory.Banana;
        }
        else if (fruitType == ResourceType.IRON)
        {
            return inventory.Iron;
        }
        else if (fruitType == ResourceType.WOOD)
        {
            return inventory.Wood;
        }
        else
        {
            throw new Exception("Invalid fruit type");
        }
    }

    internal static Inventory ChangeInventory(Inventory inventory, ResourceType fruitType, int v)
    {
        if (fruitType == ResourceType.PLUM)
        {
            return new Inventory{Plum = inventory.Plum - v, Lemon = inventory.Lemon, Apple = inventory.Apple, Banana = inventory.Banana, Iron = inventory.Iron, Wood = inventory.Wood};
        }
        else if (fruitType == ResourceType.LEMON)
        {
            return new Inventory{Plum = inventory.Plum, Lemon = inventory.Lemon - v, Apple = inventory.Apple, Banana = inventory.Banana, Iron = inventory.Iron, Wood = inventory.Wood};
        }
        else if (fruitType == ResourceType.APPLE)
        {
            return new Inventory{Plum = inventory.Plum, Lemon = inventory.Lemon, Apple = inventory.Apple - v, Banana = inventory.Banana, Iron = inventory.Iron, Wood = inventory.Wood};
        }
        else if (fruitType == ResourceType.BANANA)
        {
            return new Inventory{Plum = inventory.Plum, Lemon = inventory.Lemon, Apple = inventory.Apple, Banana = inventory.Banana - v, Iron = inventory.Iron, Wood = inventory.Wood};
        }
        else if (fruitType == ResourceType.IRON)
        {
            return new Inventory{Plum = inventory.Plum, Lemon = inventory.Lemon, Apple = inventory.Apple, Banana = inventory.Banana, Iron = inventory.Iron - v, Wood = inventory.Wood};
        }
        else if (fruitType == ResourceType.WOOD)
        {
            return new Inventory{Plum = inventory.Plum, Lemon = inventory.Lemon, Apple = inventory.Apple, Banana = inventory.Banana, Iron = inventory.Iron, Wood = inventory.Wood - v};
        }
        else
        {
            throw new Exception("Invalid fruit type");
        }
    }

    internal static ResourceType GetAnyFruitType(Inventory playerInventory)
    {
        // TODO: Order by most plentiful
        if (playerInventory.Plum > 1)
        {
            return ResourceType.PLUM;
        }
        else if (playerInventory.Lemon > 1)
        {
            return ResourceType.LEMON;
        }
        else if (playerInventory.Apple > 1)
        {
            return ResourceType.APPLE;
        }
        else if (playerInventory.Banana > 1)
        {
            return ResourceType.BANANA;
        }

        return ResourceType.PLUM;
    }
}