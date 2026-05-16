namespace SpringChallenge2026;

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
        int minimumRequired = numberOfTrolls + 1;

        if (minimumRequired > fruitCount)
        {
            // We can't do it!
            return 0;
        }

        int lastbaseStat = 1;
        int lastValid = numberOfTrolls + lastbaseStat;

        while (true)
        {
            lastbaseStat++;

            int required = numberOfTrolls + (lastbaseStat * lastbaseStat);

            if (required <= fruitCount)
            {
                lastValid = required;
            }
            else
            {
                break;
            }

        }

        if (lastbaseStat > 2)
        {
            return 2;
        }

        return lastbaseStat-1;
    }
}

