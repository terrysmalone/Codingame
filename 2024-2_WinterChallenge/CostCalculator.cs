using System.Collections.Generic;

namespace WinterChallenge2024;
internal static class CostCalculator
{
    internal static bool CanProduceOrgan(OrganType organ, ProteinStock proteinStock)
    {
        return CanProduceOrgan(organ, proteinStock, 1);
    }

    internal static bool CanProduceOrgan(OrganType organ, ProteinStock proteinStock, int amount)
    {
        switch (organ)
        {
            case OrganType.BASIC:
                if (proteinStock.A >= (1 * amount))
                {
                    return true;
                }
                return false;
            case OrganType.HARVESTER:
                if (proteinStock.C >= (1 * amount) &&
                    proteinStock.D >= (1 * amount))
                {
                    return true;
                }
                return false;
            case OrganType.ROOT:
                if (proteinStock.A >= (1 * amount) &&
                    proteinStock.B >= (1 * amount) &&
                    proteinStock.C >= (1 * amount) &&
                    proteinStock.D >= (1 * amount))
                {
                    return true;
                }
                return false;
            case OrganType.SPORER:
                if (proteinStock.B >= (1 * amount) &&
                    proteinStock.D >= (1 * amount))
                {
                    return true;
                }
                return false;
            case OrganType.TENTACLE:
                if (proteinStock.B >= (1 * amount) &&
                    proteinStock.C >= (1 * amount))
                {
                    return true;
                }
                return false;
        }

        return false;
    }

    internal static bool CanProduceOrgans(List<OrganType> organs, ProteinStock proteinStock)
    {
        int aNeeded = 0;
        int bNeeded = 0;
        int cNeeded = 0;
        int dNeeded = 0;

        foreach (OrganType organType in organs)
        {
            switch (organType)
            {
                case OrganType.BASIC:
                    aNeeded++;
                    break;
                case OrganType.HARVESTER:
                    cNeeded++;
                    dNeeded++;
                    break;
                case OrganType.ROOT:
                    aNeeded++;
                    bNeeded++;
                    cNeeded++;
                    dNeeded++;
                    break;
                case OrganType.SPORER:
                    bNeeded++;
                    dNeeded++;
                    break;
                case OrganType.TENTACLE:
                    bNeeded++;
                    cNeeded++;
                    break;
            }
        }

        if (proteinStock.A < aNeeded)
        {
            return false;
        }

        if (proteinStock.B < bNeeded)
        {
            return false;
        }

        if (proteinStock.C < cNeeded)
        {
            return false;
        }

        if (proteinStock.D < dNeeded)
        {
            return false;
        }

        return true;
    }
}
