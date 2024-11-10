
namespace Fall2024Challenge_SeleniaCity;

internal sealed class Tube(int building1Id, int building2Id, int capacity)
{
    public int Building1Id { get; private set; } = building1Id;
    public int Building2Id { get; private set; } = building2Id;
    public int Capacity { get; private set; } = capacity;
}

