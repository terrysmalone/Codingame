using System.Linq;
using NUnit.Framework;
using UltimateTicTacToe;

namespace UltimateTicTacToeTest
{
    [TestFixture]
    public class TicTacToeTests
    {
        [Test]
        public void OneMoveWinEvaluatesProperly_ForPlayerO_1([Range(1,10)] int depth)
        {
            // |X| |O|
            // |X| |O|
            // | | | |
            var ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(SetBoard("X-OX-O---"));
            
            var bestMove = ticTacToe.GetBestMove(depth, 'O');
            
            var expectedBestMove = new Move(2, 2);
            Assert.That(bestMove.Column, Is.EqualTo(expectedBestMove.Column));
            Assert.That(bestMove.Row, Is.EqualTo(expectedBestMove.Row));
        }
        
        [Test]
        public void OneMoveWinEvaluatesProperly_ForPlayerX_Inverse([Range(1,10)] int depth)
        {
            // |O| |X|
            // |O| |X|
            // | | | |
            var ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(SetBoard("O-XO-X---"));
            
            var bestMove = ticTacToe.GetBestMove(depth, 'X');
            
            var expectedBestMove = new Move(2, 2);
            Assert.That(bestMove.Column, Is.EqualTo(expectedBestMove.Column));
            Assert.That(bestMove.Row, Is.EqualTo(expectedBestMove.Row));
        }
        
        [Test]
        public void OneMoveWinEvaluatesProperly_ForPlayerO_2([Range(1,10)] int depth)
        {
            // |O|X|O|
            // | | |X|
            // |X| |O|
            var ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(SetBoard("OXO--XX-O"));
            
            var bestMove = ticTacToe.GetBestMove(depth, 'O');
            
            var expectedBestMove = new Move(1, 1);
            Assert.That(bestMove.Column, Is.EqualTo(expectedBestMove.Column));
            Assert.That(bestMove.Row, Is.EqualTo(expectedBestMove.Row));
        }
        
        [Test]
        public void OneMoveWinEvaluatesProperlyForPlayerX_1([Range(1,10)] int depth)
        {
            // | |X|O|
            // | |X| |
            // |O| | |
            var ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(SetBoard("-XO-X-O--"));
            
            var bestMove = ticTacToe.GetBestMove(depth, 'X');
            
            var expectedBestMove = new Move(1, 2);
            Assert.That(bestMove.Column, Is.EqualTo(expectedBestMove.Column));
            Assert.That(bestMove.Row, Is.EqualTo(expectedBestMove.Row));
        }
        
        [Test]
        public void OneMoveWinEvaluatesProperlyForPlayerX_2([Range(1,10)] int depth)
        {
            // | | |O|
            // |X|X| |
            // |O| | |
            var ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(SetBoard("--OXX-O--"));
            
            var bestMove = ticTacToe.GetBestMove(depth, 'X');
            
            var expectedBestMove = new Move(2, 1);
            Assert.That(bestMove.Column, Is.EqualTo(expectedBestMove.Column));
            Assert.That(bestMove.Row, Is.EqualTo(expectedBestMove.Row));
        }
        
        [Test]
        public void TwoMoveWinEvaluatesProperlyForPlayer0([Range(3,10)] int depth)  // 3 is the earliest is can see
        {
            // |O| | |
            // |X| | |
            // |O| |X|
            var ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(SetBoard("O--X--O-X"));
            
            var bestMove = ticTacToe.GetBestMove(depth, 'O');
            
            var expectedBestMove = new Move(2, 0);
            Assert.That(bestMove.Column, Is.EqualTo(expectedBestMove.Column));
            Assert.That(bestMove.Row, Is.EqualTo(expectedBestMove.Row));
        }
        
        [Test]
        public void LosingGameDelaysLoss_ForPlayerO([Range(3,10)] int depth)
        {
            // | |X| |
            // | | |X|
            // |O|O|X|
            var ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(SetBoard("-X---XOOX"));
            
            var bestMove = ticTacToe.GetBestMove(depth, 'O');
            
            var expectedBestMove = new Move(2, 0);
            Assert.That(bestMove.Column, Is.EqualTo(expectedBestMove.Column));
            Assert.That(bestMove.Row, Is.EqualTo(expectedBestMove.Row));
        }
        
        [Test]
        public void LosingGameDelaysLoss_ForPlayerX([Range(3,10)] int depth)
        {
            // | |O| |
            // | | |O|
            // |X|X|O|
            var ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(SetBoard("-O---OXXO"));
            
            var bestMove = ticTacToe.GetBestMove(depth, 'X');
            
            var expectedBestMove = new Move(2, 0);
            Assert.That(bestMove.Column, Is.EqualTo(expectedBestMove.Column));
            Assert.That(bestMove.Row, Is.EqualTo(expectedBestMove.Row));
        }
        
        [Test]
        public void WinningGameGoesForFastestWin_ForPlayerX([Range(3,10)] int depth)
        {
            // The only non winning move is 1, 2
            // All other moves win in 3 except 1,0 
            // which wins in 1. We expect it to pick that
            // 
            // |X| |X|
            // | | |O|
            // | | |O|
            var ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(SetBoard("X-X--O--O"));
            
            var bestMove = ticTacToe.GetBestMove(depth, 'X');
            
            var expectedBestMove = new Move(1, 0);
            Assert.That(bestMove.Column, Is.EqualTo(expectedBestMove.Column));
            Assert.That(bestMove.Row, Is.EqualTo(expectedBestMove.Row));
        }
        
