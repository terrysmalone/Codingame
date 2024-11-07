namespace Connect4; 

using System;
using System.Collections.Generic;
using System.Linq;

internal sealed class MoveCalculator
{
    private ConnectFour _connectFour;

    internal int GetBestMoveUsingAlphaBeta(ConnectFour connectFour, int depth, int startingPlayer)
    {
        var moves = GetMoveScoresUsingAlphaBeta(connectFour, depth, startingPlayer).OrderByDescending(m => m.Item2).ToList();
        
        var max = moves.Max(m => m.Item2);
        
        PrintMoveScores(moves);
        
        var highest = moves.Where(m => m.Item2 == max).ToList();

        var rand = new Random();
        
        return highest[rand.Next(highest.Count)].Item1;
    
        //return GetMoveScoresUsingAlphaBeta(ticTacToeBoard, depth, startingPlayer).OrderByDescending((m => m.Item2)).First().Item1;
    }
    private void PrintMoveScores(List<Tuple<int, int>> moves)
    {
        Console.Error.WriteLine($"---------------------------------------");

        foreach (var move in moves)
        {
            Console.Error.WriteLine($"Move:{move.Item1}, score:{move.Item2}");
        }
    }

    internal List<Tuple<int, int>> GetMoveScoresUsingAlphaBeta(ConnectFour connectFour, int depth, int player)
    {
        _connectFour = connectFour;

        var moveScores = new List<Tuple<int, int>>();

        var validMoves = _connectFour.CalculateValidMoves();

        foreach (var validAction in validMoves)
        {
            var is0 = player == 0;
            
            //var board = _connectFour.DisplayBoard();

            _connectFour.AddMove(validAction, player);

            var score = -Calculate(int.MinValue+1, int.MaxValue, depth-1, !is0, SwapPieces(player));

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

        var validMoves = _connectFour.CalculateValidMoves();

        if(validMoves.Count == 0
           || _connectFour.IsGameOver())
        {
            return _connectFour.Evaluate(is0, depth);
        }

        var score = int.MinValue;

        foreach (var move in validMoves)
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