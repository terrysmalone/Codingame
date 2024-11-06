namespace CodeVsZombies;
    
using System.Drawing;

internal sealed class Human
{
    public int Id { get; }
    public Point Position { get; }
    
    internal Human(int id, Point position)
    {
        Id = id;
        Position = position;
    }
}