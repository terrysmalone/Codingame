using System;
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
            var  board = new char[3,3];
            board[0,0] = 'X';
            board[0,1] = 'X';
            board[2,0] = 'O';
            board[2,1] = 'O';

            var ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(board);
            
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
            var  board = new char[3,3];
            board[0,0] = 'O';
            board[0,1] = 'O';
            board[2,0] = 'X';
            board[2,1] = 'X';

            var ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(board);
            
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
            var  board = new char[3,3];
            board[0,0] = 'O';
            board[1,0] = 'X';
            board[2,0] = 'O';
            board[2,1] = 'X';
            board[0,2] = 'X';
            board[2,2] = 'O';
            

            var ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(board);
            
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
            var  board = new char[3,3];
            board[1,0] = 'X';
            board[2,0] = 'O';
            board[1,1] = 'X';
            board[0,2] = 'O';

            var ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(board);
            
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
            var  board = new char[3,3];
            board[2,0] = 'O';
            board[0,1] = 'X';
            board[1,1] = 'X';
            board[0,2] = 'O';

            var ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(board);
            
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
            var  board = new char[3,3];
            board[0,0] = 'O';
            board[0,1] = 'X';
            board[0,2] = 'O';
            board[2,2] = 'X';

            var ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(board);
            
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
            var  board = new char[3,3];
            board[1,0] = 'X';
            board[2,1] = 'X';
            board[0,2] = 'O';
            board[1,2] = 'O';
            board[2,2] = 'X';

            var ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(board);
            
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
            var  board = new char[3,3];
            board[1,0] = 'O';
            board[2,1] = 'O';
            board[0,2] = 'X';
            board[1,2] = 'X';
            board[2,2] = 'O';

            var ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(board);
            
            var bestMove = ticTacToe.GetBestMove(depth, 'X');
            
            var expectedBestMove = new Move(2, 0);
            Assert.That(bestMove.Column, Is.EqualTo(expectedBestMove.Column));
            Assert.That(bestMove.Row, Is.EqualTo(expectedBestMove.Row));
        }

        [Test]
        public void DrawnGamePlaysForTheTie()
        {
            throw new NotImplementedException();
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
            var  board = new char[3,3];
            board[0,0] = 'X';
            board[2,0] = 'X';
            board[2,1] = 'O';
            board[2,2] = 'O';

            var ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(board);
            
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
            var  board = new char[3,3];
            board[0,0] = 'O';
            board[2,0] = 'O';
            board[2,1] = 'X';
            board[2,2] = 'X';

            var ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(board);
            
            var bestMove = ticTacToe.GetBestMove(depth, 'O');
            
            var expectedBestMove = new Move(1, 0);
            Assert.That(bestMove.Column, Is.EqualTo(expectedBestMove.Column));
            Assert.That(bestMove.Row, Is.EqualTo(expectedBestMove.Row));
        }
    }
}