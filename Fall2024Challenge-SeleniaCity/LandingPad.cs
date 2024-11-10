namespace Fall2024Challenge_SeleniaCity;
using System.Drawing;

internal sealed class LandingPad(int id, Point position, int[] astronauts)
{
    internal int Id { get; } = id;
    internal Point Position { get; } = position;
    internal int[] Astronauts { get; } = astronauts;
}
