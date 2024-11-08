using System;
using System.Collections.Generic;
using System.Linq;

namespace UltimateTicTacToe
{
    internal sealed class MoveCalculator
    {
        private ITicTacToe _board;

        internal Move GetBestMoveUsingAlphaBeta(ITicTacToe ticTacToeBoard, int depth, char startingPlayer)
        {
            List<Tuple<Move, int>> moves = GetMoveScoresUsingAlphaBeta(ticTacToeBoard, depth, startingPlayer).OrderByDescending(m => m.Item2).ToList();

            int max = moves.Max(m => m.Item2);

            List<Tuple<Move, int>> highest = moves.Where(m => m.Item2 == max).ToList();

            Random rand = new Random();
            
            return highest[rand.Next(highest.Count)].Item1;
        
            //return GetMoveScoresUsingAlphaBeta(ticTacToeBoard, depth, startingPlayer).OrderByDescending((m => m.Item2)).First().Item1;
        }

        internal List<Tuple<Move, int>> GetMoveScoresUsingAlphaBeta(ITicTacToe ticTacToeBoard, int depth, char player)
        {
            _board = ticTacToeBoard;

            List<Tuple<Move, int>> moveScores = new List<Tuple<Move, int>>();

            List<Move> validMoves = _board.CalculateValidMoves();

            foreach (Move validAction in validMoves)
            {
                bool isX = player == 'X';

                _board.AddMove(validAction.Column, validAction.Row, player);

                int score = -Calculate(int.MinValue+1, int.MaxValue, depth-1, !isX, SwapPieces(player));

                moveScores.Add(new Tuple<Move, int>(new Move(validAction.Column, validAction.Row), score));

                _board.UndoMove(validAction.Column, validAction.Row);
            }

            //PrintMovesList(moveScores);

            return moveScores;
        }

        private int Calculate(int alpha, int beta, int depth, bool isX, char piece)
        {
            if (depth == 0)
            {
                return _board.Evaluate(isX, depth);
            }

            List<Move> validMoves = _board.CalculateValidMoves();

            if(validMoves.Count == 0
               || _board.IsGameOver())
            {
                return _board.Evaluate(isX, depth);
            }

            int score = int.MinValue;

            foreach (Move move in validMoves)
            {
                _board.AddMove(move.Column, move.Row, piece);
                score = Math.Max(score, -Calculate(-beta, -alpha,depth-1, !isX, SwapPieces(piece)));

                _board.UndoMove(move.Column, move.Row);

                alpha = Math.Max(alpha, score);

                if (alpha >= beta)
                {
                    return alpha;
                }
            }

            return score;
        }

        //------------------------------------------------------------------
        internal Move GetBestMove(ITicTacToe ticTacToeBoard, int depth, char startingPlayer)
        {
            return GetMoveScores(ticTacToeBoard, depth, startingPlayer).OrderByDescending((m => m.Item2)).First().Item1;
        }

        private List<Tuple<Move, int>> GetMoveScores(ITicTacToe ticTacToeBoard, int depth, char player)
        {
            _board = ticTacToeBoard;

            List<Tuple<Move, int>> moveScores = new List<Tuple<Move, int>>();

            List<Move> validMoves = _board.CalculateValidMoves();

            foreach (Move validAction in validMoves)
            {
                bool maximisingPlayer = player == 'X';

                _board.AddMove(validAction.Column, validAction.Row, player);

                int score = -Calculate(depth-1, !maximisingPlayer, SwapPieces(player));

                moveScores.Add(new Tuple<Move, int>(new Move(validAction.Column, validAction.Row), score));

                _board.UndoMove(validAction.Column, validAction.Row);
            }

            return moveScores;
        }

        private int Calculate(int depth, bool maximisingPlayer, char piece)
        {
            if (depth == 0)
            {
                return _board.Evaluate(maximisingPlayer, depth);
            }

            List<Move> validMoves = _board.CalculateValidMoves();

            if(validMoves.Count == 0)
            {
                return _board.Evaluate(maximisingPlayer, depth);
            }

            int evaluation = _board.Evaluate(maximisingPlayer, depth);

            if(evaluation != 0)
            {
                return evaluation;
            }

            int maxScore = int.MinValue;

            foreach (Move move in validMoves)
            {
                _board.AddMove(move.Column, move.Row, piece);

                int score = -Calculate(depth-1, !maximisingPlayer, SwapPieces(piece));

                _board.UndoMove(move.Column, move.Row);

                if (score > maxScore)
                {
                    maxScore = score;
                }
            }

            return maxScore;
        }

        private static char SwapPieces(char piece)
        {
            return piece == 'O' ? 'X' : 'O';
        }

        private void PrintMovesList(List<Move> moves)
        {
            Console.Error.WriteLine("======================");

            foreach (Move move in moves)
            {
                Console.Error.WriteLine($"Move:{move.Column}, {move.Row}");
            }
        }

        private void PrintMovesList(List<Tuple<Move, int>> moveScores)
        {
            Console.Error.WriteLine("======================");

            foreach (Tuple<Move, int> moveScore in moveScores)
            {
                Console.Error.WriteLine($"Move:{moveScore.Item1.Column}, {moveScore.Item1.Row} - {moveScore.Item2}");
            }
        }
    }
}