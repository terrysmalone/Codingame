using System;
using System.Drawing;

namespace SpringChallenge2026;

internal class Troll
{
    internal int Id { get; private set; }
    internal Point Position { get; private set; }
    internal int MovementSpeed { get; private set; }
    internal int CarryCapacity { get; private set; }
    internal int HarvestPower { get; private set; }

    internal int ChopPower { get; set; }
    internal int CarryPlum { get; set; }
    internal int CarryLemon { get; set; }
    internal int CarryApple { get; set; }
    internal int CarryBanana { get; set; }
    internal int CarryIron { get; set; }
    internal int CarryWood { get; set; }

    public Troll(int id, Point position, int movementSpeed, int carryCapacity, int harvestPower, int chopPower)
    {
        Id = id;
        Position = position;
        MovementSpeed = movementSpeed;
        CarryCapacity = carryCapacity;
        HarvestPower = harvestPower;
        ChopPower = chopPower;
    }

    internal int GetTotalCarrying()
    {
        return CarryPlum + CarryLemon + CarryApple + CarryBanana + CarryIron + CarryWood;
    }

    internal bool IsCarryingAnyFruit()
    {
        return CarryPlum > 0 || CarryLemon > 0 || CarryApple > 0 || CarryBanana > 0;
    }


    internal int IsCarryingFruit(ResourceType fruitType)
    {
        if (fruitType == ResourceType.PLUM)
        {
            return CarryPlum;
        }
        else if (fruitType == ResourceType.LEMON)
        {
            return CarryLemon;
        }
        else if (fruitType == ResourceType.APPLE)
        {
            return CarryApple;
        }
        else if (fruitType == ResourceType.BANANA)
        {
            return CarryBanana;
        }
        else if (fruitType == ResourceType.IRON)
        {
            return CarryIron;
        }
        else if (fruitType == ResourceType.WOOD)
        {
            return CarryWood;
        }
        else
        {
            return 0;
        }
    }

    internal bool CanCarry()
    {
        return GetTotalCarrying() < CarryCapacity;
    }

    internal ResourceType CarryingFruitType()
    {
        if (CarryPlum > 0)
        {
            return ResourceType.PLUM;
        }
        else if (CarryLemon > 0)
        {
            return ResourceType.LEMON;
        }
        else if (CarryApple > 0)
        {
            return ResourceType.APPLE;
        }
        else if (CarryBanana > 0)
        {
            return ResourceType.BANANA;
        }

        return ResourceType.PLUM;
    }

    internal bool IsCarryingWood()
    {
        return CarryWood > 0;
    }
}
    