using System;
using System.Linq;
using System.Runtime.Serialization;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using UltimateTicTacToe;

namespace UltimateTicTacToeTest
{
    [TestFixture]
    public class UltimateTicTacToeTests
    {
        [Test]
        public void TestEvaluate_OneSubBoardWinFromXPointOfView()
        {
            var ticTacToe = new UltimateTicTacToe.UltimateTicTacToe();
            ticTacToe.AddMove(3, 0, 'X');
            ticTacToe.AddMove(4, 1, 'X');
            ticTacToe.AddMove(5, 2,'X');
            
            Assert.That(ticTacToe.Evaluate(isX:true, depth:4), Is.EqualTo(5));
        }
        
        [Test]
        public void TestEvaluate_OneSubBoardLossFromXPointOfView()
        {
            var ticTacToe = new UltimateTicTacToe.UltimateTicTacToe();
            ticTacToe.AddMove(8, 8, 'O');
            ticTacToe.AddMove(7, 7, 'O');
            ticTacToe.AddMove(6, 6,'O');
            
            Assert.That(ticTacToe.Evaluate(isX:true, depth:7), Is.EqualTo(-8));
        }
        
        [Test]
        public void TestEvaluate_MoreSubBoardWinFromXPointOfView()
        {
            var ticTacToe = new UltimateTicTacToe.UltimateTicTacToe();
            // X won sub board 0
            ticTacToe.AddMove(0, 0, 'X');
            ticTacToe.AddMove(0, 1, 'X');
            ticTacToe.AddMove(0, 2, 'X');
            // O won sub board 3
            ticTacToe.AddMove(6, 0, 'O');
            ticTacToe.AddMove(7, 0, 'O');
            ticTacToe.AddMove(8, 0, 'O');
            // X won sub board 7
            ticTacToe.AddMove(3, 6, 'X');
            ticTacToe.AddMove(4, 7, 'X');
            ticTacToe.AddMove(5, 8, 'X');

            Assert.That(ticTacToe.Evaluate(isX:true, depth:8), Is.EqualTo(9));
        }
        
        [Test]
        public void TestEvaluate_MoreSubBoardLossesFromXPointOfView()
        {
            var ticTacToe = new UltimateTicTacToe.UltimateTicTacToe();
            // X won sub board 0
            ticTacToe.AddMove(0, 0, 'O');
            ticTacToe.AddMove(1, 1, 'O');
            ticTacToe.AddMove(2, 2, 'O');
            // O won sub board 3
            ticTacToe.AddMove(6, 0, 'O');
            ticTacToe.AddMove(7, 0, 'O');
            ticTacToe.AddMove(8, 0, 'O');
            // X won sub board 8
            ticTacToe.AddMove(6, 6, 'X');
            ticTacToe.AddMove(7, 7, 'X');
            ticTacToe.AddMove(8, 8, 'X');

            Assert.That(ticTacToe.Evaluate(isX:true, depth:5), Is.EqualTo(-6));
        }
        
        [Test]
        public void TestEvaluate_OverallWinFromXPointOfView()
        {
            var ticTacToe = new UltimateTicTacToe.UltimateTicTacToe();
            // X won sub board 0
            ticTacToe.AddMove(0, 0, 'X');
            ticTacToe.AddMove(0, 1, 'X');
            ticTacToe.AddMove(0, 2, 'X');
            // X won sub board 3
            ticTacToe.AddMove(0, 3, 'X');
            ticTacToe.AddMove(0, 4, 'X');
            ticTacToe.AddMove(0, 5, 'X');
            // X won sub board 6
            ticTacToe.AddMove(0, 6, 'X');
            ticTacToe.AddMove(0, 7, 'X');
            ticTacToe.AddMove(0, 8, 'X');

            Assert.That(ticTacToe.Evaluate(isX:true, depth:8), Is.EqualTo(117)); // 9 + 9 + 9 + 9*10
        }
        
