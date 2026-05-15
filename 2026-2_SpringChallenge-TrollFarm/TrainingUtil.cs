using System;

namespace SpringChallenge2026;

// Training a troll costs the number of trolls on your team + the square of the matching attribute.
// Plums = movementSpeed
// Lemons = carryCapacity
// Apples = harvestPower
// Iron = chopPower
internal static class TrainingUtil
{
    internal static (int plums, int lemons, int apples, int iron) GetBestTrollTraining(int numberOfTrolls, Inventory inventory)
    {
        int plums = GetMaxAmount(numberOfTrolls, inventory.Plum);
        int lemons = GetMaxAmount(numberOfTrolls, inventory.Lemon);
        int apples = GetMaxAmount(numberOfTrolls, inventory.Apple);
        int iron = GetMaxAmount(numberOfTrolls, inventory.Iron);

        return (plums, lemons, apples, iron);
    }

    private static int GetMaxAmount(int numberOfTrolls, int fruitCount)
    {
        int max = 0;

        while(true)
        {
            max++;

            if(numberOfTrolls + (max * max) > fruitCount)
            {
                return max - 1;
            }
        }
    }
}