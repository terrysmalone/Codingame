using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleToAttribute("UltimateTicTacToeTest")]
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

                if(moveNum == 0)
                {
                    if(opponentRow != -1)
                    {
                        game.SetPlayer('O');
                        game.AddMove(opponentCol, opponentRow, game.EnemyPiece);
                        moveNum++;
                    }
                    else
                    {
                        game.SetPlayer('X');
                    }
                }
                else
                {
                    game.AddMove(opponentCol, opponentRow, game.EnemyPiece);
                    moveNum++;
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

                game.AddMove(action.Column, action.Row, game.PlayerPiece);
                Console.WriteLine($"{action.Row} {action.Column}");

                moveNum++;
            }
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
