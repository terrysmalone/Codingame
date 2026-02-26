namespace Connect4Tests; 

using Connect4;
using NUnit.Framework;
using System.Linq;

[TestFixture]
public class MoveCalculatorTests
{
    [Test]
    public void TestEvaluate_Bug([Range(2,10)]int depth)
    {
        ConnectFour connect4 = new ConnectFour();

        // Player 1 can get a win on the next move by playing in column 2. We should block that
        // In the game my bot played 5 next, letting the opponent then win with 2
        connect4.SetMoveSequence("112301226272");
         
        MoveCalculator moveCalculator = new MoveCalculator();
        System.Collections.Generic.List<System.Tuple<int, int>> results = moveCalculator.GetMoveScoresUsingAlphaBeta(connect4, depth, 0);

        int bestMove = results.Where(r => r.Item2 == results.Max(r => r.Item2)).Max(r => r.Item1);

        Assert.That(bestMove, Is.EqualTo(2));
    }

    [Test]
    public void TestEvaluate_Bug_FromPlayer1sPerspective([Range(2, 10)] int depth)
    {
        ConnectFour connect4 = new ConnectFour();

        // Player 1 can get a win on the next move by playing in column 2.
        connect4.SetMoveSequence("112301226272");

        MoveCalculator moveCalculator = new MoveCalculator();
        System.Collections.Generic.List<System.Tuple<int, int>> results = moveCalculator.GetMoveScoresUsingAlphaBeta(connect4, depth, 1);

        int bestMove = results.Where(r => r.Item2 == results.Max(r => r.Item2)).Max(r => r.Item1);

        Assert.That(bestMove, Is.EqualTo(2));
    }
}