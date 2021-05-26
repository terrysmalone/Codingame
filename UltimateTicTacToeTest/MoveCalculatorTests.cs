using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UltimateTicTacToe;

namespace UltimateTicTacToeTest
{
    [TestFixture]
    public class MoveCalculatorTests
    {
        [Test]
        public void TicTacToe_OneMoveWinEvaluatesProperly_ForPlayerO_1([Range(1,10)] int depth)
        {
            // |X| |O|
            // |X| |O|
            // | | | |
            var ticTacToe = TicTacToeBoardFactory.GetTicTacToeBoard("X-OX-O---");

            var calculator = new MoveCalculator();
            var bestMove = calculator.GetBestMoveUsingAlphaBeta(ticTacToe, depth, 'O');

            var expectedBestMove = new Move(2, 2);
            Assert.That(bestMove.Column, Is.EqualTo(expectedBestMove.Column));
            Assert.That(bestMove.Row, Is.EqualTo(expectedBestMove.Row));
        }
        
        [Test]
        public void TicTacToe_OneMoveWinEvaluatesProperly_ForPlayerX_Inverse([Range(1,10)] int depth)
        {
            // |O| |X|
            // |O| |X|
            // | | | |
            var ticTacToe = TicTacToeBoardFactory.GetTicTacToeBoard("O-XO-X---");

            var calculator = new MoveCalculator();
            var bestMove = calculator.GetBestMoveUsingAlphaBeta(ticTacToe, depth, 'X');

            var expectedBestMove = new Move(2, 2);
            Assert.That(bestMove.Column, Is.EqualTo(expectedBestMove.Column));
            Assert.That(bestMove.Row, Is.EqualTo(expectedBestMove.Row));
        }
        
        [Test]
        public void TicTacToe_OneMoveWinEvaluatesProperly_ForPlayerO_2([Range(1,10)] int depth)
        {
            // |O|X|O|
            // | | |X|
            // |X| |O|
            var ticTacToe = TicTacToeBoardFactory.GetTicTacToeBoard("OXO--XX-O");

            var calculator = new MoveCalculator();
            var bestMove = calculator.GetBestMoveUsingAlphaBeta(ticTacToe, depth, 'O');
            
            var expectedBestMove = new Move(1, 1);
            Assert.That(bestMove.Column, Is.EqualTo(expectedBestMove.Column));
            Assert.That(bestMove.Row, Is.EqualTo(expectedBestMove.Row));
        }
        
        [Test]
        public void TicTacToe_OneMoveWinEvaluatesProperlyForPlayerX_1([Range(1,10)] int depth)
        {
            // | |X|O|
            // | |X| |
            // |O| | |
            var ticTacToe = TicTacToeBoardFactory.GetTicTacToeBoard("-XO-X-O--");

            var calculator = new MoveCalculator();
            var bestMove = calculator.GetBestMoveUsingAlphaBeta(ticTacToe, depth, 'X');
            
            var expectedBestMove = new Move(1, 2);
            Assert.That(bestMove.Column, Is.EqualTo(expectedBestMove.Column));
            Assert.That(bestMove.Row, Is.EqualTo(expectedBestMove.Row));
        }
        
        [Test]
        public void TicTacToe_OneMoveWinEvaluatesProperlyForPlayerX_2([Range(1,10)] int depth)
        {
            // | | |O|
            // |X|X| |
            // |O| | |
            var ticTacToe = TicTacToeBoardFactory.GetTicTacToeBoard("--OXX-O--");

            var calculator = new MoveCalculator();
            var bestMove = calculator.GetBestMoveUsingAlphaBeta(ticTacToe, depth, 'X');
            
            var expectedBestMove = new Move(2, 1);
            Assert.That(bestMove.Column, Is.EqualTo(expectedBestMove.Column));
            Assert.That(bestMove.Row, Is.EqualTo(expectedBestMove.Row));
        }
        
        [Test]
        public void TicTacToe_TwoMoveWinEvaluatesProperlyForPlayer0([Range(3,10)] int depth)  // 3 is the earliest is can see
        {
            // |O| | |
            // |X| | |
            // |O| |X|
            var ticTacToe = TicTacToeBoardFactory.GetTicTacToeBoard("O--X--O-X");

            var calculator = new MoveCalculator();
            var bestMove = calculator.GetBestMoveUsingAlphaBeta(ticTacToe, depth, 'O');
 
            
            var expectedBestMove = new Move(2, 0);
            Assert.That(bestMove.Column, Is.EqualTo(expectedBestMove.Column));
            Assert.That(bestMove.Row, Is.EqualTo(expectedBestMove.Row));
        }
        
