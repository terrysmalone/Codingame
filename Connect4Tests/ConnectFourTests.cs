namespace Connect4Tests; 

using Connect4;
using NUnit.Framework;

public class ConnectFourTests
{
    [Test]
    public void TestEvaluate_VerticalWinFor0([Range(1,10)]int depth)
    {
        ConnectFour connect4 = new ConnectFour();
        
        connect4.SetMoveSequence("0101010");

        int result = connect4.Evaluate(is0: true, depth);
        
        Assert.That(result, Is.EqualTo(depth + 1));
    }
    
    [Test]
    public void TestEvaluate_VerticalLossFor0([Range(1,10)]int depth)
    {
        ConnectFour connect4 = new ConnectFour();
        
        connect4.SetMoveSequence("0101010");

        int result = connect4.Evaluate(is0: false, depth);
        
        Assert.That(result, Is.EqualTo(-depth - 1));
    }
    
    [Test]
    public void TestEvaluate_VerticalWinFor1([Range(1,10)]int depth)
    {
        ConnectFour connect4 = new ConnectFour();
        
        connect4.SetMoveSequence("05150515");

        int result = connect4.Evaluate(is0: false, depth);
        
        Assert.That(result, Is.EqualTo(depth + 1));
    }
    
    [Test]
    public void TestEvaluate_VerticalLossFor1([Range(1,10)]int depth)
    {
        ConnectFour connect4 = new ConnectFour();
        
        connect4.SetMoveSequence("43454041");

        int result = connect4.Evaluate(is0: false, depth);
        
        Assert.That(result, Is.EqualTo(-depth - 1));
    }
    
    [Test]
    public void TestEvaluate_HorizontalWinFor0([Range(1,10)]int depth)
    {
        ConnectFour connect4 = new ConnectFour();
        
        connect4.AddMove(3, 0);
        connect4.AddMove(3, 1);
        connect4.AddMove(4, 0);
        connect4.AddMove(3, 1);
        connect4.AddMove(5, 0);
        connect4.AddMove(3, 1);
        connect4.AddMove(6, 0);

        int result = connect4.Evaluate(is0: true, depth);
        
        Assert.That(result, Is.EqualTo(depth + 1));
    }
    
    [Test]
    public void TestEvaluate_HorizontalLossFor0([Range(1,10)]int depth)
    {
        ConnectFour connect4 = new ConnectFour();
        
        connect4.AddMove(3, 0);
        connect4.AddMove(3, 1);
        connect4.AddMove(4, 0);
        connect4.AddMove(3, 1);
        connect4.AddMove(5, 0);
        connect4.AddMove(3, 1);
        connect4.AddMove(6, 0);

        int result = connect4.Evaluate(is0: true, depth);
        
        Assert.That(result, Is.EqualTo(depth + 1));
    }

    [Test]
    public void TestEvaluate_BugWin()
    {
        ConnectFour connect4 = new ConnectFour();
        connect4.SetMoveSequence("11230122627252");

        int result = connect4.Evaluate(is0: true);
        
        Assert.That(result, Is.EqualTo(-1));
    }
}