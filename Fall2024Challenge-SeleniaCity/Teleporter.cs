
namespace Fall2024Challenge_SeleniaCity;

internal sealed class Teleporter(int building1Id, int building2Id)
{
    public int Building1Id { get; private set; } = building1Id;
    public int Building2Id { get; private set; } = building2Id;
}