using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

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

    internal sealed class Game
    {
        private int _depth = 5;
        
        private int _myId;
        private int _oppId;
        
        private ConnectFour _connectFour;
        private MoveCalculator _calculator;
        
        private int _turn = 0;

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
        
        internal void AddMyMove(int column)
        {
            _connectFour.AddMove(column, _myId);
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
                if(_board[i].Count < _boardMax)
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
        
        public int Evaluate(bool isX, int depth)
        {
            int score;

            if(isX)
            {
                score = -EvaluateBoard() - depth;
            }
            else
            {
                score = EvaluateBoard() + depth;
            }

            return score;
        }
        
        private int EvaluateBoard()
        {
            for (var i = 0; i < _board.Length; i++)
            {
                var currentColumn = _board[i];
                var columnHeight = currentColumn.Count;

                var run0 = 0;
                var run1 = 0;
                
                var last = -1;

                if(columnHeight >= 4)
                {
                    //Console.Error.WriteLine($"Column: {i} - height:{columnHeight}");
                    
                    for (var j = 0; j < columnHeight-4; j++)
                    {
                        var currentPiece = currentColumn[j];
                        if(currentPiece == last)
                        {
                            if(currentPiece == 0)
                            {
                                run0++;
                                run1 = 0;
                            }
                            else if (currentPiece == 1)
                            {
                                run1++;
                                run0 = 0;
                            }
                            else
                            {
                                run0 = 0;
                                run1 = 0;
                            }
                        }
                        else
                        {
                            run0 = 0;
                            run1 = 0;
                        }
                        
                        last = currentPiece;
                    }
                }
                
                if(run0 >= 4)
                {
                    return +1;
                } 
                
                if(run1 >= 4)
                {
                    Console.Error.WriteLine("Opponent WON");
                    return -1;
                }

                if(i <= 5)  // check right
                {
                    
                }
                
                
            }
        
            // for each point 
                // Is it within 4 up
                    // check 4 up
                // Is it within 4 to the right
                    // check 4 to the right
            
            return 0;
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
            
            var highest = moves.Where(m => m.Item2 == max).ToList();
            
            var rand = new Random();
            
            return highest[rand.Next(highest.Count)].Item1;
        
            //return GetMoveScoresUsingAlphaBeta(ticTacToeBoard, depth, startingPlayer).OrderByDescending((m => m.Item2)).First().Item1;
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

        private int Calculate(int alpha, int beta, int depth, bool isX, int piece)
        {
            if (depth == 0)
            {
                return _connectFour.Evaluate(isX, depth);
            }

            var validMoves = _connectFour.CalculateValidMoves();

            if(validMoves.Count == 0
               || _connectFour.IsGameOver())
            {
                return _connectFour.Evaluate(isX, depth);
            }

            var score = int.MinValue;

            foreach (var move in validMoves)
            {
                _connectFour.AddMove(move, piece);
                score = Math.Max(score, -Calculate(-beta, -alpha,depth-1, !isX, SwapPieces(piece)));

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