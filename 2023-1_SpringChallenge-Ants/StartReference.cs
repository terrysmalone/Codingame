namespace _2023_1_SpringChallenge_Ants;

internal struct StartReference
{
    public int CellId;
    public int PathId;
    public int ParentPathId;

    public StartReference(int cellId, int pathId, int parentPathId)
    {
        CellId = cellId;
        ParentPathId = parentPathId;
        PathId = pathId;
    }
}

