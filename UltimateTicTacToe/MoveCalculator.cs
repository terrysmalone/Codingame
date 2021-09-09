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
            var moves = GetMoveScoresUsingAlphaBeta(ticTacToeBoard, depth, startingPlayer).OrderByDescending(m => m.Item2).ToList();
            
            var max = moves.Max(m => m.Item2);
            
            var highest = moves.Where(m => m.Item2 == max).ToList();
            
            var rand = new Random();
            
            return highest[rand.Next(highest.Count)].Item1;
        
            //return GetMoveScoresUsingAlphaBeta(ticTacToeBoard, depth, startingPlayer).OrderByDescending((m => m.Item2)).First().Item1;
        }

        internal List<Tuple<Move, int>> GetMoveScoresUsingAlphaBeta(ITicTacToe ticTacToeBoard, int depth, char player)
        {
            _board = ticTacToeBoard;

            var moveScores = new List<Tuple<Move, int>>();

            var validMoves = _board.CalculateValidMoves();

            foreach (var validAction in validMoves)
            {
                var isX = player == 'X';

                _board.AddMove(validAction.Column, validAction.Row, player);

                var score = -Calculate(int.MinValue+1, int.MaxValue, depth-1, !isX, SwapPieces(player));

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

            var validMoves = _board.CalculateValidMoves();

            if(validMoves.Count == 0
               || _board.IsGameOver())
            {
                return _board.Evaluate(isX, depth);
            }

            var score = int.MinValue;

            foreach (var move in validMoves)
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

            var moveScores = new List<Tuple<Move, int>>();

            var validMoves = _board.CalculateValidMoves();

            foreach (var validAction in validMoves)
            {
                var maximisingPlayer = player == 'X';

                _board.AddMove(validAction.Column, validAction.Row, player);

                var score = -Calculate(depth-1, !maximisingPlayer, SwapPieces(player));

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

            var validMoves = _board.CalculateValidMoves();

            if(validMoves.Count == 0)
            {
                return _board.Evaluate(maximisingPlayer, depth);
            }

            var evaluation = _board.Evaluate(maximisingPlayer, depth);

            if(evaluation != 0)
            {
                return evaluation;
            }

            var maxScore = int.MinValue;

            foreach (var move in validMoves)
            {
                _board.AddMove(move.Column, move.Row, piece);

                var score = -Calculate(depth-1, !maximisingPlayer, SwapPieces(piece));

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

            foreach (var move in moves)
            {
                Console.Error.WriteLine($"Move:{move.Column}, {move.Row}");
            }
        }

        private void PrintMovesList(List<Tuple<Move, int>> moveScores)
        {
            Console.Error.WriteLine("======================");

            foreach (var moveScore in moveScores)
            {
                Console.Error.WriteLine($"Move:{moveScore.Item1.Column}, {moveScore.Item1.Row} - {moveScore.Item2}");
            }
        }
    }
}