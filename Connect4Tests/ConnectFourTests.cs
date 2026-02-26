namespace Connect4Tests; 

using Connect4;
using NUnit.Framework;

public class ConnectFourTests
{
    [Test]
    public void TestEvaluate_VerticalWinFor0([Range(1,10)]int depth)
    {
        ConnectFour connect4 = new ConnectFour();

        // Player 0 has 4 in colum 0
        connect4.SetMoveSequence("0101010");

        int result = connect4.Evaluate(playerToMove: 0, depth);

        int expectedResult = (depth + 1) * connect4.WinWeighting;

        Assert.That(result, Is.EqualTo(expectedResult));
    }
    
    [Test]
    public void TestEvaluate_VerticalLossFor0([Range(1,10)]int depth)
    {
        ConnectFour connect4 = new ConnectFour();

        // Player 1 has 4 in column 1
        connect4.SetMoveSequence("01010121");

        int result = connect4.Evaluate(playerToMove: 0, depth);

        int expectedResult = -(depth + 1) * connect4.WinWeighting;

        Assert.That(result, Is.EqualTo(expectedResult));
    }
    
    [Test]
    public void TestEvaluate_VerticalWinFor1([Range(1,10)]int depth)
    {
        ConnectFour connect4 = new ConnectFour();

        // Player 1 has 4 in column 5
        connect4.SetMoveSequence("05150515");

        int result = connect4.Evaluate(playerToMove: 1, depth);

        int expectedResult = (depth + 1) * connect4.WinWeighting;

        Assert.That(result, Is.EqualTo(expectedResult));
    }
    
    [Test]
    public void TestEvaluate_VerticalLossFor1([Range(1,10)]int depth)
    {
        ConnectFour connect4 = new ConnectFour();

        // Player 0 has 4 in column 4
        connect4.SetMoveSequence("43454041");

        int result = connect4.Evaluate(playerToMove: 1, depth);

        int expectedResult = -(depth + 1) * connect4.WinWeighting;

        Assert.That(result, Is.EqualTo(expectedResult));
    }
    
    [Test]
    public void TestEvaluate_HorizontalWinFor0([Range(1,10)]int depth)
    {
        ConnectFour connect4 = new ConnectFour();

        // Player 0 has 4 in row 0
        connect4.SetMoveSequence("3343536");

        int result = connect4.Evaluate(playerToMove: 0, depth);

        int expectedResult = (depth + 1) * connect4.WinWeighting;

        Assert.That(result, Is.EqualTo(expectedResult));
    }
    
    [Test]
    public void TestEvaluate_HorizontalLossFor0([Range(1,10)]int depth)
    {
        ConnectFour connect4 = new ConnectFour();

        // Player 1 has 4 in row 0
        connect4.SetMoveSequence("01020314");

        int result = connect4.Evaluate(playerToMove: 0, depth);

        int expectedResult = -(depth + 1) * connect4.WinWeighting;

        Assert.That(result, Is.EqualTo(expectedResult));
    }
}