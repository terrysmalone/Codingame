namespace Connect4;

// https://www.codingame.com/multiplayer/bot-programming/connect-4
internal sealed class Game
{
    private int _depth = 4;

    private int _rows = 7;
    private int _columns = 9;

    private int _myId;
    private int _oppId;
    private MiniMax _miniMax;
    GameState _gameState;

    private int _turn = 0;
    
    internal Game(int myId)
    {
        _myId = myId;
        _oppId = myId == 0 ? 1 : 0;

        _miniMax = new MiniMax();
        _gameState = new GameState(_rows, _columns);
    }
    
    internal string GetMove(int[,] board)
    {
        Logger.StartRoundStopwatch();
        Logger.Board(board);

        if (_turn <= 1)
        {
            if (_myId == 1)
            {
                if (_turn == 0)
                {
                    return "4";
                }
            }
            else
            {
                if (_turn == 1)
                {
                    if (board[6, 4] == 1)
                    {
                        return "STEAL";
                    }
                    else
                    {
                        return "4";
                    }
                }
            }
        }

        _gameState.SetGameState(board, _myId);

        int bestMove = _miniMax.FindBestMove(_gameState, _depth);

        return bestMove.ToString();
    }

    internal void SetTurnIndex(int turnIndex)
    {
        _turn = turnIndex;
    }
}