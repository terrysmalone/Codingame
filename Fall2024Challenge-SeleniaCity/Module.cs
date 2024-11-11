namespace Fall2024Challenge_SeleniaCity;
using System.Drawing;

internal sealed class Module(int id, int type, Point position): IBuilding
{
    internal int Id { get; } = id;
    public int Type { get; } = type;
    internal Point Position { get; } = position;

    int IBuilding.GetId()
    {
        return Id;
    }

    Point IBuilding.Getposition()
    {
        return Position;
    }
}
