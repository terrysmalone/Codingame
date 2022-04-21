using System.Drawing;

namespace SpringChallenge2022;

internal sealed class Hero
{
    public int Id { get; }
    public Point Position { get; set; }

    public Hero(int id, Point position)
    {
        Id = id;
        Position = position;
    }
}
