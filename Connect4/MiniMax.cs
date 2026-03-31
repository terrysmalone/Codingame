using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Connect4;

internal class MiniMax
{
    public int FindBestMove(GameState gameState, int depth)
    {
        // If it's player 1s turn we want to maximise the score,
        // if it's player -1s turn we want to minimise the score
        int bestScore = int.MinValue;
        if (gameState.CurrentPlayer == -1)
        {
            bestScore = int.MaxValue;
        }

        int bestMove = -1;

        foreach (int move in gameState.GetValidMoves())
        {
            // Make the move, recursively evaluate the game state, then undo the move
            gameState.ApplyMove(move);
            int score = MiniMaxRecursive(gameState, depth - 1);
            gameState.UndoLastMove();

            Console.Error.WriteLine($"move:{move} - score:{score}");

            // If it's player 1, we want the highest score, if it's player -1, we want the lowest score
            if ((gameState.CurrentPlayer == 1 && score > bestScore)
                || (gameState.CurrentPlayer == -1 && score < bestScore))
            {
                bestScore = score;
                bestMove = move;
            }
        }

        return bestMove;
    }

    private static int MiniMaxRecursive(GameState gameState, int depth)
    {
        // If we've hit the depth limit or the game is over, return the score
        if (depth <= 0 || gameState.IsTerminal())
        {
            int score = gameState.CalculateScore();
            return score + (Math.Sign(score) * depth);
        }

        int bestScore = int.MinValue;
        if (gameState.CurrentPlayer == -1)
        {
            bestScore = int.MaxValue;
        }

        List<int> validMoves = gameState.GetValidMoves();

        foreach (int move in validMoves)
        {
            // Make the move, recursively evaluate the game state, then undo the move
            gameState.ApplyMove(move);
            int score = MiniMaxRecursive(gameState, depth - 1);
            gameState.UndoLastMove();

            // If it's player 1, we want the highest score, if it's player -1, we want the lowest score
            if ((gameState.CurrentPlayer == 1 && score > bestScore)
                || (gameState.CurrentPlayer == -1 && score < bestScore))
            {
                bestScore = score;
            }
        }

        return bestScore;
    }
}
