namespace Connect4; 

using System;

/**
 * Drop chips in the columns.
 * Connect at least 4 of your chips in any direction to win.
 **/
class Player
{
    static void Main(string[] args)
    {
        string[] inputs = Console.ReadLine().Split(' ');
        int myId = int.Parse(inputs[0]); // 0 or 1 (Player 0 plays first)
        int oppId = int.Parse(inputs[1]); // if your index is 0, this will be 1, and vice versa

        Game game = new Game(myId);
        
        // game loop
        while (true)
        {
            int turnIndex = int.Parse(Console.ReadLine()); // starts from 0; As the game progresses, first player gets [0,2,4,...] and second player gets [1,3,5,...]
            for (int i = 0; i < 7; i++)
            {
                string boardRow = Console.ReadLine(); // one row of the board (from top to bottom)
            }

            int numValidActions = int.Parse(Console.ReadLine()); // number of unfilled columns in the board
            for (int i = 0; i < numValidActions; i++)
            {
                int action = int.Parse(Console.ReadLine()); // a valid column index into which a chip can be dropped
            }
            int oppPreviousAction = int.Parse(Console.ReadLine()); // opponent's previous chosen column index (will be -1 for first player in the first turn)

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
            string move = game.GetMove();
            Console.WriteLine(move);
        }
    }
}