        [Test]
        public void TicTacToe_LosingGameDelaysLoss_ForPlayerO([Range(3,10)] int depth)
        {
            // | |X| |
            // | | |X|
            // |O|O|X|
            var ticTacToe = TicTacToeBoardFactory.GetTicTacToeBoard("-X---XOOX");

            var calculator = new MoveCalculator();
            var bestMove = calculator.GetBestMoveUsingAlphaBeta(ticTacToe, depth, 'O');
            
            var expectedBestMove = new Move(2, 0);
            Assert.That(bestMove.Column, Is.EqualTo(expectedBestMove.Column));
            Assert.That(bestMove.Row, Is.EqualTo(expectedBestMove.Row));
        }
        
        [Test]
        public void TicTacToe_LosingGameDelaysLoss_ForPlayerX([Range(3,10)] int depth)
        {
            // | |O| |
            // | | |O|
            // |X|X|O|
            var ticTacToe = TicTacToeBoardFactory.GetTicTacToeBoard("-O---OXXO");

            var calculator = new MoveCalculator();
            var bestMove = calculator.GetBestMoveUsingAlphaBeta(ticTacToe, depth, 'X');
            
            var expectedBestMove = new Move(2, 0);
            Assert.That(bestMove.Column, Is.EqualTo(expectedBestMove.Column));
            Assert.That(bestMove.Row, Is.EqualTo(expectedBestMove.Row));
        }
        
        [Test]
        public void TicTacToe_WinningGameGoesForFastestWin_ForPlayerX([Range(3,10)] int depth)
        {
            // The only non winning move is 1, 2
            // All other moves win in 3 except 1,0 
            // which wins in 1. We expect it to pick that
            // 
            // |X| |X|
            // | | |O|
            // | | |O|
            var ticTacToe = TicTacToeBoardFactory.GetTicTacToeBoard("X-X--O--O");

            var calculator = new MoveCalculator();
            var bestMove = calculator.GetBestMoveUsingAlphaBeta(ticTacToe, depth, 'O');
            
            var expectedBestMove = new Move(1, 0);
            Assert.That(bestMove.Column, Is.EqualTo(expectedBestMove.Column));
            Assert.That(bestMove.Row, Is.EqualTo(expectedBestMove.Row));
        }
        
        [Test]
        public void TicTacToe_WinningGameGoesForFastestWin_ForPlayerO([Range(3,10)] int depth)
        {
            // |O| |O|
            // | | |X|
            // | | |X|
            var ticTacToe = TicTacToeBoardFactory.GetTicTacToeBoard("O-O--X--X");

            var calculator = new MoveCalculator();
            var bestMove = calculator.GetBestMoveUsingAlphaBeta(ticTacToe, depth, 'O');
            
            var expectedBestMove = new Move(1, 0);
            Assert.That(bestMove.Column, Is.EqualTo(expectedBestMove.Column));
            Assert.That(bestMove.Row, Is.EqualTo(expectedBestMove.Row));
        }
        
        [Test]
        public void UltimateTicTacToe_OneMoveLossIsAvoidedForX([Range(2,2)] int depth)
        {
            var subBoard0 = TicTacToeBoardFactory.GetTicTacToeBoard("X-O--O-OO");
            var subBoard1 = TicTacToeBoardFactory.GetTicTacToeBoard("---------");
            var subBoard2 = TicTacToeBoardFactory.GetTicTacToeBoard("X--X--X--");
            var subBoard3 = TicTacToeBoardFactory.GetTicTacToeBoard("----O---O");
            var subBoard4 = TicTacToeBoardFactory.GetTicTacToeBoard("------X--");
            var subBoard5 = TicTacToeBoardFactory.GetTicTacToeBoard("X--------");
            var subBoard6 = TicTacToeBoardFactory.GetTicTacToeBoard("OOO------");
            var subBoard7 = TicTacToeBoardFactory.GetTicTacToeBoard("X--------");
            var subBoard8 = TicTacToeBoardFactory.GetTicTacToeBoard("X--X-----");


            var ticTacToe = new UltimateTicTacToe.UltimateTicTacToe();
            
            ticTacToe.SubBoards[0,0] = subBoard0;
            ticTacToe.SubBoards[1,0] = subBoard1;
            ticTacToe.SubBoards[2,0] = subBoard2;
            ticTacToe.SubBoards[0,1] = subBoard3;
            ticTacToe.SubBoards[1,1] = subBoard4;
            ticTacToe.SubBoards[2,1] = subBoard5;
            ticTacToe.SubBoards[0,2] = subBoard6;
            ticTacToe.SubBoards[1,2] = subBoard7;
            ticTacToe.SubBoards[2,2] = subBoard8;
            
            ticTacToe.AddMove(1, 6, 'O');   //Emulate the last move played to kick off all the updates

            var calculator = new MoveCalculator();
            
            // (3,0), (3,1), (3,2) and (5,0) are all losing moves. Never pick one of them
            
            var bestMove = calculator.GetBestMoveUsingAlphaBeta(ticTacToe, depth, 'X');
            
            var goodMoves = new List<Move>
            {
                new(4, 0),
                new(4, 1),
                new(4, 2),
                new(5, 1),
                new(5, 2)
            };

            Assert.That(goodMoves.Any(m => m.Column == bestMove.Column && m.Row == bestMove.Row));
        }
        