        [Test]
        public void WinningGameGoesForFastestWin_ForPlayerO([Range(3,10)] int depth)
        {
            // The only non winning move is 1, 2
            // All other moves win in 3 except 1,0 
            // which wins in 1. We expect it to pick that
            // 
            // |O| |O|
            // | | |X|
            // | | |X|
            var ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(SetBoard("O-O--X--X"));
            
            var bestMove = ticTacToe.GetBestMove(depth, 'O');
            
            var expectedBestMove = new Move(1, 0);
            Assert.That(bestMove.Column, Is.EqualTo(expectedBestMove.Column));
            Assert.That(bestMove.Row, Is.EqualTo(expectedBestMove.Row));
        }

        [Test]
        public void GetNumberOfPiecesScore_EmptyBoard()
        {
            var ticTacToe = new TicTacToe();

            Assert.That(ticTacToe.GetNumberOfPiecesScore('X'), Is.EqualTo(0));
            Assert.That(ticTacToe.GetNumberOfPiecesScore('O'), Is.EqualTo(0));
        }
        
        [Test]
        public void GetNumberOfPiecesScore_EvenGame()
        {
            // |O| |O|
            // | | |X|
            // | | |X|
            var ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(SetBoard("O-O--X--X"));
            
            Assert.That(ticTacToe.GetNumberOfPiecesScore('X'), Is.EqualTo(0));
            Assert.That(ticTacToe.GetNumberOfPiecesScore('O'), Is.EqualTo(0));
        }
        
        [Test]
        public void GetNumberOfPiecesScore_OWinning()
        {
            // The only non winning move is 1, 2
            // All other moves win in 3 except 1,0 
            // which wins in 1. We expect it to pick that
            // 
            // |X|O|X|
            // | | |O|
            // | | |O|
            var ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(SetBoard("XOX--O--O"));
            
            Assert.That(ticTacToe.GetNumberOfPiecesScore('X'), Is.EqualTo(-1));
            Assert.That(ticTacToe.GetNumberOfPiecesScore('O'), Is.EqualTo(1));
        }
        
        [Test]
        public void GetNumberOfPiecesScore_XWinning()
        {
            // | |X| |
            // | | |X|
            // |O|O|X|
            var ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(SetBoard("-X---XOOX"));
            
            Assert.That(ticTacToe.GetNumberOfPiecesScore('X'), Is.EqualTo(1));
            Assert.That(ticTacToe.GetNumberOfPiecesScore('O'), Is.EqualTo(-1));
        }

        [Test]
        public void IsGameOVer_IfOWins()
        {
            // | |X| |
            // | | |X|
            // |O|O|O|
            var ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(SetBoard("-X---XOOO"));
            
            Assert.That(ticTacToe.IsGameOver());
        }
        
        [Test]
        public void IsGameOVer_IfXWins()
        {
            // |X| | |
            // | |X| |
            // |O|O|X|
            var ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(SetBoard("X---X-OOX"));
            
            Assert.That(ticTacToe.IsGameOver());
        }
        
        [TestCase("XXX------")]
        [TestCase("---XXX---")]
        [TestCase("------XXX")]
        [TestCase("X--X--X--")]
        [TestCase("-X--X--X-")]
        [TestCase("--X--X--X")]
        [TestCase("X---X---X")]
        [TestCase("--X-X-X--")]
        public void IsGameOver_CheckAllLines(string boardPieces)
        {
            var ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(SetBoard(boardPieces));
            
            Assert.That(ticTacToe.IsGameOver());
        }
        
        [Test]
        public void IsGameOver_IfWonBoardIsFull()
        {
            // |X|X|O|
            // |X|O|O|
            // |X|O|X|
            var ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(SetBoard("XXOXOOXOX"));
            
            Assert.That(ticTacToe.IsGameOver());
        }
        
        [Test]
        public void IsGameOver_IfNonWonBoardIsFull()
        {
            // |X|O|O|
            // |O|X|X|
            // |X|O|O|
            var ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(SetBoard("XOOOXXXOO"));
            
            Assert.That(ticTacToe.IsGameOver());
        }
        
        [Test]
        public void IsGameOver_FalseIfEmpty()
        {
            var  board = new char[3,3];
            
            var ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(board);
            
            Assert.That(ticTacToe.IsGameOver(), Is.False);
        }
        
        [Test]
        public void IsGameOver_FalseForPartiallyPlayedBoard()
        {
            // | | |O|
            // | | | |
            // |X|X| |
            var ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(SetBoard("--O---XX-"));
            
            Assert.That(ticTacToe.IsGameOver(), Is.False);
        }
        
        [Test]
        public void GetScores([Range(1,10)] int depth)
        {
            // |O|X|X|
            // |X|O|O|
            // | |O|X|
            var ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(SetBoard("OXXXOO-OX"));
            
            var bestMove = ticTacToe.GetMoveScores(depth, 'X');
            
            var expectedBestMove = new Move(2, 2);
            //Assert.That(bestMove.Column, Is.EqualTo(expectedBestMove.Column));
            //Assert.That(bestMove.Row, Is.EqualTo(expectedBestMove.Row));
        }
        
        // 
        // |0|1|2|
        // |3|4|5|
        // |6|7|8|
        // "-" for empty
        private static char[,] SetBoard(string boardString)
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