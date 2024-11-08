namespace Connect4; 

using System;
using System.Collections.Generic;

internal sealed class ConnectFour
{
    private List<int>[] _board;
    
    private const int _boardMax = 6;
    
    internal ConnectFour()
    {
        ClearBoard();
    }
    
    private void ClearBoard()
    {
        _board = new List<int>[9];

        for (int i = 0; i < _board.Length; i++)
        {
            _board[i] = new List<int>();
        }
    }
    
    public List<int> CalculateValidMoves()
    {
        List<int> validMoves = new List<int>();

        for (int i = 0; i < _board.Length; i++)
        {
            if(_board[i].Count <= _boardMax)
            {
                validMoves.Add(i);
            }
        }
        
        // Also add steal
        
        return validMoves;
    }
    
    internal void AddMove(int column, int playerId)
    {
        _board[column].Add(playerId);
    }
    
    public void UndoMove(int column)
    {
        _board[column].RemoveAt(_board[column].Count-1);
    }
    
    public void Steal(int playerId)
    {
        int column = -1;
        
        for (int i = 0; i < _board.Length; i++)
        {
            if(_board[i].Count > 0)
            {
                column = i;
                break;
            }
        }
        
        UndoMove(column);
        AddMove(column, playerId);
    }
    
    public int Evaluate(bool is0, int depth = 0)
    {
        const int winWeighting = 1000;
        const int potentialWinsWeighting = 100;
        const int threeWeighting = 15;


        //var score = CountSequences(4) * (depth + 1) * winWeighting;     // Can we just cut off here? A win is probably trumps any other score

        int score = FindWin() * (depth + 1) * winWeighting;
        
        if(score == 0)
        {
            score += CountPotentialWins() * (depth + 1) * potentialWinsWeighting;
        }
       
        if(!is0)
        {
            score = -score;
        }

        return score;
    }
    
    // Returns as soon as there's a win
    private int FindWin()
    {
        int sequenceLength = 4;
        int numberOfSequences = 0;
        
        for (int column = 0; column < _board.Length; column++)
        {
            List<int> currentColumn = _board[column];
            
            for (int row = 0; row < currentColumn.Count; row++)
            {
                int currentPiece = currentColumn[row];
                
                if(currentPiece == -1)
                {
                    continue;
                }
                
                // Check up
                if(   row < currentColumn.Count-3 
                      && CheckUp(currentColumn, row, currentPiece, sequenceLength))
                {
                    if(currentPiece == 0)
                    {
                        return 1;
                    } 
                    else if(currentPiece == 1)
                    {
                        return -1;
                    }
                }
                
                // Check right
                if(    column <= 5
                       &&  CheckRight(column, row, currentPiece, sequenceLength))
                {
                    if(currentPiece == 0)
                    {
                        return 1;
                    } 
                    else if(currentPiece == 1)
                    {
                        return -1;
                    }
                }
                
                // Check diagonal up
                if(   column <= 5 
                      && row <= 3
                      && CheckDiagonalUp(column, row, currentPiece, sequenceLength))
                {
                    if(currentPiece == 0)
                    {
                        return 1;
                    } 
                    else if(currentPiece == 1)
                    {
                        return -1;
                    }
                }
                
                if(   column <= 5 
                      && row >= 3
                      && CheckDiagonalDown(column, row, currentPiece, sequenceLength))
                {
                    if(currentPiece == 0)
                    {
                        return 1;
                    } 
                    else if(currentPiece == 1)
                    {
                        return -1;
                    }
                }
            }
        }

        return 0;
    }
    

