using System;
using System.Collections.Generic;
using System.Drawing;

namespace Connect4;

internal class GameState
{
    private List<Point[]> _windows = new List<Point[]>();

    private int[,] _board;
    public int CurrentPlayer = 1; // 1 for player one, -1 for player two

    private int _columns = 9;
    private int _rows = 7;

    private int _lastMoveColumn = -1;
    private int _lastMoveRow = -1;

    private Stack<(int row, int column, int prevPlayer)> _moveHistory = new Stack<(int row, int column, int prevPlayer)>();

    private const int TERMINAL_SCORE = 10000;
    private const int THREE_OF_FOUR_SCORE = 100;
    private const int TWO_OF_FOUR_SCORE = 30;
    private const int CENTRAL_COLUMNS_SCORE = 3;

    public GameState(int rows = 6, int columns = 7)
    {
        _rows = rows;
        _columns = columns;
        _board = new int[_rows, _columns];

        PreComputeWindows();

        Console.Error.WriteLine($"Windows: {_windows.Count}");
    }

    private void PreComputeWindows()
    {
        // Horizontal windows
        for (int row = 0; row < _rows; row++)
        {
            for (int column = 0; column < _columns - 3; column++)
            {
                _windows.Add(new Point[]
                {
                    new Point(row, column),
                    new Point(row, column + 1),
                    new Point(row, column + 2),
                    new Point(row, column + 3),
                });                
            }
        }

        // Vertical windows
        for (int column = 0; column < _columns; column++)
        {
            for (int row = 0; row < _rows - 3; row++)
            {
                _windows.Add(new Point[]
                {
                    new Point(row, column),
                    new Point(row + 1, column),
                    new Point(row + 2, column),
                    new Point(row + 3, column),
                });

            }
        }
        // Diagonal down-right windows
        for (int row = 0; row < _rows - 3; row++)
        {
            for (int column = 0; column < _columns - 3; column++)
            {
                _windows.Add(new Point[]
                {
                    new Point(row, column),
                    new Point(row + 1, column + 1),
                    new Point(row + 2, column + 2),
                    new Point(row + 3, column + 3),
                });
            }
        }
        // Diagonal up-right windows
        for (int row = 3; row < _rows; row++)
        {
            for (int column = 0; column < _columns - 3; column++)
            {
                _windows.Add(new Point[]
                {
                    new Point(row, column),
                    new Point(row - 1, column + 1),
                    new Point(row - 2, column + 2),
                    new Point(row - 3, column + 3),
                });
            }
        }
    }

    // Set the game state to a specific board and player, used for testing
    public void SetGameState(int[,] board, int currentPlayer)
    {
        _board = board;
        CurrentPlayer = currentPlayer;
    }


    public void SetGameStateFromPlay(List<int> moves)
    {
        _board = new int[_rows, _columns];
        CurrentPlayer = 1;

        foreach (int move in moves)
        {
            ApplyMove(move);
        }
    }

    public void ApplyMove(int move)
    {
        int firstEmpty = 0;

        for (int row = _rows - 1; row >= 0; row--)
        {
            if (_board[row, move] == 0)
            {
                firstEmpty = row;
                break;
            }
        }

        _board[firstEmpty, move] = CurrentPlayer;

        _moveHistory.Push((firstEmpty, move, CurrentPlayer));

        CurrentPlayer = -CurrentPlayer;
    }

    public GameState Clone()
    {
        var newState = new GameState();

        for (int i = 0; i < _board.GetLength(0); i++)
        {
            for (int j = 0; j < _board.GetLength(1); j++)
            {
                newState._board[i, j] = _board[i, j];
            }
        }
        newState.CurrentPlayer = CurrentPlayer;

        return newState;
    }

    public List<int> GetValidMoves()
    {
        var moves = new List<int>();

        for (int col = 0; col < _board.GetLength(1); col++)
        {
            // If the top row of a column is empty a piece can be dropped there
            if (_board[0, col] == 0)
            {
                moves.Add(col);
            }
        }

        return moves;
    }

    public void UndoLastMove()
    {
        if (_moveHistory.Count == 0)
        {
            return;
        }

        (var lastMoveRow, var lastMoveColumn, int prevPlayer) = _moveHistory.Pop();

        _board[lastMoveRow, lastMoveColumn] = 0;
        CurrentPlayer = prevPlayer;
    }

    public bool IsTerminal()
    {
        int winScore = CheckForWinner();

        if (winScore != 0)
        {
            return true;
        }

        // If there are any more moves left it's not terminal
        for (int col = 0; col < _columns; col++)
        {
            if (_board[0, col] == 0)
            {
                return false;
            }
        }

        // There are no free moves and no winner, it's a draw
        return true;
    }

    private int CheckForWinner()
    {
        for (int row = 0; row < _rows; row++)
        {
            for (int column = 0; column < _columns; column++)
            {
                if (_board[row, column] == 0)
                {
                    continue;
                }

                int player = _board[row, column];

                // Horizontal
                if (column <= _columns-4 &&
                    player == _board[row, column + 1] &&
                    player == _board[row, column + 2] &&
                    player == _board[row, column + 3])
                {
                    return player;
                }

                // Vertical
                if (row <= _rows-4 &&
                    player == _board[row + 1, column] &&
                    player == _board[row + 2, column] &&
                    player == _board[row + 3, column])
                {
                    return player;
                }

                // Diagonal down-right
                if (row <= _rows-4 && column <= _columns-4 &&
                    player == _board[row + 1, column + 1] &&
                    player == _board[row + 2, column + 2] &&
                    player == _board[row + 3, column + 3])
                {
                    return player;
                }

                // Diagonal up-right
                if (row >= 3 && column <= _columns - 4 &&
                    player == _board[row - 1, column + 1] &&
                    player == _board[row - 2, column + 2] &&
                    player == _board[row - 3, column + 3])
                {
                    return player;
                }
            }
        }

        return 0;
    }

    public int CalculateScore()
    {
        int score = 0;

        score = CheckForWinner();
        score *= TERMINAL_SCORE;

        score += CentralColumnsScore();

        score += WindowScores();

        return score;
    }

    private int WindowScores()
    {
        int score = 0;

        foreach (Point[] window in _windows)
        {
            score += WindowScore(window);
        }

        return score;
    }

    private int WindowScore(Point[] window)
    {
        int score = 0;

        int player1Count = 0;
        int player2Count = 0;
        int emptyCount = 0;

        foreach (Point point in window)
        {
            switch (_board[point.X, point.Y])
            {
                case 0:
                    emptyCount++;
                    break;
                case 1:
                    player1Count++;
                    break;
                case -1:
                    player2Count++;
                    break;
            } 
        }

        if (player1Count == 3 && emptyCount == 1)
        {
            score += THREE_OF_FOUR_SCORE;
        }
        else if (player1Count == 2 && emptyCount == 2)
        {
            score += TWO_OF_FOUR_SCORE;
        }
        else if (player2Count == 3 && emptyCount == 1)
        {
            score -= THREE_OF_FOUR_SCORE;
        }
        else if (player2Count == 2 && emptyCount == 2)
        {
            score -= TWO_OF_FOUR_SCORE;
        }

        return score;
    }

    private int CentralColumnsScore()
    {
        int score = 0;
        for (int column = 3; column <= 5; column++) 
        {
            for (int row = 0; row < _rows; row++)
            {
                if (_board[row, column] == 1)
                {
                    score += CENTRAL_COLUMNS_SCORE;
                }
                else if (_board[row, column] == -1)
                {
                    score -= CENTRAL_COLUMNS_SCORE;
                }
            }
        }

        return score;
    }
}