using System.Drawing;

namespace WinterChallenge2024;

internal struct Organ
{
    internal int Id { get; private set; }

    internal Point Position { get; private set; }

    public Organ(int id, Point position) : this()
    {
        Id = id;
        Position = position;
    }
}