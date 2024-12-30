using System.Drawing;

namespace WinterChallenge2024;

internal class Protein
{
    internal ProteinType Type { get; private set; }
    internal Point Position { get; private set; }

    internal bool IsHarvested { get; set; }

    internal Protein(ProteinType type, Point position)
    {
        Type = type;
        Position = position;
    }

    internal Protein Clone()
    {
        return new Protein(Type, Position)
        {
            IsHarvested = IsHarvested
        };
    }
}
