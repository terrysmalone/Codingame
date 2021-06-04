using Connect4;
using NUnit.Framework;

namespace Connect4Tests
{
    public class ConnectFourTests
    {
        [Test]
        public void TestEvaluate_UpWinFor0([Range(1,10)]int depth)
        {
            var connect4 = new ConnectFour();
            
            connect4.AddMove(0, 0);
            connect4.AddMove(1, 1);
            connect4.AddMove(0, 0);
            connect4.AddMove(1, 1);
            connect4.AddMove(0, 0);
            connect4.AddMove(1, 1);
            connect4.AddMove(0, 0);
            
            var result = connect4.Evaluate(is0: true, depth);
            
            Assert.That(result, Is.EqualTo(depth + 1));
        }
        
        [Test]
        public void TestEvaluate_UpLossFor0([Range(1,10)]int depth)
        {
            var connect4 = new ConnectFour();
            
            connect4.AddMove(0, 0);
            connect4.AddMove(1, 1);
            connect4.AddMove(0, 0);
            connect4.AddMove(1, 1);
            connect4.AddMove(0, 0);
            connect4.AddMove(1, 1);
            connect4.AddMove(0, 0);
            
            var result = connect4.Evaluate(is0: false, depth);
            
            Assert.That(result, Is.EqualTo(-depth - 1));
        }
        
        [Test]
        public void TestEvaluate_UpWinFor1([Range(1,10)]int depth)
        {
            var connect4 = new ConnectFour();
            
            connect4.AddMove(0, 1);
            connect4.AddMove(1, 1);
            connect4.AddMove(0, 1);
            connect4.AddMove(1, 1);
            connect4.AddMove(0, 1);
            connect4.AddMove(1, 1);
            connect4.AddMove(0, 1);

            var result = connect4.Evaluate(is0: false, depth);
            
            Assert.That(result, Is.EqualTo(depth + 1));
        }
        
        [Test]
        public void TestEvaluate_UpLossFor1([Range(1,10)]int depth)
        {
            var connect4 = new ConnectFour();
            
            connect4.AddMove(0, 1);
            connect4.AddMove(1, 1);
            connect4.AddMove(0, 1);
            connect4.AddMove(1, 1);
            connect4.AddMove(0, 1);
            connect4.AddMove(1, 1);
            connect4.AddMove(0, 1);

            var result = connect4.Evaluate(is0: true, depth);
            
            Assert.That(result, Is.EqualTo(-depth - 1));
        }
    }
}