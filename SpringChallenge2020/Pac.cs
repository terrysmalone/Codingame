
using System.Drawing;

namespace SpringChallenge2020;

internal struct Pac
{
    internal int Id;
    internal Point Position;

    public Pac(int id, Point position)
    {
        Id = id;
        Position = position;
    }
}