        [Test]
        public void TestEvaluate_OverallLossFromXPointOfView()
        {
            var ticTacToe = new UltimateTicTacToe.UltimateTicTacToe();
            // X won sub board 0
            ticTacToe.AddMove(0, 0, 'O');
            ticTacToe.AddMove(0, 1, 'O');
            ticTacToe.AddMove(0, 2, 'O');
            // X won sub board 3
            ticTacToe.AddMove(0, 3, 'O');
            ticTacToe.AddMove(0, 4, 'O');
            ticTacToe.AddMove(0, 5, 'O');
            // X won sub board 6
            ticTacToe.AddMove(0, 6, 'O');
            ticTacToe.AddMove(0, 7, 'O');
            ticTacToe.AddMove(0, 8, 'O');
            // X won sub board 8
            ticTacToe.AddMove(6, 6, 'X');
            ticTacToe.AddMove(7, 7, 'X');
            ticTacToe.AddMove(8, 8, 'X');
            
            Assert.That(ticTacToe.Evaluate(isX:true, depth:5), Is.EqualTo(-72)); // 6 + 6 + 6 - 6 + 6*10
        }
        
        [Test]
        public void TestEvaluate_OneSubBoardWinFromOPointOfView()
        {
            var ticTacToe = new UltimateTicTacToe.UltimateTicTacToe();
            ticTacToe.AddMove(6, 0, 'O');
            ticTacToe.AddMove(6, 1, 'O');
            ticTacToe.AddMove(6, 2, 'O');
            
            Assert.That(ticTacToe.Evaluate(isX:false, depth:16), Is.EqualTo(17));
        }
        
        [Test]
        public void TestEvaluate_OneSubBoardLossFromOPointOfView()
        {
            var ticTacToe = new UltimateTicTacToe.UltimateTicTacToe();
            ticTacToe.AddMove(3, 0, 'X');
            ticTacToe.AddMove(4, 0, 'X');
            ticTacToe.AddMove(5, 0, 'X');
            
            Assert.That(ticTacToe.Evaluate(isX:false, depth:6), Is.EqualTo(-7));
        }
        
        [Test]
        public void TestEvaluate_MoreSubBoardWinFromOPointOfView()
        {
            var ticTacToe = new UltimateTicTacToe.UltimateTicTacToe();
            ticTacToe.AddMove(3, 0, 'X');
            ticTacToe.AddMove(4, 0, 'X');
            ticTacToe.AddMove(5, 0, 'X');
            
            Assert.That(ticTacToe.Evaluate(isX:false, depth:6), Is.EqualTo(-7));
        }
        
        [Test]
        public void TestEvaluate_MoreSubBoardLossFromOPointOfView()
        {
            var ticTacToe = new UltimateTicTacToe.UltimateTicTacToe();
            // (X) won sub board 0
            ticTacToe.AddMove(0, 0, 'O');
            ticTacToe.AddMove(0, 1, 'O');
            ticTacToe.AddMove(0, 2, 'O');
            // X won sub board 3
            ticTacToe.AddMove(0, 3, 'X');
            ticTacToe.AddMove(0, 4, 'X');
            ticTacToe.AddMove(0, 5, 'X');
            // X won sub board 6
            ticTacToe.AddMove(0, 6, 'X');
            ticTacToe.AddMove(0, 7, 'X');
            ticTacToe.AddMove(0, 8, 'X');
            
            // X won sub board 8
            ticTacToe.AddMove(6, 6, 'X');
            ticTacToe.AddMove(7, 7, 'X');
            ticTacToe.AddMove(8, 8, 'X');
            
            Assert.That(ticTacToe.Evaluate(isX:false, depth:3), Is.EqualTo(-8));  // 4 + 4 + 4 - 4
        }
        
        [Test]
        public void TestEvaluate_OverallWinFromOPointOfView()
        {
            var ticTacToe = new UltimateTicTacToe.UltimateTicTacToe();
            // O won sub board 0
            ticTacToe.AddMove(0, 0, 'O');
            ticTacToe.AddMove(0, 1, 'O');
            ticTacToe.AddMove(0, 2, 'O');
            // O won sub board 3
            ticTacToe.AddMove(0, 3, 'O');
            ticTacToe.AddMove(0, 4, 'O');
            ticTacToe.AddMove(0, 5, 'O');
            // O won sub board 6
            ticTacToe.AddMove(0, 6, 'O');
            ticTacToe.AddMove(0, 7, 'O');
            ticTacToe.AddMove(0, 8, 'O');

            Assert.That(ticTacToe.Evaluate(isX:false, depth:3), Is.EqualTo(52)); // 4 + 4 + 4 + 4*10
        }
        
