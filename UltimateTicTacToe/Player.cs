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
                
                var validActions = new List<Tuple<int, int>>();
                
                for (var i = 0; i < validActionCount; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    
                    var row = int.Parse(inputs[0]);
                    var column = int.Parse(inputs[1]);
                    validActions.Add(new Tuple<int, int>(column, row));
                }

                game.ValidActions = validActions;
                
                // If we're first might as well pick a corner
                var action = game.GetAction();
                
                game.AddMove(action.Item1, action.Item2, true);
                Console.WriteLine($"{action.Item2} {action.Item1}");
                
                moveNum++;
            }
        }
    }

    internal sealed class Game
    {
        internal List<Tuple<int,int>> ValidActions { get; set; }
        
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

        public Tuple<int,int> GetAction()
        {
            TicTacToe boardInPlay = null;
            
            var boardInPlayColumn = 0;
            var boardInPlayRow = 0;
            
            
            // Identify which board we're playing on (it could be them all)
            
            // If the range between either row or column is 3 or more we're being given a choice from multiple boards
            if(   ValidActions.Max(a => a.Item1) - ValidActions.Min(a => a.Item1) >= 3
               || ValidActions.Max(a => a.Item2) - ValidActions.Min(a => a.Item2) >= 3)
            {
                // We get to choose which board to play
                // We're testing. Just play the first one
                boardInPlayColumn = ValidActions.First().Item1/3;
                boardInPlayRow = ValidActions.First().Item2/3;
                boardInPlay = _boards[boardInPlayColumn,boardInPlayRow];
            }
            else
            {
                boardInPlayColumn = ValidActions.First().Item1/3;
                boardInPlayRow = ValidActions.First().Item2/3;
                boardInPlay = _boards[boardInPlayColumn, boardInPlayRow];
            }
            
            // Make a move on that board
            var bestMove = boardInPlay.GetBestMove(_depth, _player);
            
            return new Tuple<int, int>(boardInPlayColumn * 3 + bestMove.Item1, boardInPlayRow * 3 + bestMove.Item2);
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

        public Tuple<int, int> GetBestMove(int depth, char startingPlayer)
        {
            return CalculateMove(depth, startingPlayer);
        }
        
        private Tuple<int, int> CalculateMove(int depth, char player)
        {
            var maxScore = int.MinValue;
            
            var validMoves = CalculateValidMoves();
            var bestMove = validMoves.First();
            
            foreach (var validAction in validMoves) 
            {
                var maximisingPlayer = player == 'X';
                
                AddMove(validAction.Item1, validAction.Item2, maximisingPlayer);
                
                var score = -Calculate(depth-1, !maximisingPlayer);
                
                UndoMove(validAction.Item1, validAction.Item2);

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
                AddMove(move.Item1, move.Item2, maximisingPlayer);
                
                var score = -Calculate(depth-1, !maximisingPlayer);
                
                UndoMove(move.Item1, move.Item2);
                
                if (score > maxScore)
                {
                    maxScore = score;
                }
            }
            
            return maxScore;
        }
        
        private List<Tuple<int, int>> CalculateValidMoves()
        {
            var moves = new List<Tuple<int, int>>();
            
            for(var column = 0; column < _board.GetLength(0); column++)
            {
                for(var row = 0; row < _board.GetLength(1); row++)
                {
                    if(_board[column, row] == '\0')
                    {
                        moves.Add(new Tuple<int, int>(column, row));
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

        private List<Tuple<int, int>[]> _lines = new List<Tuple<int, int>[]>
        {
                new[] { new Tuple<int, int>(0,0), new Tuple<int, int>(0,1), new Tuple<int, int>(0,2) }, // Left column
                new[] { new Tuple<int, int>(1,0), new Tuple<int, int>(1,1), new Tuple<int, int>(1,2) }, // Middle column
                new[] { new Tuple<int, int>(2,0), new Tuple<int, int>(2,1), new Tuple<int, int>(2,2) }, // Right column
                    
                new[] { new Tuple<int, int>(0,0), new Tuple<int, int>(1,0), new Tuple<int, int>(2,0) }, // Top row
                new[] { new Tuple<int, int>(0,1), new Tuple<int, int>(1,1), new Tuple<int, int>(2,1) }, // middle row
                new[] { new Tuple<int, int>(0,2), new Tuple<int, int>(1,2), new Tuple<int, int>(2,2) }, // Bottom row
                    
                new[] { new Tuple<int, int>(0,0), new Tuple<int, int>(1,1), new Tuple<int, int>(2,2) }, // top left to bottom right diagonal
                new[] { new Tuple<int, int>(2,0), new Tuple<int, int>(1,1), new Tuple<int, int>(0,2) }  // bottom left to top right diagonal
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
        private char PlayerWithLine(Tuple<int, int>[] line)
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
        private bool DoesPlayerHaveLine(Tuple<int, int>[] line, char player)
        {
            if(   _board[line[0].Item1, line[0].Item2] == player 
               && _board[line[1].Item1, line[1].Item2] == player
               && _board[line[2].Item1, line[2].Item2] == player)
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