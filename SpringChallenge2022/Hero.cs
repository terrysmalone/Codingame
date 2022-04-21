using System.Drawing;

namespace SpringChallenge2022;

internal sealed class Hero
{
    public int Id { get; }
    public Point Postion { get; set; }

    public Hero(int id, Point postion)
    {
        Id = id;
        Postion = postion;
    }
}
