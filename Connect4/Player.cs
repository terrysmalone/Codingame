namespace Connect4; 

using System;
using System.Collections.Generic;
using System.Linq;

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

        int myActualId = 0;

        if(myId == 0)
        {
            myActualId = 1;
        }
        else
        {
            myActualId = -1;
        }

        Game game = new Game(myActualId);

        int rows = 7;
        int columns = 9;
        

        // game loop
        while (true)
        {
            int[,] board = new int[rows, columns];
            int turnIndex = int.Parse(Console.ReadLine()); // starts from 0; As the game progresses, first player gets [0,2,4,...] and second player gets [1,3,5,...]
            game.SetTurnIndex(turnIndex);

            for (int row = 0; row < rows; row++)
            {
                string boardRow = Console.ReadLine(); // one row of the board (from top to bottom)
                ParseRow(board, row, boardRow);
            }

            int numValidActions = int.Parse(Console.ReadLine()); // number of unfilled columns in the board
            for (int i = 0; i < numValidActions; i++)
            {
                int action = int.Parse(Console.ReadLine()); // a valid column index into which a chip can be dropped
            }

            int oppPreviousAction = int.Parse(Console.ReadLine()); // opponent's previous chosen column index (will be -1 for first player in the first turn)

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");


            // Output a column index to drop the chip in. Append message to show in the viewer.
            string move = game.GetMove(board);
            Console.WriteLine(move);
        }
    }

    private static void ParseRow(int[,] board, int row, string boardRow)
    {
        string[] colArray = boardRow.Select(c => c.ToString()).ToArray();

        for (int col = 0; col < colArray.Length; col++)
        {
            int piece = 0;
            if (colArray[col] == "0")
            {
                piece = 1;
            }
            else if (colArray[col] == "1")
            {
                piece = -1;
            }

            board[row, col] = piece;
        }
    }
}