using System;
using System.Collections.Generic;

namespace UltimateTicTacToe
{
    internal sealed class TicTacToe : ITicTacToe
    {
        private char[,] _board = new char[3,3];

        public List<Move> CalculateValidMoves()
        {
            var moves = new List<Move>();

            for(var column = 0; column < _board.GetLength(0); column++)
            {
                for(var row = 0; row < _board.GetLength(1); row++)
                {
                    if(_board[column, row] == '\0')
                    {
                        moves.Add(new Move(column, row));
                    }
                }
            }

            return moves;
        }

        public int Evaluate(bool isX, int currentDepth)
        {
            int score;

            if(isX)
            {
                score = -EvaluateBoard(currentDepth);
            }
            else
            {
                score = EvaluateBoard(currentDepth);
            }

            return score;
        }

        private List<Move[]> _lines = new List<Move[]>
        {
            new[] { new Move(0,0), new Move(0,1), new Move(0,2) }, // Left column
            new[] { new Move(1,0), new Move(1,1), new Move(1,2) }, // Middle column
            new[] { new Move(2,0), new Move(2,1), new Move(2,2) }, // Right column

            new[] { new Move(0,0), new Move(1,0), new Move(2,0) }, // Top row
            new[] { new Move(0,1), new Move(1,1), new Move(2,1) }, // middle row
            new[] { new Move(0,2), new Move(1,2), new Move(2,2) }, // Bottom row

            new[] { new Move(0,0), new Move(1,1), new Move(2,2) }, // top left to bottom right diagonal
            new[] { new Move(2,0), new Move(1,1), new Move(0,2) }  // bottom left to top right diagonal
        };

        internal int EvaluateBoard(int currentDepth = 0)
        {
            foreach (var line in _lines)
            {
                var playerWithLine = PlayerWithLine(line);

                if(playerWithLine == 'O')
                {
                    return 1 + currentDepth;
                }

                if(playerWithLine == 'X')
                {
                    return -1 - currentDepth;
                }
            }

            return 0;
        }
        
        private char PlayerWithLine(Move[] line)
        {
            if (DoesPlayerHaveLine(line, 'X'))
            {
                return 'X';
            }
            else if (DoesPlayerHaveLine(line, 'O'))
            {
                return 'O';
            }

            return '\0';
        }
        private bool DoesPlayerHaveLine(Move[] line, char player)
        {
            if(   _board[line[0].Column, line[0].Row] == player
                  && _board[line[1].Column, line[1].Row] == player
                  && _board[line[2].Column, line[2].Row] == player)
            {
                return true;
            }

            return false;
        }

        public void AddMove(int column, int row, char piece)
        {
            _board[column, row] = piece;
        }

        public void UndoMove(int column, int row)
        {
            _board[column, row] = '\0';
        }

        internal void SetBoard(char[,] board)
        {
            _board = (char[,])board.Clone();
        }

        internal char[,] GetBoard()
        {
            return (char[,])_board.Clone();
        }
        
        internal void ClearBoard()
        {
            _board = new char[3,3];
        }

        // Returns my placed pieces - opponent placed pieces
        internal int GetNumberOfPiecesScore(char player)
        {
            var playerPieces = 0;
            var opponentPieces = 0;

            foreach (var cell in _board)
            {
                if(cell  == player)
                {
                    playerPieces++;
                }
                else if(cell != '\0')
                {
                    opponentPieces++;
                }
            }

            return playerPieces - opponentPieces;
        }

        public bool IsGameOver()
        {
            if(   EvaluateBoard() != 0
                  || AvailableSpacesOnBoard() == 0)
            {
                return true;
            }

            return false;
        }

        public int AvailableSpacesOnBoard()
        {
            var availableSpaces = 0;

            foreach (var cell in _board)
            {
                if(cell == '\0')
                {
                    availableSpaces++;
                }
            }

            return availableSpaces;
        }

        internal void PrintBoard()
        {
            Console.Error.WriteLine("------");

            for(var row = 0; row < _board.GetLength(1); row++)
            {
                for(var column = 0; column < _board.GetLength(0); column++)
                {
                    if(_board[column, row] == 'X')
                    {
                        Console.Error.Write("X");
                    }
                    else if(_board[column, row] == 'O')
                    {
                        Console.Error.Write("O");
                    }
                    else
                    {
                        Console.Error.Write(" ");
                    }

                    Console.Error.Write("|");
                }

                Console.Error.WriteLine();
                Console.Error.WriteLine("------");
            }
        }
    }
}