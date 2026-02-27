namespace Connect4Tests; 

using Connect4;
using NUnit.Framework;

public class ConnectFourTests
{
    [Test]
    public void TestEvaluate_VerticalWinFor0([Range(1,10)]int ply)
    {
        ConnectFour connect4 = new ConnectFour();

        // Player 0 has 4 in colum 0
        connect4.SetMoveSequence("0101010");

        int result = connect4.Evaluate(playerToMove: 0, ply);

        int expectedResult = connect4.WinWeighting - ply;

        Assert.That(result, Is.EqualTo(expectedResult));
    }
    
    [Test]
    public void TestEvaluate_VerticalLossFor0([Range(1,10)]int ply)
    {
        ConnectFour connect4 = new ConnectFour();

        // Player 1 has 4 in column 1
        connect4.SetMoveSequence("01010121");

        int result = connect4.Evaluate(playerToMove: 0, ply);

        int expectedResult = -(connect4.WinWeighting - ply);

        Assert.That(result, Is.EqualTo(expectedResult));
    }
    
    [Test]
    public void TestEvaluate_VerticalWinFor1([Range(1,10)]int ply)
    {
        ConnectFour connect4 = new ConnectFour();

        // Player 1 has 4 in column 5
        connect4.SetMoveSequence("05150515");

        int result = connect4.Evaluate(playerToMove: 1, ply);

        int expectedResult = connect4.WinWeighting - ply;

        Assert.That(result, Is.EqualTo(expectedResult));
    }
    
    [Test]
    public void TestEvaluate_VerticalLossFor1([Range(1,10)]int ply)
    {
        ConnectFour connect4 = new ConnectFour();

        // Player 0 has 4 in column 4
        connect4.SetMoveSequence("43454041");

        int result = connect4.Evaluate(playerToMove: 1, ply);

        int expectedResult = -(connect4.WinWeighting - ply);

        Assert.That(result, Is.EqualTo(expectedResult));
    }
    
    [Test]
    public void TestEvaluate_HorizontalWinFor0([Range(1,10)]int ply)
    {
        ConnectFour connect4 = new ConnectFour();

        // Player 0 has 4 in row 0
        connect4.SetMoveSequence("3343536");

        int result = connect4.Evaluate(playerToMove: 0, ply);

        int expectedResult = connect4.WinWeighting - ply;

        Assert.That(result, Is.EqualTo(expectedResult));
    }
    
    [Test]
    public void TestEvaluate_HorizontalLossFor0([Range(1,10)]int ply)
    {
        ConnectFour connect4 = new ConnectFour();

        // Player 1 has 4 in row 0
        connect4.SetMoveSequence("01020314");

        int result = connect4.Evaluate(playerToMove: 0, ply);

        int expectedResult = -(connect4.WinWeighting - ply);

        Assert.That(result, Is.EqualTo(expectedResult));
    }

    [Test]
    public void TestEvaluate_SoonerWinScoresHigher()
    {
        ConnectFour connect4 = new ConnectFour();

        // Win for player 0
        connect4.SetMoveSequence("0101010");

        // Evaluate same winning board at different plys
        int deeper = connect4.Evaluate(playerToMove: 0, ply: 5);
        int sooner = connect4.Evaluate(playerToMove: 0, ply: 1);

        // A sooner win should be MORE valuable
        Assert.That(sooner, Is.GreaterThan(deeper), "Wins closer to the root should score higher than deeper wins");
    }
}