    // Counts potential sequences
    // if sequenceLength is 4 finds all sequences of 3 where there is a free space for a 4th
    private int CountPotentialWins()
    {
        const int maxRow = 6;
        const int maxColumn = 8;

        int numberOfSequences = 0;
        int sequenceLength = 4;

        for (int column = 0; column < _board.Length; column++)
        {
            List<int> currentColumn = _board[column];
            
            for (int row = 0; row < currentColumn.Count; row++)
            {
                int currentPiece = currentColumn[row];

                int increment = 0;
                
                if(currentPiece == 0)
                {
                    increment = 1;
                }
                else if (currentPiece == 1)
                {
                    increment = -1;
                }

                // Check up
                if(   row < currentColumn.Count-3 
                      && CheckUp(currentColumn, row, currentPiece, sequenceLength - 1))
                {
                    if(row+(sequenceLength-1) <= maxRow && currentColumn.Count <= maxRow - (sequenceLength-1))
                    {
                        numberOfSequences += increment;
                    }
                }
                
                // Check right
                if(    column <= 6
                       &&  CheckRight(column, row, currentPiece, sequenceLength - 1))
                {
                    if(column > 0)
                    {
                        List<int> previousColumn = _board[column-1];
                        
                        if(previousColumn.Count < row)
                        {
                            numberOfSequences += increment;
                        }
                    }
                    
                    if(column <= maxColumn - (sequenceLength-1))
                    {
                        List<int> endColumn = _board[column+(sequenceLength-1)];
                        
                        if(endColumn.Count < row)
                        {
                            numberOfSequences += increment;
                        }
                    }
                }
                
                // Check diagonal up
                if(   column <= 6 
                      && row <= 4
                      && CheckDiagonalUp(column, row, currentPiece, sequenceLength - 1))
                {
                    // Check down left for empty space
                    if(column > 0 && row > 0)
                    {
                        List<int> previousColumn = _board[column-1];
                        
                        if(previousColumn.Count < row-1)
                        {
                            numberOfSequences += increment;
                        }
                    }
                    
                    // Check up right for empty space
                    if(row+(sequenceLength-1) <= maxRow && column <= maxColumn - (sequenceLength-1))
                    {
                        List<int> endColumn = _board[column+(sequenceLength-1)];
                        
                        if(endColumn.Count < row + (sequenceLength-1))
                        {
                            numberOfSequences += increment;
                        }
                    }
                }
                
                if(   column <= 6 
                      && row >= 2
                      && CheckDiagonalDown(column, row, currentPiece, sequenceLength - 1))
                {
                    // Check up left for empty spaces
                    if(column > 0 && row+(sequenceLength-1) <= maxRow)
                    {
                        List<int> previousColumn = _board[column-1];
                        
                        if(previousColumn.Count < row - 1)
                        {
                            numberOfSequences += increment;
                        }
                    }
                    
                    // Check down right
                    if(column <= maxColumn - (sequenceLength-1))
                    {
                        List<int> endColumn = _board[column+(sequenceLength-1)];
                        
                        if(endColumn.Count < row - (sequenceLength-1))
                        {
                            numberOfSequences += increment;
                        }
                    }
                }
            }
        }

        return numberOfSequences;
        
    }
    
    private int CountSequences(int sequenceLength)
    {
        int numberOfSequences = 0;
        
        for (int column = 0; column < _board.Length; column++)
        {
            List<int> currentColumn = _board[column];
            
            for (int row = 0; row < currentColumn.Count; row++)
            {
                int currentPiece = currentColumn[row];
                
                if(currentPiece == -1)
                {
                    continue;
                }
                
                // Check up
                if(   row < currentColumn.Count-3 
                      && CheckUp(currentColumn, row, currentPiece, sequenceLength))
                {
                    if(currentPiece == 0)
                    {
                        numberOfSequences++;
                    } 
                    else if(currentPiece == 1)
                    {
                        numberOfSequences--;
                    }
                }
                
                // Check right
                if(    column <= 5
                       &&  CheckRight(column, row, currentPiece, sequenceLength))
                {
                    if(currentPiece == 0)
                    {
                        numberOfSequences++;
                    } 
                    else if(currentPiece == 1)
                    {
                        numberOfSequences--;
                    }
                }
                
                // Check diagonal up
                if(   column <= 5 
                      && row <= 3
                      && CheckDiagonalUp(column, row, currentPiece, sequenceLength))
                {
                    if(currentPiece == 0)
                    {
                        numberOfSequences++;
                    } 
                    else if(currentPiece == 1)
                    {
                        numberOfSequences--;
                    }
                }
                
                if(   column <= 5 
                      && row >= 3
                      && CheckDiagonalDown(column, row, currentPiece, sequenceLength))
                {
                    if(currentPiece == 0)
                    {
                        numberOfSequences++;
                    } 
                    else if(currentPiece == 1)
                    {
                        numberOfSequences--;
                    }
                }
            }
        }

        return numberOfSequences;
    }

