namespace CodeRoyale;
    
using System.Drawing;

internal sealed class Site
{
    internal int Id { get; }
    internal Point Position { get; }
    internal int Radius { get; }
    internal StructureType Structure { get; set; }
    internal int Owner { get; set; }
    public int Gold { get; set; }
    public int MaxMineSize { get; set; }
        
    public int MineSize { get; private set; }
    public int TowerSize { get;  private set; }

    internal Site(int id, Point position, int radius)
    {
        Id = id;
        Position = position;
        Radius = radius;

        Structure = StructureType.Empty;
    }

    public void IncrementMineSize() => MineSize++;
    public void IncrementTowerSize() => TowerSize++;  
}