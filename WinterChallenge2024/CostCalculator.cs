namespace WinterChallenge2024;
internal static class CostCalculator
{
    internal static bool CanProduceOrgan(OrganType organ, ProteinStock proteinStock)
    {
        switch (organ)
        {
            case OrganType.BASIC:
                if (proteinStock.A >= 1)
                {
                    return true;
                }
                return false;
            case OrganType.HARVESTER:
                if (proteinStock.C >= 1 &&
                    proteinStock.D >= 1)
                {
                    return true;
                }
                return false;
            case OrganType.ROOT:
                if (proteinStock.A >= 1 &&
                    proteinStock.B >= 1 &&
                    proteinStock.C >= 1 &&
                    proteinStock.D >= 1)
                {
                    return true;
                }
                return false;
            case OrganType.SPORER:
                if (proteinStock.B >= 1 &&
                    proteinStock.D >= 1)
                {
                    return true;
                }
                return false;
            case OrganType.TENTACLE:
                if (proteinStock.B >= 1 &&
                    proteinStock.C >= 1)
                {
                    return true;
                }
                return false;
        }

        return false;
    }
}
