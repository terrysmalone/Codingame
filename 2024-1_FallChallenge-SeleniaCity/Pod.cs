namespace Fall2024Challenge_SeleniaCity;

internal sealed class Pod(int id, int numberOfStops, int[] path)
{
    internal int Id { get; private set; } = id;
    internal int NumberOfStops { get; private set; } = numberOfStops;
    internal int[] Path { get; private set; } = path;
}