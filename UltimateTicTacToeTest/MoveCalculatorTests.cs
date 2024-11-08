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
            TicTacToe ticTacToe = TicTacToeBoardFactory.GetTicTacToeBoard("X-OX-O---");

            MoveCalculator calculator = new MoveCalculator();
            Move bestMove = calculator.GetBestMoveUsingAlphaBeta(ticTacToe, depth, 'O');

            Move expectedBestMove = new Move(2, 2);
            Assert.That(bestMove.Column, Is.EqualTo(expectedBestMove.Column));
            Assert.That(bestMove.Row, Is.EqualTo(expectedBestMove.Row));
        }
        
        [Test]
        public void TicTacToe_OneMoveWinEvaluatesProperly_ForPlayerX_Inverse([Range(1,10)] int depth)
        {
            // |O| |X|
            // |O| |X|
            // | | | |
            TicTacToe ticTacToe = TicTacToeBoardFactory.GetTicTacToeBoard("O-XO-X---");

            MoveCalculator calculator = new MoveCalculator();
            Move bestMove = calculator.GetBestMoveUsingAlphaBeta(ticTacToe, depth, 'X');

            Move expectedBestMove = new Move(2, 2);
            Assert.That(bestMove.Column, Is.EqualTo(expectedBestMove.Column));
            Assert.That(bestMove.Row, Is.EqualTo(expectedBestMove.Row));
        }
        
        [Test]
        public void TicTacToe_OneMoveWinEvaluatesProperly_ForPlayerO_2([Range(1,10)] int depth)
        {
            // |O|X|O|
            // | | |X|
            // |X| |O|
            TicTacToe ticTacToe = TicTacToeBoardFactory.GetTicTacToeBoard("OXO--XX-O");

            MoveCalculator calculator = new MoveCalculator();
            Move bestMove = calculator.GetBestMoveUsingAlphaBeta(ticTacToe, depth, 'O');

            Move expectedBestMove = new Move(1, 1);
            Assert.That(bestMove.Column, Is.EqualTo(expectedBestMove.Column));
            Assert.That(bestMove.Row, Is.EqualTo(expectedBestMove.Row));
        }
        
        [Test]
        public void TicTacToe_OneMoveWinEvaluatesProperlyForPlayerX_1([Range(1,10)] int depth)
        {
            // | |X|O|
            // | |X| |
            // |O| | |
            TicTacToe ticTacToe = TicTacToeBoardFactory.GetTicTacToeBoard("-XO-X-O--");

            MoveCalculator calculator = new MoveCalculator();
            Move bestMove = calculator.GetBestMoveUsingAlphaBeta(ticTacToe, depth, 'X');

            Move expectedBestMove = new Move(1, 2);
            Assert.That(bestMove.Column, Is.EqualTo(expectedBestMove.Column));
            Assert.That(bestMove.Row, Is.EqualTo(expectedBestMove.Row));
        }
        
        [Test]
        public void TicTacToe_OneMoveWinEvaluatesProperlyForPlayerX_2([Range(1,10)] int depth)
        {
            // | | |O|
            // |X|X| |
            // |O| | |
            TicTacToe ticTacToe = TicTacToeBoardFactory.GetTicTacToeBoard("--OXX-O--");

            MoveCalculator calculator = new MoveCalculator();
            Move bestMove = calculator.GetBestMoveUsingAlphaBeta(ticTacToe, depth, 'X');

            Move expectedBestMove = new Move(2, 1);
            Assert.That(bestMove.Column, Is.EqualTo(expectedBestMove.Column));
            Assert.That(bestMove.Row, Is.EqualTo(expectedBestMove.Row));
        }
        
        [Test]
        public void TicTacToe_TwoMoveWinEvaluatesProperlyForPlayer0([Range(3,10)] int depth)  // 3 is the earliest is can see
        {
            // |O| | |
            // |X| | |
            // |O| |X|
            TicTacToe ticTacToe = TicTacToeBoardFactory.GetTicTacToeBoard("O--X--O-X");

            MoveCalculator calculator = new MoveCalculator();
            Move bestMove = calculator.GetBestMoveUsingAlphaBeta(ticTacToe, depth, 'O');


            Move expectedBestMove = new Move(2, 0);
            Assert.That(bestMove.Column, Is.EqualTo(expectedBestMove.Column));
            Assert.That(bestMove.Row, Is.EqualTo(expectedBestMove.Row));
        }
        
        [Test]
        public void TicTacToe_LosingGameDelaysLoss_ForPlayerO([Range(3,10)] int depth)
        {
            // | |X| |
            // | | |X|
            // |O|O|X|
            TicTacToe ticTacToe = TicTacToeBoardFactory.GetTicTacToeBoard("-X---XOOX");

            MoveCalculator calculator = new MoveCalculator();
            Move bestMove = calculator.GetBestMoveUsingAlphaBeta(ticTacToe, depth, 'O');

            Move expectedBestMove = new Move(2, 0);
            Assert.That(bestMove.Column, Is.EqualTo(expectedBestMove.Column));
            Assert.That(bestMove.Row, Is.EqualTo(expectedBestMove.Row));
        }
        
        [Test]
        public void TicTacToe_LosingGameDelaysLoss_ForPlayerX([Range(3,10)] int depth)
        {
            // | |O| |
            // | | |O|
            // |X|X|O|
            TicTacToe ticTacToe = TicTacToeBoardFactory.GetTicTacToeBoard("-O---OXXO");

            MoveCalculator calculator = new MoveCalculator();
            Move bestMove = calculator.GetBestMoveUsingAlphaBeta(ticTacToe, depth, 'X');

            Move expectedBestMove = new Move(2, 0);
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
            TicTacToe ticTacToe = TicTacToeBoardFactory.GetTicTacToeBoard("X-X--O--O");

            MoveCalculator calculator = new MoveCalculator();
            Move bestMove = calculator.GetBestMoveUsingAlphaBeta(ticTacToe, depth, 'O');

            Move expectedBestMove = new Move(1, 0);
            Assert.That(bestMove.Column, Is.EqualTo(expectedBestMove.Column));
            Assert.That(bestMove.Row, Is.EqualTo(expectedBestMove.Row));
        }
        
        [Test]
        public void TicTacToe_WinningGameGoesForFastestWin_ForPlayerO([Range(3,10)] int depth)
        {
            // |O| |O|
            // | | |X|
            // | | |X|
            TicTacToe ticTacToe = TicTacToeBoardFactory.GetTicTacToeBoard("O-O--X--X");

            MoveCalculator calculator = new MoveCalculator();
            Move bestMove = calculator.GetBestMoveUsingAlphaBeta(ticTacToe, depth, 'O');

            Move expectedBestMove = new Move(1, 0);
            Assert.That(bestMove.Column, Is.EqualTo(expectedBestMove.Column));
            Assert.That(bestMove.Row, Is.EqualTo(expectedBestMove.Row));
        }
        
        [Test]
        public void UltimateTicTacToe_OneMoveLossIsAvoidedForX([Range(2,2)] int depth)
        {
            TicTacToe subBoard0 = TicTacToeBoardFactory.GetTicTacToeBoard("X-O--O-OO");
            TicTacToe subBoard1 = TicTacToeBoardFactory.GetTicTacToeBoard("---------");
            TicTacToe subBoard2 = TicTacToeBoardFactory.GetTicTacToeBoard("X--X--X--");
            TicTacToe subBoard3 = TicTacToeBoardFactory.GetTicTacToeBoard("----O---O");
            TicTacToe subBoard4 = TicTacToeBoardFactory.GetTicTacToeBoard("------X--");
            TicTacToe subBoard5 = TicTacToeBoardFactory.GetTicTacToeBoard("X--------");
            TicTacToe subBoard6 = TicTacToeBoardFactory.GetTicTacToeBoard("OOO------");
            TicTacToe subBoard7 = TicTacToeBoardFactory.GetTicTacToeBoard("X--------");
            TicTacToe subBoard8 = TicTacToeBoardFactory.GetTicTacToeBoard("X--X-----");


            MultiTicTacToe ticTacToe = new MultiTicTacToe();
            
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

            MoveCalculator calculator = new MoveCalculator();

            // (3,0), (3,1), (3,2) and (5,0) are all losing moves. Never pick one of them

            Move bestMove = calculator.GetBestMoveUsingAlphaBeta(ticTacToe, depth, 'X');

            List<Move> goodMoves = new List<Move>
            {
                new(4, 0),
                new(4, 1),
                new(4, 2),
                new(5, 1),
                new(5, 2)
            };

            Assert.That(goodMoves.Any(m => m.Column == bestMove.Column && m.Row == bestMove.Row));
        }
        
        [Ignore("")]
        [Test]
        public void UltimateTicTacToe_OneMoveLossIsAvoidedForX_2([Range(2,2)] int depth)
        {
            TicTacToe subBoard0 = TicTacToeBoardFactory.GetTicTacToeBoard("X-O-----O");
            TicTacToe subBoard1 = TicTacToeBoardFactory.GetTicTacToeBoard("----OOX--");
            TicTacToe subBoard2 = TicTacToeBoardFactory.GetTicTacToeBoard("X-----X--");
            TicTacToe subBoard3 = TicTacToeBoardFactory.GetTicTacToeBoard("-O--O----");
            TicTacToe subBoard4 = TicTacToeBoardFactory.GetTicTacToeBoard("-X-XX----");
            TicTacToe subBoard5 = TicTacToeBoardFactory.GetTicTacToeBoard("-X-------");
            TicTacToe subBoard6 = TicTacToeBoardFactory.GetTicTacToeBoard("--O-O----");
            TicTacToe subBoard7 = TicTacToeBoardFactory.GetTicTacToeBoard("---------");
            TicTacToe subBoard8 = TicTacToeBoardFactory.GetTicTacToeBoard("---X-----");

            MultiTicTacToe ticTacToe = new MultiTicTacToe();
            
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

            MoveCalculator calculator = new MoveCalculator();

            //  are all losing moves. Never pick one of them

            List<Tuple<Move, int>> moveScores = calculator.GetMoveScoresUsingAlphaBeta(ticTacToe, depth, 'X');

            Move bestMove = calculator.GetBestMoveUsingAlphaBeta(ticTacToe, depth, 'X');
            
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
            TicTacToe ticTacToe = TicTacToeBoardFactory.GetTicTacToeBoard("O-O--X--X");

            MoveCalculator calculator = new MoveCalculator();
            List<Tuple<Move, int>> bestMoves = calculator.GetMoveScoresUsingAlphaBeta(ticTacToe, 9, 'O');
            
        }

        [Test]
        public void CompareTimes()
        {
            long alphaBetaTime = MeasureAlphaBetaTime(1000);
            long negaTime = MeasureNegaMaxTime(1000);
        }

        private static long MeasureNegaMaxTime(int times)
        {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            // The only non winning move is 1, 2
            // All other moves win in 3 except 1,0 
            // which wins in 1. We expect it to pick that
            // 
            // |O| |O|
            // | | |X|
            // | | |X|
            for (int i = 0; i < times; i++)
            {
                TicTacToe ticTacToe = TicTacToeBoardFactory.GetTicTacToeBoard("O-O--X--X");

                MoveCalculator calculator = new MoveCalculator();
                Move bestMove = calculator.GetBestMove(ticTacToe, 10, 'O');
            }

            watch.Stop();


            long negaMaxTime = watch.ElapsedMilliseconds;

            return negaMaxTime;
        }
        
        private static long MeasureAlphaBetaTime(int times)
        {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            // The only non winning move is 1, 2
            // All other moves win in 3 except 1,0 
            // which wins in 1. We expect it to pick that
            // 
            // |O| |O|
            // | | |X|
            // | | |X|

            for (int i = 0; i < times; i++)
            {
                TicTacToe ticTacToe = TicTacToeBoardFactory.GetTicTacToeBoard("O-O--X--X");

                MoveCalculator calculator = new MoveCalculator();
                Move bestMove = calculator.GetBestMoveUsingAlphaBeta(ticTacToe, 10, 'O');
            }

            watch.Stop();

            long time = watch.ElapsedMilliseconds;

            return time;
        }
    }
}