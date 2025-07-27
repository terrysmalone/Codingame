namespace Spring2021Challenge
{
    internal sealed class Tree
    {
        public int CellIndex { get; }
        public int Size { get; set; }
        public bool IsMine { get; }
        public bool IsDormant { get; }
        
        public Tree(int cellIndex, int size, bool isMine, bool isDormant)
        {
            CellIndex = cellIndex;
            Size = size;
            IsMine = isMine;
            IsDormant = isDormant;
        }
    }
}