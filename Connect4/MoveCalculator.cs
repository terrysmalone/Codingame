namespace Connect4; 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

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

            // Ply should be one because we made one move at the root
            int score = -Calculate(int.MinValue+1, int.MaxValue, depth-1, ply: 1, GetSwappedPlayer(player));

            moveScores.Add(new Tuple<int, int>(validAction, score));

            _connectFour.UndoMove(validAction);
        }

        return moveScores;
    }

    private int Calculate(int alpha, int beta, int depth, int ply, int player)
    {
        if (depth == 0)
        {                                      
            return _connectFour.Evaluate(player, ply);
        }

        List<int> validMoves = _connectFour.CalculateValidMoves();

        if(validMoves.Count == 0
           || _connectFour.IsGameOver())
        {
            return _connectFour.Evaluate(player, ply);
        }

        int score = int.MinValue;

        foreach (int move in validMoves)
        {
            //var board = _connectFour.DisplayBoard();
            _connectFour.AddMove(move, player);
            
            score = Math.Max(score, -Calculate(-beta, -alpha, depth-1, ply+1, GetSwappedPlayer(player)));

            _connectFour.UndoMove(move);

            alpha = Math.Max(alpha, score);

            if (alpha >= beta)
            {
                return alpha;
            }
        }

        return score;
    }
    
    private static int GetSwappedPlayer(int player)
    {
        return player == 0 ? 1 : 0;
    }
}