        [Test]
        public void UltimateTicTacToe_OneMoveLossIsAvoidedForX_2([Range(2,2)] int depth)
        {
            var subBoard0 = TicTacToeBoardFactory.GetTicTacToeBoard("X-O-----O");
            var subBoard1 = TicTacToeBoardFactory.GetTicTacToeBoard("----OOX--");
            var subBoard2 = TicTacToeBoardFactory.GetTicTacToeBoard("X-----X--");
            var subBoard3 = TicTacToeBoardFactory.GetTicTacToeBoard("-O--O----");
            var subBoard4 = TicTacToeBoardFactory.GetTicTacToeBoard("-X-XX----");
            var subBoard5 = TicTacToeBoardFactory.GetTicTacToeBoard("-X-------");
            var subBoard6 = TicTacToeBoardFactory.GetTicTacToeBoard("--O-O----");
            var subBoard7 = TicTacToeBoardFactory.GetTicTacToeBoard("---------");
            var subBoard8 = TicTacToeBoardFactory.GetTicTacToeBoard("---X-----");

            var ticTacToe = new UltimateTicTacToe.UltimateTicTacToe();
            
            ticTacToe.SubBoards[0,0] = subBoard0;
            ticTacToe.SubBoards[1,0] = subBoard1;
            ticTacToe.SubBoards[2,0] = subBoard2;
            ticTacToe.SubBoards[0,1] = subBoard3;
            ticTacToe.SubBoards[1,1] = subBoard4;
            ticTacToe.SubBoards[2,1] = subBoard5;
            ticTacToe.SubBoards[0,2] = subBoard6;
            ticTacToe.SubBoards[1,2] = subBoard7;
            ticTacToe.SubBoards[2,2] = subBoard8;
            
            ticTacToe.AddMove(4, 5, 'O');   //Emulate the last move played to kick off all the updates

            var calculator = new MoveCalculator();
            
            //  are all losing moves. Never pick one of them
            
            var moveScores = calculator.GetMoveScoresUsingAlphaBeta(ticTacToe, depth, 'X');
            
            var bestMove = calculator.GetBestMoveUsingAlphaBeta(ticTacToe, depth, 'X');
            
            throw new NotImplementedException();
            // var goodMoves = new List<Move>
            // {
            //     new(4, 0),
            //     new(4, 1),
            //     new(4, 2),
            //     new(5, 1),
            //     new(5, 2)
            // };
            //
            //
            // Assert.That(goodMoves.Any(m => m.Column == bestMove.Column && m.Row == bestMove.Row));
        }

        [Test]
        public void BestMoves()
        {
            // |O| |O|
            // | | |X|
            // | | |X|
            var ticTacToe = TicTacToeBoardFactory.GetTicTacToeBoard("O-O--X--X");

            var calculator = new MoveCalculator();
            var bestMoves = calculator.GetMoveScoresUsingAlphaBeta(ticTacToe, 9, 'O');
            
        }

        [Test]
        public void CompareTimes()
        {
            var alphaBetaTime = MeasureAlphaBetaTime(1000);
            var negaTime = MeasureNegaMaxTime(1000);
        }

        private static long MeasureNegaMaxTime(int times)
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            // The only non winning move is 1, 2
            // All other moves win in 3 except 1,0 
            // which wins in 1. We expect it to pick that
            // 
            // |O| |O|
            // | | |X|
            // | | |X|
            for (var i = 0; i < times; i++)
            {
                var ticTacToe = TicTacToeBoardFactory.GetTicTacToeBoard("O-O--X--X");

                var calculator = new MoveCalculator();
                var bestMove = calculator.GetBestMove(ticTacToe, 10, 'O');
            }

            watch.Stop();
            

            var negaMaxTime = watch.ElapsedMilliseconds;

            return negaMaxTime;
        }
        
        private static long MeasureAlphaBetaTime(int times)
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            // The only non winning move is 1, 2
            // All other moves win in 3 except 1,0 
            // which wins in 1. We expect it to pick that
            // 
            // |O| |O|
            // | | |X|
            // | | |X|

            for (var i = 0; i < times; i++)
            {
                var ticTacToe = TicTacToeBoardFactory.GetTicTacToeBoard("O-O--X--X");

                var calculator = new MoveCalculator();
                var bestMove = calculator.GetBestMoveUsingAlphaBeta(ticTacToe, 10, 'O');
            }

            watch.Stop();

            var time = watch.ElapsedMilliseconds;

            return time;
        }
    }
}