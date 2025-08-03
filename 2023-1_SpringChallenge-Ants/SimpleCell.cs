namespace _2023_1_SpringChallenge_Ants;

internal struct SimpleCell
{
    public int Id { get; set; }
    public CellType CellType { get; set; }
   internal int Resources { get; set; }

    public SimpleCell(int id, CellType cellType, int resources)
    {
        Id = id;
        CellType = cellType;
        Resources = resources;
    }
}