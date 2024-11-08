using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UltimateTicTacToe;

namespace UltimateTicTacToeTest
{
    [TestFixture]
    public class TicTacToeTests
    {
        [Test]
        public void GetNumberOfPiecesScore_EmptyBoard()
        {
            TicTacToe ticTacToe = new TicTacToe();

            Assert.That(ticTacToe.GetNumberOfPiecesScore('X'), Is.EqualTo(0));
            Assert.That(ticTacToe.GetNumberOfPiecesScore('O'), Is.EqualTo(0));
        }
        
        [Test]
        public void GetNumberOfPiecesScore_EvenGame()
        {
            // |O| |O|
            // | | |X|
            // | | |X|
            TicTacToe ticTacToe = new TicTacToe();
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
            TicTacToe ticTacToe = new TicTacToe();
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
            TicTacToe ticTacToe = new TicTacToe();
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
            TicTacToe ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(SetBoard("-X---XOOO"));
            
            Assert.That(ticTacToe.IsGameOver());
        }
        
        [Test]
        public void IsGameOVer_IfXWins()
        {
            // |X| | |
            // | |X| |
            // |O|O|X|
            TicTacToe ticTacToe = new TicTacToe();
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
            TicTacToe ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(SetBoard(boardPieces));
            
            Assert.That(ticTacToe.IsGameOver());
        }
        
        [Test]
        public void IsGameOver_IfWonBoardIsFull()
        {
            // |X|X|O|
            // |X|O|O|
            // |X|O|X|
            TicTacToe ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(SetBoard("XXOXOOXOX"));
            
            Assert.That(ticTacToe.IsGameOver());
        }
        
        [Test]
        public void IsGameOver_IfNonWonBoardIsFull()
        {
            // |X|O|O|
            // |O|X|X|
            // |X|O|O|
            TicTacToe ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(SetBoard("XOOOXXXOO"));
            
            Assert.That(ticTacToe.IsGameOver());
        }
        
        [Test]
        public void IsGameOver_FalseIfEmpty()
        {
            char[,] board = new char[3,3];

            TicTacToe ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(board);
            
            Assert.That(ticTacToe.IsGameOver(), Is.False);
        }
        
        [Test]
        public void IsGameOver_FalseForPartiallyPlayedBoard()
        {
            // | | |O|
            // | | | |
            // |X|X| |
            TicTacToe ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(SetBoard("--O---XX-"));
            
            Assert.That(ticTacToe.IsGameOver(), Is.False);
        }
        
        [Test]
        public void TestCalculateValidMovesOnEmptyBoard()
        {
            TicTacToe ticTacToe = new TicTacToe();

            List<Move> moves = ticTacToe.CalculateValidMoves();

            List<Move> expectedMoves = new List<Move>
            {
                new(0, 0),
                new(0, 1),
                new(0, 2),
                new(1, 0),
                new(1, 1),
                new(1, 2),
                new(2, 0),
                new(2, 1),
                new(2, 2)
            };
            
            CollectionAssert.AreEquivalent(expectedMoves, moves);
        }
        
        [Test]
        public void TestAddMoveO()
        {
            TicTacToe ticTacToe = new TicTacToe();
            
            ticTacToe.AddMove(0, 2, 'O');

            char[,] board = ticTacToe.GetBoard();
            char[,] expectedBoard = SetBoard("------O--");


            CollectionAssert.AreEqual(expectedBoard, board);
        }
        
        [Test]
        public void TestAddMoveX()
        {
            TicTacToe ticTacToe = new TicTacToe();
            
            ticTacToe.AddMove(1, 1, 'X');

            char[,] board = ticTacToe.GetBoard();
            char[,] expectedBoard = SetBoard("----X----");

            CollectionAssert.AreEqual(expectedBoard, board);
        }
        
        [Test]
        public void TestAddMoveFull()
        {
            TicTacToe ticTacToe = new TicTacToe();
            
            ticTacToe.AddMove(0, 0, 'O');
            ticTacToe.AddMove(0, 1, 'X');
            ticTacToe.AddMove(0, 2, 'O');
            ticTacToe.AddMove(1, 0, 'X');
            ticTacToe.AddMove(1, 1, 'O');
            ticTacToe.AddMove(1, 2, 'X');
            ticTacToe.AddMove(2, 0, 'O');
            ticTacToe.AddMove(2, 1, 'X');
            ticTacToe.AddMove(2, 2, 'O');

            char[,] board = ticTacToe.GetBoard();
            char[,] expectedBoard = SetBoard("OXOXOXOXO");

            CollectionAssert.AreEqual(expectedBoard, board);
        }
        
