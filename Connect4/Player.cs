using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleToAttribute("Connect4Tests")]
namespace Connect4
{
    /**
     * Drop chips in the columns.
     * Connect at least 4 of your chips in any direction to win.
     **/
    class Player
    {
        static void Main(string[] args)
        {
            var inputs = Console.ReadLine().Split(' ');
            var myId = int.Parse(inputs[0]); // 0 or 1 (Player 0 plays first)
            var oppId = int.Parse(inputs[1]); // if your index is 0, this will be 1, and vice versa

            var game = new Game(myId);
            
            // game loop
            while (true)
            {
                var turnIndex = int.Parse(Console.ReadLine()); // starts from 0; As the game progresses, first player gets [0,2,4,...] and second player gets [1,3,5,...]
                for (var i = 0; i < 7; i++)
                {
                    var boardRow = Console.ReadLine(); // one row of the board (from top to bottom)
                }

                var numValidActions = int.Parse(Console.ReadLine()); // number of unfilled columns in the board
                for (var i = 0; i < numValidActions; i++)
                {
                    var action = int.Parse(Console.ReadLine()); // a valid column index into which a chip can be dropped
                }
                var oppPreviousAction = int.Parse(Console.ReadLine()); // opponent's previous chosen column index (will be -1 for first player in the first turn)

                if (oppPreviousAction != -1)
                {
                    game.AddOpponentMove(oppPreviousAction);
                }
                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");


                // Output a column index to drop the chip in. Append message to show in the viewer.
                var move = game.GetMove();
                Console.WriteLine(move);
            }
        }
    }
    
    internal sealed class Game
    {
        private int _depth = 6;
        
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
            var bestMove = _calculator.GetBestMoveUsingAlphaBeta(_connectFour, _depth, _myId);

            _connectFour.AddMove(bestMove, _myId);
            
            //_connectFour.PrintBoard();
            
            return bestMove.ToString();
        }

