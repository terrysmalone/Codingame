namespace Connect4Tests; 

using Connect4;
using NUnit.Framework;

[TestFixture]
public class MoveCalculatorTests
{
    [Test]
    public void TestEvaluate_Bug([Range(2,10)]int depth)
    {
        ConnectFour connect4 = new ConnectFour();
        connect4.SetMoveSequence("112301226272");
        // In the game my bot played 5 next, letting the opponent then win with 2

        MoveCalculator moveCalculator = new MoveCalculator();
        System.Collections.Generic.List<System.Tuple<int, int>> results = moveCalculator.GetMoveScoresUsingAlphaBeta(connect4, depth, 0);
        
        //Assert.That(result, Is.EqualTo(depth + 1));
    }
}