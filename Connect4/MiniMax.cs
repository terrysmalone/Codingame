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

        int alpha = int.MinValue;
        int beta = int.MaxValue;

        int bestMove = -1;

        foreach (int move in gameState.GetValidMoves())
        {
            // Make the move, recursively evaluate the game state, then undo the move
            gameState.ApplyMove(move);
            int score = MiniMaxRecursive(gameState, depth - 1, alpha, beta);
            gameState.UndoLastMove();

            Console.Error.WriteLine($"move:{move} - score:{score}");

            // If it's player 1, we want the highest score, if it's player -1, we want the lowest score
            if ((gameState.CurrentPlayer == 1 && score > bestScore)
                || (gameState.CurrentPlayer == -1 && score < bestScore))
            {
                bestScore = score;
                bestMove = move;
            }

            // Update alpha or beta so we can prune at the root
            if (gameState.CurrentPlayer == 1)
            {
                alpha = Math.Max(alpha, bestScore);
            }
            else
            {
                beta = Math.Min(beta, bestScore);
            }
        }

        return bestMove;
    }

    private static int MiniMaxRecursive(GameState gameState, int depth, int alpha, int beta)
    {
        int bestScore = 0;

        // If we've hit the depth limit or the game is over, return the score
        if (depth <= 0 || gameState.IsTerminal())
        {
            int score = gameState.CalculateScore();
            return score + (Math.Sign(score) * depth);
        }

        List<int> validMoves = gameState.GetValidMoves();

        if (gameState.CurrentPlayer == 1)
        {
            bestScore = int.MinValue;

            foreach (int move in validMoves)
            {
                // Make the move, recursively evaluate the game state, then undo the move
                gameState.ApplyMove(move);
                int score = MiniMaxRecursive(gameState, depth - 1, alpha, beta);
                gameState.UndoLastMove();

                // The best score is the maximum of all attempts so far
                bestScore = Math.Max(bestScore, score);

                // If the best score is higher than alpha update alpha
                alpha = Math.Max(alpha, bestScore);

                // If beta is less than or equal to alpha stop evaluating this branch
                if (beta <= alpha)
                {
                    break;
                }
            }
        }
        else
        {
            bestScore = int.MaxValue;

            foreach (int move in validMoves)
            {
                // Make the move, recursively evaluate the game state, then undo the move
                gameState.ApplyMove(move);
                int score = MiniMaxRecursive(gameState, depth - 1, alpha, beta);
                gameState.UndoLastMove();

                // The best score is the minimum of all attempts so far
                bestScore = Math.Min(bestScore, score);

                // If the best score is lower than beta update beta
                beta = Math.Min(beta, bestScore);

                // If beta is less than or equal to alpha stop evaluating this branch
                if (beta <= alpha)
                {
                    break;
                }
            }
        }

        return bestScore;
    }
}
