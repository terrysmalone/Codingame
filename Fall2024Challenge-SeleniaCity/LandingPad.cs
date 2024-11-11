namespace Fall2024Challenge_SeleniaCity;
using System.Drawing;

internal sealed class LandingPad(int id, Point position, int[] astronauts) : IBuilding
{
    internal int Id { get; } = id;
    internal Point Position { get; } = position;
    internal int[] Astronauts { get; } = astronauts;

    int IBuilding.GetId()
    {
        return Id;
    }

    Point IBuilding.Getposition()
    {
        return Position;
    }
}