        [Test]
        public void TestUndoMoveO()
        {
            TicTacToe ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(SetBoard("X--O--XO-"));
            
            ticTacToe.UndoMove(1, 2);

            char[,] board = ticTacToe.GetBoard();

            char[,] expectedBoard = SetBoard("X--O--X--");

            CollectionAssert.AreEqual(expectedBoard, board);
        }
        
        [Test]
        public void TestUndoMoveX()
        {
            TicTacToe ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(SetBoard("XOOOX-X--"));
            
            ticTacToe.UndoMove(0, 2);

            char[,] board = ticTacToe.GetBoard();

            char[,] expectedBoard = SetBoard("XOOOX----");

            CollectionAssert.AreEqual(expectedBoard, board);
        }
        
        [Test]
        public void FullBoard()
        {
            TicTacToe ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(SetBoard("OXOXOXOXO"));
            
            ticTacToe.UndoMove(0, 0);
            ticTacToe.UndoMove(0, 1);
            ticTacToe.UndoMove(0, 2);
            ticTacToe.UndoMove(1, 0);
            ticTacToe.UndoMove(1, 1);
            ticTacToe.UndoMove(1, 2);
            ticTacToe.UndoMove(2, 0);
            ticTacToe.UndoMove(2, 1);
            ticTacToe.UndoMove(2, 2);

            char[,] board = ticTacToe.GetBoard();

            char[,] expectedBoard = SetBoard("---------");

            CollectionAssert.AreEqual(expectedBoard, board);
        }

        [Test]
        public void TestCalculateValidMoves()
        {
            // | |O|X|
            // | | | |
            // |O|X| |
            TicTacToe ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(SetBoard("-OX---OX-"));

            List<Move> moves = ticTacToe.CalculateValidMoves();

            List<Move> expectedMoves = new List<Move>
            {
                new(0, 0),
                new(0, 1),
                new(1, 1),
                new(2, 1),
                new(2, 2)
            };
            
            CollectionAssert.AreEquivalent(expectedMoves, moves);
        }

        [Test]
        public void TestEvaluateWinningFromOPointOfView()
        {
            // | |O|X|
            // |X|O| |
            // |O|O|X|
            TicTacToe ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(SetBoard("-OXXO-OOX"));
            
            Assert.That(ticTacToe.Evaluate(isX:false, currentDepth:5), Is.EqualTo(6));    // Score should be equal to 1 + depth
        }
        
        [Test]
        public void TestEvaluateLosingFromOPointOfView()
        {
            // | |O|X|
            // |X|O|O|
            // |X|X|X|
            TicTacToe ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(SetBoard("-OXXOOXXX"));
            
            Assert.That(ticTacToe.Evaluate(isX:false, currentDepth:0), Is.EqualTo(-1));    // Score should be equal to 1 + depth
        }
        
        [Test]
        public void TestEvaluateDrawingFromOPointOfView()
        {
            // |X|O|O|
            // |X|X|O|
            // | |O| |
            TicTacToe ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(SetBoard("XOOXXO-O-"));
            
            Assert.That(ticTacToe.Evaluate(isX:false, currentDepth:3), Is.EqualTo(0)); 
        }
        
        [Test]
        public void TestEvaluateWinningFromXPointOfView()
        {
            // |X|O|O|
            // |X|X| |
            // |O|O|X|
            TicTacToe ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(SetBoard("XOOXX-OOX"));
            
            Assert.That(ticTacToe.Evaluate(isX:true, currentDepth:9), Is.EqualTo(10));    // Score should be equal to 1 + depth
        }
        
        [Test]
        public void TestEvaluateLosingFromXPointOfView()
        {
            // |X|O|O|
            // |X|O|O|
            // |O|X|X|
            TicTacToe ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(SetBoard("XOOXOOOXX"));
            
            Assert.That(ticTacToe.Evaluate(isX:true, currentDepth:3), Is.EqualTo(-4));    // Score should be equal to  - 1 - depth
        }
        
        [Test]
        public void TestEvaluateDrawingFromXPointOfView()
        {
            // |X|O|O|
            // |X|X| |
            // |O| |O|
            TicTacToe ticTacToe = new TicTacToe();
            ticTacToe.SetBoard(SetBoard("XOOXX-O-O"));
            
            Assert.That(ticTacToe.Evaluate(isX:true, currentDepth:453), Is.EqualTo(0));   
        }
        
        // 
        // |0|1|2|
        // |3|4|5|
        // |6|7|8|
        // "-" for empty
        private static char[,] SetBoard(string boardString)
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