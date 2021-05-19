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
                
                if(opponentRow != -1)
                {
                    game.AddMove(opponentRow, opponentCol, false);
                }
                
                //DisplayBoard();

                var validActionCount = int.Parse(Console.ReadLine());
                
                var validActions = new List<Tuple<int, int>>();
                
                for (var i = 0; i < validActionCount; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    
                    var row = int.Parse(inputs[0]);
                    var column = int.Parse(inputs[1]);
                    validActions.Add(new Tuple<int, int>(row, column));
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
        
        // 0 Empty
        // 1 mine
        // 2 opponents
        private int[,] _board = new int[3,3];
        
        internal int MoveNum { get; private set;}

        private int _startingDepth = 10;
        public Tuple<int,int> GetAction()
        {
            var bestMove = CalculateMove(_startingDepth);
            
            if(!ValidActions.Any(a => a.Item1 == bestMove.Item1 && a.Item2 == bestMove.Item2))
            {
                //Console.Error.WriteLine($"{bestMove.Item1}, {bestMove.Item1}");
                return ValidActions.First();
            }
            
            return bestMove;
        }
        private Tuple<int, int> CalculateMove(int startingDepth)
        {
            var bestMove = new Tuple<int, int>(-1,-1);
            var maxScore = int.MinValue;

            foreach (var validAction in ValidActions)
            {
                AddMove(validAction.Item1, validAction.Item2, true);
                
                var score = -Calculate(startingDepth-1, false);
                
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
            var evaluation = Evaluate(maximisingPlayer, depth);
            
            if(evaluation != 0)
            {
                return evaluation;
            }
        
            if (depth == 0)
            {
                return Evaluate(maximisingPlayer, depth);
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
            
            for(var row = 0; row < _board.GetLength(0); row++)
            {
                for(var column = 0; column < _board.GetLength(1); column++)
                {
                    if(_board[row, column] == 0)
                    {
                        moves.Add(new Tuple<int, int>(row, column));
                    }
                }
            }
           
            return moves;
        }

        private int Evaluate(bool maximisingPlayer, int currentDepth)
        {
            var score = EvaluateBoard();
            
            if(score != 0)
            {
                score += currentDepth;
            }
        
            if(!maximisingPlayer)
            {
                score = -score;
            }
            
            return score;
        }

        private List<Tuple<int, int>[]> _lines = new List<Tuple<int, int>[]>
        {
                new[] { new Tuple<int, int>(0,0), new Tuple<int, int>(0,1), new Tuple<int, int>(0,2) },
                new[] { new Tuple<int, int>(1,0), new Tuple<int, int>(1,1), new Tuple<int, int>(1,2) },
                new[] { new Tuple<int, int>(2,0), new Tuple<int, int>(2,1), new Tuple<int, int>(2,2) },
                    
                new[] { new Tuple<int, int>(0,0), new Tuple<int, int>(1,0), new Tuple<int, int>(2,0) },
                new[] { new Tuple<int, int>(0,1), new Tuple<int, int>(1,1), new Tuple<int, int>(2,1) },
                new[] { new Tuple<int, int>(0,2), new Tuple<int, int>(1,2), new Tuple<int, int>(2,2) },
                    
                new[] { new Tuple<int, int>(0,0), new Tuple<int, int>(1,1), new Tuple<int, int>(2,2) },
                new[] { new Tuple<int, int>(2,0), new Tuple<int, int>(1,1), new Tuple<int, int>(0,2) }
            };
        
        private int EvaluateBoard()
        {
            foreach (var line in _lines)
            {
                var playerWithLine = PlayerWithLine(line);
                
                if(playerWithLine == 1)
                {
                    return 10;
                }
                else if(playerWithLine == 2)
                {
                    return -10;
                }
            }
            
            return 0;
        }
        private int PlayerWithLine(Tuple<int, int>[] line)
        {
            if (DoesPlayerHaveLine(line, 1))
            {
                return 1;
            }
            else if (DoesPlayerHaveLine(line, 2))
            {
                return 2;
            }
            
            return 0;
        }
        private bool DoesPlayerHaveLine(Tuple<int, int>[] line, int player)
        {
            if(   _board[line[0].Item1, line[0].Item2] == player 
               && _board[line[1].Item1, line[1].Item2] == player
               && _board[line[2].Item1, line[2].Item2] == player)
            {
                return true;
            }
            
            return false;
        }

        internal void AddMove(int row, int column, bool mine)
        {
            _board[row, column] = mine ? 1 : 2;
            MoveNum++;
        }
        
        internal void UndoMove(int row, int column)
        {
            _board[row, column] = 0;
            MoveNum--;
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