    private static bool CheckUp(List<int> column, int startPoisiton, int piece, int sequenceLength)
    {
        int matchCount = 0;
        
        for (int i = 1; i <= sequenceLength-1; i++)
        {
            if(column[startPoisiton+i] == piece)
            {
                matchCount++;
            }
            else
            {
                return false;
            }
        }
        
        // if(matchCount == 3)
        // {
        //     Console.Error.WriteLine($"UP");
        // }
        
        return matchCount == sequenceLength-1;
    }
    
    private bool CheckRight(int startColumn, int row, int piece, int sequenceLength)
    {
        int matchCount = 0;
        
        for (int i = 1; i <= sequenceLength-1; i++)
        {
            List<int> nextColumn = _board[startColumn+i];
            
            if(nextColumn.Count >= row+1 && nextColumn[row] == piece)
            {
                matchCount++;
            }
            else
            {
                return false;
            }
        }
        
        return matchCount == sequenceLength-1;
    }
    
    private bool CheckDiagonalUp(int startColumn, int startRow, int piece, int sequenceLength)
    {
        int matchCount = 0;
        
        for (int i = 1; i <= sequenceLength-1; i++)
        {
            List<int> nextColumn = _board[startColumn+i];
            
            if(nextColumn.Count >= startRow+i+1 && nextColumn[startRow+i] == piece)
            {
                matchCount++;
            }
            else
            {
                return false;
            }
        }
        
        return matchCount == sequenceLength-1;
    }
    
    private bool CheckDiagonalDown(int startColumn, int startRow, int piece, int sequenceLength)
    {
        int matchCount = 0;
        
        for (int i = 1; i <= sequenceLength-1; i++)
        {
            List<int> nextColumn = _board[startColumn+i];
            
            if(nextColumn.Count >= startRow-i+1 && nextColumn[startRow-i] == piece)
            {
                matchCount++;
            }
            else
            {
                return false;
            }
        }
        
        return matchCount == sequenceLength-1;
    }

    public bool IsGameOver()
    {
        if(CountSequences(4) != 0)  // Add || board is full
        {
            return true;
        }
        
        return false;
    }
    
    internal void SetMoveSequence(string moves)
    {
        bool isPlayer0 = true;
        
        foreach (char move in moves)
        {
            AddMove(int.Parse(move.ToString()), isPlayer0 ? 0 : 1);
            
            isPlayer0 = !isPlayer0;
        }
    }
    
    
    internal void PrintBoard()
    {
        for (int i = 6; i >= 0; i--)            
        {
            string rowText = string.Empty;
            
            for (int j = 0; j < 9; j++)
            {
                if (_board[j].Count >= i+1)
                {
                    rowText += _board[j][i];
                }
                else
                {
                    rowText += "-";
                }
            }

            Console.Error.WriteLine(rowText);
        }
    }
    public string DisplayBoard()
    {
        string board = string.Empty;
        
        for (int i = 6; i >= 0; i--)            
        {
            string rowText = string.Empty;
            
            for (int j = 0; j < 9; j++)
            {
                if (_board[j].Count >= i+1)
                {
                    rowText += _board[j][i];
                }
                else
                {
                    rowText += "-";
                }
            }

            board += rowText + "\n";
        }
        
        return board;
    }
}