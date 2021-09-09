using System;
using System.Collections.Generic;

namespace UltimateTicTacToe
{
    internal sealed class MultiTicTacToe : ITicTacToe
    {
        private Move _previousActiveBoard;
        private Move ActiveBoard { get; set; } = new Move(-1, -1);
        public TicTacToe[,] SubBoards { get; }
        public TicTacToe Board { get;  }

        internal MultiTicTacToe()
        {
            Board = new TicTacToe();

            SubBoards = new TicTacToe[3,3];
            SubBoards[0,0] = new TicTacToe();
            SubBoards[0,1] = new TicTacToe();
            SubBoards[0,2] = new TicTacToe();
            SubBoards[1,0] = new TicTacToe();
            SubBoards[1,1] = new TicTacToe();
            SubBoards[1,2] = new TicTacToe();
            SubBoards[2,0] = new TicTacToe();
            SubBoards[2,1] = new TicTacToe();
            SubBoards[2,2] = new TicTacToe();
        }

        public void AddMove(int column, int row, char piece)
        {
            _previousActiveBoard = ActiveBoard;

            var localColumn = column % 3;
            var localRow = row % 3;

            SubBoards[column / 3, row / 3].AddMove(localColumn, localRow, piece);

            // update active board
            var probablyNextActiveBoard = SubBoards[localColumn, localRow];

            if (!probablyNextActiveBoard.IsGameOver())
            {
                ActiveBoard = new Move(localColumn, localRow);
            }
            else
            {
                ActiveBoard = new Move(-1, -1);
            }

            UpdateOverallBoard();
        }

        public void UndoMove(int column, int row)
        {
            SubBoards[column / 3, row / 3].UndoMove(column % 3, row % 3);

            // Set active board back
            ActiveBoard = _previousActiveBoard;

            UpdateOverallBoard();
        }
        
        public bool IsGameOver()
        {
            if(  Board.EvaluateBoard() != 0
                 || AvailableSpacesOnBoard() == 0)
            {
                return true;
            }

            return false;
        }
        
        private int AvailableSpacesOnBoard()
        {
            var availableSpaces = Board.AvailableSpacesOnBoard();

            foreach (var board in SubBoards)
            {
                availableSpaces += board.AvailableSpacesOnBoard();
            }

            return availableSpaces;
        }

        private void UpdateOverallBoard()
        {
            Board.ClearBoard();
            
            for(var column = 0; column < 3; column++)
            {
                for(var row = 0; row < 3; row++)
                {
                    var evaluation = SubBoards[column, row].EvaluateBoard();

                    if(evaluation > 0)
                    {
                        Board.AddMove(column, row, 'O');
                    }
                    else if(evaluation < 0)
                    {
                        Board.AddMove(column, row, 'X');
                    }
                }
            }
        }

        public List<Move> CalculateValidMoves()
        {
            var moves = new List<Move>();

            if (ActiveBoard.Column == -1)
            {
                for (var column = 0; column < 3; column++)
                {
                    for (var row = 0; row < 3; row++)
                    {
                        var subBoard = SubBoards[column, row];

                        if (!subBoard.IsGameOver())
                        {
                            moves.AddRange(TranslateToGlobalMoves(subBoard.CalculateValidMoves(), new Move(column, row)));
                        }
                    }
                }
            }
            else
            {
                moves.AddRange(TranslateToGlobalMoves(SubBoards[ActiveBoard.Column, ActiveBoard.Row].CalculateValidMoves(), ActiveBoard));
            }

            return moves;
        }

        private static IEnumerable<Move> TranslateToGlobalMoves(List<Move> moves, Move activeBoard)
        {
            var translatedMoves = new List<Move>();

            foreach (var move in moves)
            {
                translatedMoves.Add(TranslateToGlobalMove(move, activeBoard));
            }

            return translatedMoves;
        }

        private static Move TranslateToGlobalMove(Move move, Move activeBoard)
        {
            return new Move(activeBoard.Column * 3 + move.Column, activeBoard.Row * 3 + move.Row);
        }

        public int Evaluate(bool isX, int depth)
        {
            var score = 0;

            foreach (var board in SubBoards)
            {
                score += board.Evaluate(isX, depth);
            }

            score += Board.Evaluate(isX, depth) * 10;

            return score;
        }
        public void PrintBoard()
        {
            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    Console.Error.WriteLine($"Board {i+j}");
                    SubBoards[j, i].PrintBoard();
                }
            }
        }
    }
}