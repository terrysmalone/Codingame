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
            var ticTacToe = new TicTacToe();
            
            var board = GetBoard(boardString);
            
            ticTacToe.SetBoard(board);
            
            return ticTacToe;
        }
        
        internal static char[,] GetBoard(string boardString)
        {
            var board = new char[3,3];
            
            var position = 0;
            
            for(var row = 0; row < board.GetLength(1); row++)
            {
                for(var column = 0; column < board.GetLength(0); column++)
                {
                    var letter = boardString.Substring(position, 1);
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