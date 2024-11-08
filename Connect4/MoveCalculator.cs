namespace Connect4; 

using System;
using System.Collections.Generic;
using System.Linq;

internal sealed class MoveCalculator
{
    private ConnectFour _connectFour;

    internal int GetBestMoveUsingAlphaBeta(ConnectFour connectFour, int depth, int startingPlayer)
    {
        List<Tuple<int, int>> moves = GetMoveScoresUsingAlphaBeta(connectFour, depth, startingPlayer).OrderByDescending(m => m.Item2).ToList();

        int max = moves.Max(m => m.Item2);
        
        PrintMoveScores(moves);

        List<Tuple<int, int>> highest = moves.Where(m => m.Item2 == max).ToList();

        Random rand = new Random();
        
        return highest[rand.Next(highest.Count)].Item1;
    
        //return GetMoveScoresUsingAlphaBeta(ticTacToeBoard, depth, startingPlayer).OrderByDescending((m => m.Item2)).First().Item1;
    }
    private void PrintMoveScores(List<Tuple<int, int>> moves)
    {
        Console.Error.WriteLine($"---------------------------------------");

        foreach (Tuple<int, int> move in moves)
        {
            Console.Error.WriteLine($"Move:{move.Item1}, score:{move.Item2}");
        }
    }

    internal List<Tuple<int, int>> GetMoveScoresUsingAlphaBeta(ConnectFour connectFour, int depth, int player)
    {
        _connectFour = connectFour;

        List<Tuple<int, int>> moveScores = new List<Tuple<int, int>>();

        List<int> validMoves = _connectFour.CalculateValidMoves();

        foreach (int validAction in validMoves)
        {
            bool is0 = player == 0;
            
            //var board = _connectFour.DisplayBoard();

            _connectFour.AddMove(validAction, player);

            int score = -Calculate(int.MinValue+1, int.MaxValue, depth-1, !is0, SwapPieces(player));

            moveScores.Add(new Tuple<int, int>(validAction, score));

            _connectFour.UndoMove(validAction);
        }

        return moveScores;
    }

    private int Calculate(int alpha, int beta, int depth, bool is0, int piece)
    {
        if (depth == 0)
        {                                      
            return _connectFour.Evaluate(is0, depth);
        }

        List<int> validMoves = _connectFour.CalculateValidMoves();

        if(validMoves.Count == 0
           || _connectFour.IsGameOver())
        {
            return _connectFour.Evaluate(is0, depth);
        }

        int score = int.MinValue;

        foreach (int move in validMoves)
        {
            //var board = _connectFour.DisplayBoard();
            _connectFour.AddMove(move, piece);
            
            score = Math.Max(score, -Calculate(-beta, -alpha,depth-1, !is0, SwapPieces(piece)));

            _connectFour.UndoMove(move);

            alpha = Math.Max(alpha, score);

            if (alpha >= beta)
            {
                return alpha;
            }
        }

        return score;
    }
    
    private static int SwapPieces(int piece)
    {
        return piece == 0 ? 1 : 0;
    }
}