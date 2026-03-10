namespace _2026_1_WinterChallenge_SnakeByte;

internal record MoveSet
{
    internal List<Move> Moves { get; init; }
    internal MoveSet(List<Move> moves)
    {
        Moves = moves;
    }
}