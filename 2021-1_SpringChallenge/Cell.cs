namespace Spring2021Challenge
{
    internal sealed class Cell
    {
        public int Index { get; }
        public int Richness { get; }
        public int[] Neighbours { get; }

        public Cell(int index, int richness, int[] neighbours)
        {
            Index = index;
            Richness = richness;
            Neighbours = neighbours;
        }
    }
}