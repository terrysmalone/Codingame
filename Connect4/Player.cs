using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleToAttribute("Connect4Tests")]
namespace Connect4
{
    /**
     * Drop chips in the columns.
     * Connect at least 4 of your chips in any direction to win.
     **/
    class Player
    {
        static void Main(string[] args)
        {
            var inputs = Console.ReadLine().Split(' ');
            var myId = int.Parse(inputs[0]); // 0 or 1 (Player 0 plays first)
            var oppId = int.Parse(inputs[1]); // if your index is 0, this will be 1, and vice versa

            var game = new Game(myId);
            
            // game loop
            while (true)
            {
                var turnIndex = int.Parse(Console.ReadLine()); // starts from 0; As the game progresses, first player gets [0,2,4,...] and second player gets [1,3,5,...]
                for (var i = 0; i < 7; i++)
                {
                    var boardRow = Console.ReadLine(); // one row of the board (from top to bottom)
                }

                var numValidActions = int.Parse(Console.ReadLine()); // number of unfilled columns in the board
                for (var i = 0; i < numValidActions; i++)
                {
                    var action = int.Parse(Console.ReadLine()); // a valid column index into which a chip can be dropped
                }
                var oppPreviousAction = int.Parse(Console.ReadLine()); // opponent's previous chosen column index (will be -1 for first player in the first turn)

                if (oppPreviousAction != -1)
                {
                    if(oppPreviousAction == -2)
                    {
                        game.AddOpponentSteal();
                    }
                    else
                    {
                        game.AddOpponentMove(oppPreviousAction);
                    }
                }
                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");


                // Output a column index to drop the chip in. Append message to show in the viewer.
                var move = game.GetMove();
                Console.WriteLine(move);
            }
        }
    }
}