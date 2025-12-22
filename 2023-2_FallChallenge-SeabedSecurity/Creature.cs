using System.Drawing;

namespace _2023_2_FallChallenge_SeabedSecurity;

internal class Creature
{
    internal int Id { get; private set; }
    internal int Color { get; private set; }
    internal int Type { get; private set; }

    internal Point Position { get; set; }
    internal Point Velocity { get; set; }


    internal Creature(int id, int color, int type)
    {
        Id = id;
        Color = color;
        Type = type;
    }
}
