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
}
