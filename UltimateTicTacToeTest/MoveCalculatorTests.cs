using NUnit.Framework;
using UltimateTicTacToe;

namespace UltimateTicTacToeTest
{
    [TestFixture]
    public class MoveCalculatorTests
    {
        [Test]
        public void OneMoveWinEvaluatesProperly_ForPlayerO_1([Range(1,10)] int depth)
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
        public void OneMoveWinEvaluatesProperly_ForPlayerX_Inverse([Range(1,10)] int depth)
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
        public void OneMoveWinEvaluatesProperly_ForPlayerO_2([Range(1,10)] int depth)
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
        public void OneMoveWinEvaluatesProperlyForPlayerX_1([Range(1,10)] int depth)
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
        public void OneMoveWinEvaluatesProperlyForPlayerX_2([Range(1,10)] int depth)
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
        public void TwoMoveWinEvaluatesProperlyForPlayer0([Range(3,10)] int depth)  // 3 is the earliest is can see
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
        public void LosingGameDelaysLoss_ForPlayerO([Range(3,10)] int depth)
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
        public void LosingGameDelaysLoss_ForPlayerX([Range(3,10)] int depth)
        {
            // | |O| |
            // | | |O|
            // |X|X|O|
            var ticTacToe = TicTacToeBoardFactory.GetTicTacToeBoard("-O---OXXO");

            var calculator = new MoveCalculator();
            var bestMove = calculator.GetBestMoveUsingAlphaBeta(ticTacToe, depth, 'O');
            
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
            var ticTacToe = TicTacToeBoardFactory.GetTicTacToeBoard("X-X--O--O");

            var calculator = new MoveCalculator();
            var bestMove = calculator.GetBestMoveUsingAlphaBeta(ticTacToe, depth, 'O');
            
            var expectedBestMove = new Move(1, 0);
            Assert.That(bestMove.Column, Is.EqualTo(expectedBestMove.Column));
            Assert.That(bestMove.Row, Is.EqualTo(expectedBestMove.Row));
        }
        
        [Test]
        public void WinningGameGoesForFastestWin_ForPlayerO([Range(3,10)] int depth)
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

        private long MeasureNegaMaxTime(int times)
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
        
        private long MeasureAlphaBetaTime(int times)
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