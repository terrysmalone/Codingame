namespace UltimateTicTacToe
{
    internal sealed class Move
    {
        public int Row { get; }
        public int Column { get; }

        public Move(int column, int row)
        {
            Column = column;
            Row = row;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            else
            {
                var m = (Move) obj;
                return (Column == m.Column) && (Row == m.Row);
            }
        }
    }
}