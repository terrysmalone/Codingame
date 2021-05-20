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

            // game loop
            while (true)
            {
                inputs = Console.ReadLine().Split(' ');
                var opponentRow = int.Parse(inputs[0]);
                var opponentCol = int.Parse(inputs[1]);
                
                Console.Error.WriteLine($"opponentRow:{opponentRow}, opponentCol:{opponentCol}");
                
                if(opponentRow != -1)
                {
                    game.AddMove(opponentCol, opponentRow, false);
                }
                
                //DisplayBoard();

                var validActionCount = int.Parse(Console.ReadLine());
                
                var validActions = new List<Tuple<int, int>>();
                
                for (var i = 0; i < validActionCount; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    
                    var row = int.Parse(inputs[0]);
                    var column = int.Parse(inputs[1]);
                    validActions.Add(new Tuple<int, int>(column, row));
                    Console.Error.WriteLine($"i:{i}, row:{row}, column:{column}");
                }
                
                game.ValidActions = validActions;

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");
                
                // If we're first might as well pick a corner
                if(game.MoveNum == 0)
                {
                    game.AddMove(0, 0, true);
                    Console.WriteLine($"{0} {0}");
                }
                else
                {
                    var action = game.GetAction();
                    game.AddMove(action.Item1, action.Item2, true);
                    Console.WriteLine($"{action.Item1} {action.Item2}");
                }
            }
        }
    }

    internal sealed class Game
    {
        internal List<Tuple<int,int>> ValidActions { get; set; }
        
        // '' Empty
        // 'O' Noughts
        // 'X' Crosses
        private char[,] _board = new char[3,3];
        
        private TicTacToe _ticTacToe;
        
        internal int MoveNum { get; private set;}
        
        public Game()
        {
            _ticTacToe = new TicTacToe();
        }

        public Tuple<int,int> GetAction()
        {
            var bestMove = _ticTacToe.GetBestMove(_board, 10, 'O');
            
            
            if(!ValidActions.Any(a => a.Item1 == bestMove.Item1 && a.Item2 == bestMove.Item2))
            {
                return ValidActions.First();
            }
            
            return bestMove;
        }
        
        internal void AddMove(int column, int row, bool mine)
        {
            _board[column, row] = mine ? 'O' : 'X';
            MoveNum++;
        }
    }
    
    public sealed class TicTacToe
    {
        private int _startingDepth = 10;
        private char[,] _board;
        
        public Tuple<int, int> GetBestMove(char[,] board, int depth, char startingPlayer)
        {
            _startingDepth = depth;
            _board = board;
            
            return CalculateMove(depth, startingPlayer);
        }
        
        private Tuple<int, int> CalculateMove(int depth, char player)
        {
            var maxScore = int.MinValue;
            
            var validMoves = CalculateValidMoves();
            var bestMove = validMoves.First();
            
            foreach (var validAction in validMoves) 
            {
                var maximisingPlayer = player == 'O';
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

            //if(score != 0)
            //{
            //    if(playerPerspective == 'O')
            //    {
            //        score += currentDepth;
            //    }
            //    else
            //    {
            //        score -= currentDepth;
           //    }
           // }
            
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
        private char PlayerWithLine(Tuple<int, int>[] line)
        {
            if (DoesPlayerHaveLine(line, 'O'))
            {
                return 'O';
            }
            else if (DoesPlayerHaveLine(line, 'X'))
            {
                return 'X';
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
                _board[row, column] = 'O';
            }
            else
            {
                _board[row, column] = 'X';
            }
            
            var oCount = 0;
            var xCount = 0;
            var emptyCount = 0;
            
            for(var x = 0; x < _board.GetLength(0); x++)
            {
                for(var y = 0; y < _board.GetLength(1); y++)
                {
                    switch (_board[x, y])
                    {
                        case '\0':
                            emptyCount++;
                            break;
                        case 'O':
                            oCount++;
                            break;
                        case 'X':
                            xCount++;
                            break;
                    }
                }
            }
            
            if(oCount + xCount + emptyCount != 9)
            {
                throw new NotImplementedException("Wrong total");
            }
            
            if(Math.Abs(oCount - xCount) > 1)
            {
                throw new NotImplementedException("Too many of one argument");
            }
            
            
        }
        
        internal void UndoMove(int row, int column)
        {
            _board[row, column] = '\0';
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