using System.Collections.Generic;

namespace UltimateTicTacToe
{
    internal interface ITicTacToe
    {
        List<Move> CalculateValidMoves();
        int Evaluate(bool isX, int currentDepth);
        void AddMove(int column, int row, char piece);
        void UndoMove(int column, int row);
        bool IsGameOver();
    }
}