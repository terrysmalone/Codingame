using System;
using System.Collections.Generic;
using System.Linq;

namespace UltimateTicTacToe
{
    /**
     * Auto-generated code below aims at helping you parse
     * the standard input according to the problem statement.
     **/
    class Player
    {
        static void Main(string[] args)
        {
            var game = new Game();

            string[] inputs;
            var moveNum = 0;
            
            // game loop
            while (true)
            {
                inputs = Console.ReadLine().Split(' ');
                var opponentRow = int.Parse(inputs[0]);
                var opponentCol = int.Parse(inputs[1]);
                
                if(opponentRow != -1)
                {
                    game.AddMove(opponentCol, opponentRow, false);
                    moveNum++;
                }
                
                if(moveNum == 0 && opponentCol == -1)
                {
                    game.SetPlayer('X');
                }
                else if(moveNum == 1)
                {
                    game.SetPlayer('O');
                }

                var validActionCount = int.Parse(Console.ReadLine());
                
                var validActions = new List<Move>();
                
                for (var i = 0; i < validActionCount; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    
                    var row = int.Parse(inputs[0]);
                    var column = int.Parse(inputs[1]);
                    validActions.Add(new Move(column, row));
                }

                game.ValidActions = validActions;
                
                // If we're first might as well pick a corner
                var action = game.GetAction();
                
                game.AddMove(action.Column, action.Row, true);
                Console.WriteLine($"{action.Row} {action.Column}");
                
                moveNum++;
            }
        }
    }

    internal sealed class Game
    {
        internal List<Move> ValidActions { get; set; }
        
        private TicTacToe[,] _boards = new TicTacToe[3,3];
        private TicTacToe _overArchingTicTacToe;
        private char _player = '\0';
        
        private int _depth = 6;
        
        public Game()
        {
            _boards[0,0] = new TicTacToe();
            _boards[0,1] = new TicTacToe();
            _boards[0,2] = new TicTacToe();
            _boards[1,0] = new TicTacToe();
            _boards[1,1] = new TicTacToe();
            _boards[1,2] = new TicTacToe();
            _boards[2,0] = new TicTacToe();
            _boards[2,1] = new TicTacToe();
            _boards[2,2] = new TicTacToe();
        
            _overArchingTicTacToe = new TicTacToe();
        }

        public Move GetAction()
        {
            TicTacToe boardInPlay = null;
            
            var boardInPlayColumn = 0;
            var boardInPlayRow = 0;
            
            
            // Identify which board we're playing on (it could be them all)
            
            // If the range between either row or column is 3 or more we're being given a choice from multiple boards
            if(   ValidActions.Max(a => a.Column) - ValidActions.Min(a => a.Column) >= 3
               || ValidActions.Max(a => a.Row) - ValidActions.Min(a => a.Row) >= 3)
            {
                // We get to choose which board to play
                // We're testing. Just play the first one
                boardInPlayColumn = ValidActions.First().Column/3;
                boardInPlayRow = ValidActions.First().Row/3;
                boardInPlay = _boards[boardInPlayColumn,boardInPlayRow];
            }
            else
            {
                boardInPlayColumn = ValidActions.First().Column/3;
                boardInPlayRow = ValidActions.First().Row/3;
                boardInPlay = _boards[boardInPlayColumn, boardInPlayRow];
            }
            
            // Make a move on that board
            var bestMove = boardInPlay.GetBestMove(_depth, _player);
            
            return new Move(boardInPlayColumn * 3 + bestMove.Column, boardInPlayRow * 3 + bestMove.Row);
        }
        
        internal void AddMove(int column, int row, bool mine)
        {
            _boards[column / 3, row / 3].AddMove(column % 3, row % 3, mine);
        }
        public void SetPlayer(char player)
        {
            _player = player;
        }
    }
    
    public sealed class TicTacToe
    {
        private char[,] _board = new char[3,3];

        public Move GetBestMove(int depth, char startingPlayer)
        {
            return CalculateMove(depth, startingPlayer);
        }

        private Move CalculateMove(int depth, char player)
        {
            var maxScore = int.MinValue;
            
            var validMoves = CalculateValidMoves();
            var bestMove = validMoves.First();
            
            foreach (var validAction in validMoves) 
            {
                var maximisingPlayer = player == 'X';
                
                AddMove(validAction.Column, validAction.Row, maximisingPlayer);
                
                var score = -Calculate(depth-1, !maximisingPlayer);
                
                UndoMove(validAction.Column, validAction.Row);

                if (score > maxScore)
                {
                    maxScore = score;
                    bestMove = validAction;
                }
            }
            
            return bestMove;
        }
        
        private int Calculate(int depth, bool maximisingPlayer)
        {
            if (depth == 0)
            {
                return Evaluate(maximisingPlayer, depth);
            }
            
            var evaluation = Evaluate(maximisingPlayer, depth);
            
            if(evaluation != 0)
            {
                return evaluation;
            }
            
            var maxScore = int.MinValue;
            
            foreach (var move in CalculateValidMoves())
            {
                AddMove(move.Column, move.Row, maximisingPlayer);
                
                var score = -Calculate(depth-1, !maximisingPlayer);
                
                UndoMove(move.Column, move.Row);
                
                if (score > maxScore)
                {
                    maxScore = score;
                }
            }
            
            return maxScore;
        }
        
        private List<Move> CalculateValidMoves()
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

        private int Evaluate(bool maximisingPlayer, int currentDepth)
        {
            int score;
            
            if(maximisingPlayer)
            {
                score = EvaluateBoard(currentDepth);
            }
            else
            {
                score = -EvaluateBoard(currentDepth);
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
       
        
        private int EvaluateBoard(int currentDepth)
        {
            foreach (var line in _lines)
            {
                var playerWithLine = PlayerWithLine(line);
                
                if(playerWithLine == 'X')
                {
                    return 1 + currentDepth;
                }
                
                if(playerWithLine == 'O')
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
        
        internal void AddMove(int row, int column, bool maximisingPlayer)
        {
            if(maximisingPlayer)
            {
                _board[row, column] = 'X';
            }
            else
            {
                _board[row, column] = 'O';
            }
        }
        
        private void UndoMove(int row, int column)
        {
            _board[row, column] = '\0';
        }

        public  void SetBoard(char[,] board)
        {
            _board = (char[,])board.Clone();
        }
    }

    public sealed class Move
    {
        public int Row { get; }
        public int Column { get; }

        public Move(int column, int row)
        {
            Column = column;
            Row = row;
        }
    }
}

// while (true)
// {
//     inputs = Console.ReadLine().Split(' ');
//     int opponentRow = int.Parse(inputs[0]);
//     int opponentCol = int.Parse(inputs[1]);
//     int validActionCount = int.Parse(Console.ReadLine());
//     for (int i = 0; i < validActionCount; i++)
//     {
//         inputs = Console.ReadLine().Split(' ');
//         int row = int.Parse(inputs[0]);
//         int col = int.Parse(inputs[1]);
//     }
//
//     // Write an action using Console.WriteLine()
//     // To debug: Console.Error.WriteLine("Debug messages...");
//
//     Console.WriteLine("0 0");
// }