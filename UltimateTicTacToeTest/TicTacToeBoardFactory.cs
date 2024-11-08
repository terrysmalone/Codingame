using System.Linq;

namespace UltimateTicTacToeTest
{
    internal static class TicTacToeBoardFactory
    {
        // 
        // |0|1|2|
        // |3|4|5|
        // |6|7|8|
        // "-" for empty
        internal static TicTacToe GetTicTacToeBoard(string boardString)
        {
            TicTacToe ticTacToe = new TicTacToe();

            char[,] board = GetBoard(boardString);
            
            ticTacToe.SetBoard(board);
            
            return ticTacToe;
        }
        
        internal static char[,] GetBoard(string boardString)
        {
            char[,] board = new char[3,3];

            int position = 0;
            
            for(int row = 0; row < board.GetLength(1); row++)
            {
                for(int column = 0; column < board.GetLength(0); column++)
                {
                    string letter = boardString.Substring(position, 1);
                    if(letter != "-")
                    {
                        board[column, row] = letter.ToCharArray().First();
                    }
                    
                    position++;
                }
            }
            
            return board;
        }
    }
}