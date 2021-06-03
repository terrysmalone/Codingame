using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

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
        
        //var board = new Stack[9];

        // for (var i = 0; i < board.Length; i++)
        // {
        //     board[i] = new Stack();
        // }

        // game loop
        while (true)
        {
            var turnIndex = int.Parse(Console.ReadLine()); // starts from 0; As the game progresses, first player gets [0,2,4,...] and second player gets [1,3,5,...]
            for (var i = 0; i < 7; i++)
            {
                var boardRow = Console.ReadLine(); // one row of the board (from top to bottom)

                //var pieces = boardRow.ToCharArray();
                
                //for (var j = 0; j < pieces.Length; j++)
                //{
                //    board[j].Push(pieces[j]);
                //}
            }

            //PrintBoard(board);

            var numValidActions = int.Parse(Console.ReadLine()); // number of unfilled columns in the board
            for (var i = 0; i < numValidActions; i++)
            {
                var action = int.Parse(Console.ReadLine()); // a valid column index into which a chip can be dropped
            }
            var oppPreviousAction = int.Parse(Console.ReadLine()); // opponent's previous chosen column index (will be -1 for first player in the first turn)

            if (oppPreviousAction != -1)
            {
                game.AddOpponentMove(oppPreviousAction);
            }
            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");


            // Output a column index to drop the chip in. Append message to show in the viewer.
            game.AddMyMove(0);
            game.PrintBoard();
            Console.WriteLine("0");
        }
    }

    private static void PrintBoard(Stack[] board)
    {
        var columns = new List<object?[]>();
        
        foreach (var column in board)
        {
            columns.Add(column.ToArray());
        }
        
        Console.Error.WriteLine($"column count:{columns.Count}");
        
        for (var i = 6; i >= 0; i--)
        {
            for (var j = 0; j < 9; j++)
            {
                Console.Error.Write($"{columns[j][i]}");
            }

            Console.Error.WriteLine("");
        }
    }

    internal sealed class Game
    {
        private int _myId;
        private int _oppId;
        
        private Stack[] _board;
        private int _turn = 0;

        internal Game(int myId)
        {
            _myId = myId;
            _oppId = myId == 0 ? 1 : 0;
            
            _board = new Stack[9];

            for (var i = 0; i < _board.Length; i++)
            {
                _board[i] = new Stack();
            }
        }

        internal void AddOpponentMove(int column)
        {
            _board[column].Push(_oppId);
        }
        
        internal void AddMyMove(int column)
        {
            _board[column].Push(_myId);
        }
        
        internal void PrintBoard()
        {
            var columns = new List<object?[]>();
        
            foreach (var column in _board)
            {
                columns.Add(column.ToArray().Reverse().ToArray());
            }
        
            Console.Error.WriteLine($"column count:{columns.Count}");
        
            for (var i = 6; i >= 0; i--)            
            {
                var rowText = string.Empty;
                
                for (var j = 0; j < 9; j++)
                {
                    if (columns[j].Length >= i+1)
                    {
                        rowText += columns[j][i];
                    }
                    else
                    {
                        rowText += "-";
                    }
                }

                Console.Error.WriteLine(rowText);
            }
        }
    }
}