        internal void AddOpponentMove(int column)
        {
            _connectFour.AddMove(column, _oppId);
        }
    }
    
    internal sealed class ConnectFour
    {
        private List<int>[] _board;
        
        private const int _boardMax = 6;
        
        internal ConnectFour()
        {
            
            _board = new List<int>[9];

            for (var i = 0; i < _board.Length; i++)
            {
                _board[i] = new List<int>();
            }
        }
        
        public List<int> CalculateValidMoves()
        {
            var validMoves = new List<int>();

            for (var i = 0; i < _board.Length; i++)
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
        
        public int Evaluate(bool is0, int depth = 0)
        {
            int score;

            if(is0)
            {
                score = EvaluateBoard() * depth;
            }
            else
            {
                score = -EvaluateBoard() * depth;
            }

            return score;
        }
        
        private int EvaluateBoard()
        {
            // foreach cell
                // check up
                // Check right
                // check diagonal up
                // check diagonal down
                // 
            
            for (var i = 0; i < _board.Length; i++)
            {
                var currentColumn = _board[i];
                
                for (var j = 0; j < currentColumn.Count; j++)
                {
                    var currentPiece = currentColumn[j];
                    
                    if(currentPiece == -1)
                    {
                        continue;
                    }
                    
                    // Check up
                    if(   j < currentColumn.Count-3 
                       && CheckUp(currentColumn, j, currentPiece))
                    {
                        if(currentPiece == 0)
                        {
                            return +1;
                        } 
                        else if(currentPiece == 1)
                        {
                            return -1;
                        }
                    }
                    
                    // Check right
                    if(    i <= 5
                       &&  CheckRight(i, j, currentPiece))
                    {
                        if(currentPiece == 0)
                        {
                            return +1;
                        } 
                        else if(currentPiece == 1)
                        {
                            return -1;
                        }
                    }
                    
                    // Check diagonal up
                    if(   i <= 5 
                       && j <= 3
                       && CheckDiagonalUp(i, j, currentPiece))
                    {
                        if(currentPiece == 0)
                        {
                            return +1;
                        } 
                        else if(currentPiece == 1)
                        {
                            return -1;
                        }
                    }
                    
                    if(   i <= 5 
                       && j >= 3
                       && CheckDiagonalDown(i, j, currentPiece))
                    {
                        if(currentPiece == 0)
                        {
                            return +1;
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

        private static bool CheckUp(List<int> column, int startPoisiton, int piece)
        {
            var matchCount = 0;
            
            for (var i = 1; i <= 3; i++)
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
            
            return matchCount == 3;
        }
        
        private bool CheckRight(int startColumn, int row, int piece)
        {
            var matchCount = 0;
            
            for (var i = 1; i <= 3; i++)
            {
                var nextColumn = _board[startColumn+i];
                
                if(nextColumn.Count >= row+1 && nextColumn[row] == piece)
                {
                    matchCount++;
                }
                else
                {
                    return false;
                }
            }
            
            return matchCount == 3;
        }
        
        private bool CheckDiagonalUp(int startColumn, int startRow, int piece)
        {
            var matchCount = 0;
            
            for (var i = 1; i <= 3; i++)
            {
                var nextColumn = _board[startColumn+i];
                
                if(nextColumn.Count >= startRow+i+1 && nextColumn[startRow+i] == piece)
                {
                    matchCount++;
                }
                else
                {
                    return false;
                }
            }
            
            return matchCount == 3;
        }
        
        private bool CheckDiagonalDown(int startColumn, int startRow, int piece)
        {
            var matchCount = 0;
            
            for (var i = 1; i <= 3; i++)
            {
                var nextColumn = _board[startColumn+i];
                
                if(nextColumn.Count >= startRow-i+1 && nextColumn[startRow-i] == piece)
                {
                    matchCount++;
                }
                else
                {
                    return false;
                }
            }
            
            return matchCount == 3;
        }

        public bool IsGameOver()
        {
            if(Evaluate(true, 0) != 0)  // Add || board is full
            {
                return true;
            }
            
            return false;
        }
        
        internal void PrintBoard()
        {
            for (var i = 6; i >= 0; i--)            
            {
                var rowText = string.Empty;
                
                for (var j = 0; j < 9; j++)
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
    }
    
    internal sealed class MoveCalculator
    {
        private ConnectFour _connectFour;

        internal int GetBestMoveUsingAlphaBeta(ConnectFour connectFour, int depth, int startingPlayer)
        {
            var moves = GetMoveScoresUsingAlphaBeta(connectFour, depth, startingPlayer).OrderByDescending(m => m.Item2).ToList();
            
            var max = moves.Max(m => m.Item2);
            
            PrintMoveScores(moves);
            
            var highest = moves.Where(m => m.Item2 == max).ToList();

            var rand = new Random();
            
            return highest[rand.Next(highest.Count)].Item1;
        
            //return GetMoveScoresUsingAlphaBeta(ticTacToeBoard, depth, startingPlayer).OrderByDescending((m => m.Item2)).First().Item1;
        }
        private void PrintMoveScores(List<Tuple<int, int>> moves)
        {
            Console.Error.WriteLine($"---------------------------------------");

            foreach (var move in moves)
            {
                Console.Error.WriteLine($"Move:{move.Item1}, score:{move.Item2}");
            }
        }

        internal List<Tuple<int, int>> GetMoveScoresUsingAlphaBeta(ConnectFour connectFour, int depth, int player)
        {
            _connectFour = connectFour;

            var moveScores = new List<Tuple<int, int>>();

            var validMoves = _connectFour.CalculateValidMoves();

            foreach (var validAction in validMoves)
            {
                var is0 = player == 0;

                _connectFour.AddMove(validAction, player);

                var score = -Calculate(int.MinValue+1, int.MaxValue, depth-1, !is0, SwapPieces(player));

                moveScores.Add(new Tuple<int, int>(validAction, score));

                _connectFour.UndoMove(validAction);
            }

            return moveScores;
        }

        private int Calculate(int alpha, int beta, int depth, bool is0, int piece)
        {
            if (depth == 0)
            {
                return _connectFour.Evaluate(is0, depth);
            }

            var validMoves = _connectFour.CalculateValidMoves();

            if(validMoves.Count == 0
               || _connectFour.IsGameOver())
            {
                return _connectFour.Evaluate(is0, depth);
            }

            var score = int.MinValue;

            foreach (var move in validMoves)
            {
                _connectFour.AddMove(move, piece);
                score = Math.Max(score, -Calculate(-beta, -alpha,depth-1, !is0, SwapPieces(piece)));

                _connectFour.UndoMove(move);

                alpha = Math.Max(alpha, score);

                if (alpha >= beta)
                {
                    return alpha;
                }
            }

            return score;
        }
        
        private static int SwapPieces(int piece)
        {
            return piece == 0 ? 1 : 0;
        }
    }
}