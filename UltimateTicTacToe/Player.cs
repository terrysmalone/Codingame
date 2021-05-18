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
            // 0 no move
            // 1 min
            // 2 opponenets
            
            string[] inputs;

            // game loop
            while (true)
            {
                inputs = Console.ReadLine().Split(' ');
                var opponentRow = int.Parse(inputs[0]);
                var opponentCol = int.Parse(inputs[1]);
                
                if(opponentRow != -1)
                {
                    game.AddOpponentMove(opponentRow, opponentCol);
                }
                
                DisplayBoard();

                var validActionCount = int.Parse(Console.ReadLine());
                
                var validActions = new List<int[]>();
                
                for (var i = 0; i < validActionCount; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    
                    var row = int.Parse(inputs[0]);
                    var column = int.Parse(inputs[1]);
                    validActions.Add(new []{ row, column });
                }
                
                game.ValidActions = validActions;

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");

                var action = game.getAction();
                Console.WriteLine($"{action[0]} {action[1]}");
            }
        }
    }

    internal sealed class Game
    {
        internal List<int[]> ValidActions { get; set; }
        
        internal int[,] _moves = new int[3,3];
        
        static int _moveCount = 0;
        
        internal Game()
        {
            ValidActions = new List<int[]>();
        }
        
        public int[,] getAction()
        {
            // Start on a corner
            if(_moveCount == 0)
            {
                if(_moves[0,0] == 0)
                {
                    MakeMove(0, 0);
                    return new int[0,0];
                }
                else if (_moves[2,0] == 0)
                {
                    MakeMove(2,0);
                    return new int[2,0];
                }
            }
            else
            {
                // var bestMove = string.Empty;
                // foreach (var validAction in validActions)
                // {
                //     // Find move with most winning routes
                //     int CalculateWinningRoutes(validAction);
                // }
                    
            }
        }
        private void MakeMove(int row, int column)
        {
            _moves[row,column] = 1;
            _moveCount++;
        }

        public void AddOpponentMove(int opponentRow, int opponentCol)
        {
            _moves[opponentRow, opponentCol] = 2;
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