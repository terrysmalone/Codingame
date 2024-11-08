namespace Connect4;

// https://www.codingame.com/multiplayer/bot-programming/connect-4
internal sealed class Game
{
    private int _depth = 5;
    
    private int _myId;
    private int _oppId;
    
    private ConnectFour _connectFour;
    private MoveCalculator _calculator;
    
    internal Game(int myId)
    {
        _myId = myId;
        _oppId = myId == 0 ? 1 : 0;
        
        _connectFour = new ConnectFour();
        _calculator = new MoveCalculator();
    }
    
    internal string GetMove()
    {
        int bestMove = _calculator.GetBestMoveUsingAlphaBeta(_connectFour, _depth, _myId);

        _connectFour.AddMove(bestMove, _myId);
        
        //_connectFour.PrintBoard();
        
        return bestMove.ToString();
    }

    internal void AddOpponentMove(int column)
    {
        _connectFour.AddMove(column, _oppId);
    }
    
    public void AddOpponentSteal()
    {
        _connectFour.Steal(_oppId);
    }
}