        [Test]
        public void TestEvaluate_OverallLossFromOPointOfView()
        {
            var ticTacToe = new UltimateTicTacToe.UltimateTicTacToe();
            // O won sub board 0
            ticTacToe.AddMove(0, 0, 'X');
            ticTacToe.AddMove(0, 1, 'X');
            ticTacToe.AddMove(0, 2, 'X');
            // O won sub board 3
            ticTacToe.AddMove(0, 3, 'X');
            ticTacToe.AddMove(0, 4, 'X');
            ticTacToe.AddMove(0, 5, 'X');
            // O won sub board 6
            ticTacToe.AddMove(0, 6, 'X');
            ticTacToe.AddMove(0, 7, 'X');
            ticTacToe.AddMove(0, 8, 'X');

            Assert.That(ticTacToe.Evaluate(isX:false, depth:3), Is.EqualTo(-52)); // 4 + 4 + 4 + 4*10
        }
        
        [Test]
        public void TestAddMove()
        {
            var ticTacToe = new UltimateTicTacToe.UltimateTicTacToe();
            // O won sub board 0
            ticTacToe.AddMove(0, 3, 'X');
            
            var board = ticTacToe.SubBoards[0,1].GetBoard();
            Assert.That(CountEmptySpacesOnBoard(ticTacToe.SubBoards[0,1].GetBoard()), Is.EqualTo(8));
            
            Assert.That(board[0,0], Is.EqualTo('X'));

            Assert.That(CountEmptySpacesOnBoard(ticTacToe.SubBoards[0,0].GetBoard()), Is.EqualTo(9));
            Assert.That(CountEmptySpacesOnBoard(ticTacToe.SubBoards[0,2].GetBoard()), Is.EqualTo(9));
            Assert.That(CountEmptySpacesOnBoard(ticTacToe.SubBoards[1,0].GetBoard()), Is.EqualTo(9));
            Assert.That(CountEmptySpacesOnBoard(ticTacToe.SubBoards[1,1].GetBoard()), Is.EqualTo(9));
            Assert.That(CountEmptySpacesOnBoard(ticTacToe.SubBoards[1,2].GetBoard()), Is.EqualTo(9));
            Assert.That(CountEmptySpacesOnBoard(ticTacToe.SubBoards[2,0].GetBoard()), Is.EqualTo(9));
            Assert.That(CountEmptySpacesOnBoard(ticTacToe.SubBoards[2,1].GetBoard()), Is.EqualTo(9));
            Assert.That(CountEmptySpacesOnBoard(ticTacToe.SubBoards[2,2].GetBoard()), Is.EqualTo(9));
        }

        [Test]
        public void TestUndoMethod()
        {
            throw new NotImplementedException();
        }
        
        [Test]
        public void TestAddingAndUndoing()
        {
            var ticTacToe = new UltimateTicTacToe.UltimateTicTacToe();
            
            var rand = new Random();
            
            for (var i = 0; i < 1000000; i++)
            {
                var xAdd =rand.Next(9);
                var yAdd =rand.Next(9);
                
                var piece = rand.Next(2) == 1 ? 'X' : 'O';
                
                
                ticTacToe.AddMove(xAdd, yAdd, piece);
                ticTacToe.UndoMove(xAdd, yAdd);
            }

            Assert.That(CountEmptySpacesOnBoard(ticTacToe.SubBoards[0,0].GetBoard()), Is.EqualTo(9));
            Assert.That(CountEmptySpacesOnBoard(ticTacToe.SubBoards[0,1].GetBoard()), Is.EqualTo(9));
            Assert.That(CountEmptySpacesOnBoard(ticTacToe.SubBoards[0,2].GetBoard()), Is.EqualTo(9));
            Assert.That(CountEmptySpacesOnBoard(ticTacToe.SubBoards[1,0].GetBoard()), Is.EqualTo(9));
            Assert.That(CountEmptySpacesOnBoard(ticTacToe.SubBoards[1,1].GetBoard()), Is.EqualTo(9));
            Assert.That(CountEmptySpacesOnBoard(ticTacToe.SubBoards[1,2].GetBoard()), Is.EqualTo(9));
            Assert.That(CountEmptySpacesOnBoard(ticTacToe.SubBoards[2,0].GetBoard()), Is.EqualTo(9));
            Assert.That(CountEmptySpacesOnBoard(ticTacToe.SubBoards[2,1].GetBoard()), Is.EqualTo(9));
            Assert.That(CountEmptySpacesOnBoard(ticTacToe.SubBoards[2,2].GetBoard()), Is.EqualTo(9));
        }
        
        private static int CountEmptySpacesOnBoard(char[,] board)
        {
            return board.Cast<char>().Count(c => c == '\0');
        }
    }
}