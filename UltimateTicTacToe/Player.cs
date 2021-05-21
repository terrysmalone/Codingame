﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleToAttribute("UltimateTicTacToeTest")]
namespace UltimateTicTacToe
{
    /**
     * Auto-generated code below aims at helping you parse
     * the standard input according to the problem statement.
     **/
    class Player
    {
        static void Main(string[] args)
        {
            var game = new Game();

            string[] inputs;
            var moveNum = 0;
            
            // game loop
            while (true)
            {
                inputs = Console.ReadLine().Split(' ');
                var opponentRow = int.Parse(inputs[0]);
                var opponentCol = int.Parse(inputs[1]);
                
                if(moveNum == 0)
                {
                    if(opponentRow != -1)
                    {
                        game.SetPlayer('X');
                        game.AddMove(opponentCol, opponentRow, game.EnemyPiece);
                        moveNum++;
                    }
                    else
                    {
                        game.SetPlayer('O');
                    }
                }
                else
                {
                    game.AddMove(opponentCol, opponentRow, game.EnemyPiece);
                    moveNum++;
                }

                var validActionCount = int.Parse(Console.ReadLine());
                
                var validActions = new List<Move>();
                
                for (var i = 0; i < validActionCount; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    
                    var row = int.Parse(inputs[0]);
                    var column = int.Parse(inputs[1]);
                    validActions.Add(new Move(column, row));
                }

                game.ValidActions = validActions;
                
                // If we're first might as well pick a corner
                var action = game.GetAction();
                
                game.AddMove(action.Column, action.Row, game.PlayerPiece);
                Console.WriteLine($"{action.Row} {action.Column}");
                
                moveNum++;
            }
        }
    }

    internal sealed class Game
    {
        internal List<Move> ValidActions { get; set; }
        
        private char _playerPiece; 
        public char PlayerPiece => _playerPiece;   
        
        public char _enemyPiece;
        public char EnemyPiece => _enemyPiece;
        

        private TicTacToe[,] _boards = new TicTacToe[3,3];
        private TicTacToe _overArchingTicTacToe;
        private char _player = '\0';
        
        private int _depth = 6;
        
        public Game()
        {
            _boards[0,0] = new TicTacToe();
            _boards[0,1] = new TicTacToe();
            _boards[0,2] = new TicTacToe();
            _boards[1,0] = new TicTacToe();
            _boards[1,1] = new TicTacToe();
            _boards[1,2] = new TicTacToe();
            _boards[2,0] = new TicTacToe();
            _boards[2,1] = new TicTacToe();
            _boards[2,2] = new TicTacToe();
        
            _overArchingTicTacToe = new TicTacToe();
        }

        public Move GetAction()
        {
            TicTacToe boardInPlay = null;
            
            var boardInPlayColumn = 0;
            var boardInPlayRow = 0;
            
            
            // Identify which board we're playing on (it could be them all)
            
            // If the range between either row or column is 3 or more we're being given a choice from multiple boards
            if(   ValidActions.Max(a => a.Column) - ValidActions.Min(a => a.Column) >= 3
               || ValidActions.Max(a => a.Row) - ValidActions.Min(a => a.Row) >= 3)
            {
                // We get to choose which board to play
                // We're testing. Just play the first one
                boardInPlayColumn = ValidActions.First().Column/3;
                boardInPlayRow = ValidActions.First().Row/3;
                boardInPlay = _boards[boardInPlayColumn,boardInPlayRow];
            }
            else
            {
                boardInPlayColumn = ValidActions.First().Column/3;
                boardInPlayRow = ValidActions.First().Row/3;
                boardInPlay = _boards[boardInPlayColumn, boardInPlayRow];
            }
            
            // Make a move on that board
            var moveScores = boardInPlay.GetMoveScores(_depth, _player);
            //PrintMovesAndScoresList(moveScores);
            
            // Get all the best scores
            var highest = moveScores.OrderByDescending(m => m.Item2).First().Item2;
            var highestMoves = moveScores.Where(m => m.Item2 == highest).Select(m => m.Item1).ToList();
            
            var currentMax = int.MinValue;
            Move bestMove = null;
            
            // I wnt to move my opponent onto the game where I have the most pieces relative to him
            foreach (var highMove in highestMoves)
            {
                var currentBoard = _boards[highMove.Column, highMove.Row];
                
                //currentBoard.PrintBoard();
                
                var pieceScore = currentBoard.GetNumberOfPiecesScore(_player);
                
                // To Do: Don't include finished games. If we try to send them their they get free reign
                if(!currentBoard.IsGameOver() && pieceScore > currentMax)
                {
                    currentMax = pieceScore;
                    bestMove = highMove;
                }
            }
            
            if(bestMove == null)
            {
                // Go back to random for now
                var rand = new Random();
                bestMove = highestMoves[rand.Next(highestMoves.Count)];
            }

            return new Move(boardInPlayColumn * 3 + bestMove.Column, boardInPlayRow * 3 + bestMove.Row);
        }

        internal void AddMove(int column, int row, char piece)
        {
            _boards[column / 3, row / 3].AddMove(column % 3, row % 3, piece);
        }
        
        internal void SetPlayer(char playerPiece)
        {
            _player = playerPiece;
            
            _playerPiece = playerPiece;
            _enemyPiece = playerPiece == 'O' ? 'X' : 'O';
        }
        
        private static void PrintMovesAndScoresList(List<Tuple<Move, int>> moveScores)
        {
            Console.Error.WriteLine("======================");
            
            foreach (var moveScore in moveScores)
            {
                Console.Error.WriteLine($"Move:{moveScore.Item1.Column}, {moveScore.Item1.Row} - Score:{moveScore.Item2}");
            }
        }
    }
    
    internal sealed class TicTacToe
    {
        private char[,] _board = new char[3,3];

        internal Move GetBestMove(int depth, char startingPlayer)
        {
            return GetMoveScores(depth, startingPlayer).OrderByDescending((m => m.Item2)).First().Item1;
        }
        
        internal List<Tuple<Move, int>> GetMoveScores(int depth, char player)
        {
            var moveScores = new List<Tuple<Move, int>>();
            
            var validMoves = CalculateValidMoves();

            foreach (var validAction in validMoves) 
            {
                var maximisingPlayer = player == 'X';
                
                AddMove(validAction.Column, validAction.Row, player);
                
                var score = -Calculate(depth-1, !maximisingPlayer, SwapPieces(player));
                
                moveScores.Add(new Tuple<Move, int>(new Move(validAction.Column, validAction.Row), score));
                
                UndoMove(validAction.Column, validAction.Row);

            }
            
            return moveScores;
        }
        
        private int Calculate(int depth, bool maximisingPlayer, char piece)
        {
            if (depth == 0)
            {
                return Evaluate(maximisingPlayer, depth);
            }
            
            var evaluation = Evaluate(maximisingPlayer, depth);
            
            if(evaluation != 0)
            {
                return evaluation;
            }
            
            var maxScore = int.MinValue;
            
            foreach (var move in CalculateValidMoves())
            {
                AddMove(move.Column, move.Row, piece);
                
                var score = -Calculate(depth-1, !maximisingPlayer, SwapPieces(piece));
                
                UndoMove(move.Column, move.Row);
                
                if (score > maxScore)
                {
                    maxScore = score;
                }
            }
            
            return maxScore;
        }
        
        private List<Move> CalculateValidMoves()
        {
            var moves = new List<Move>();
            
            for(var column = 0; column < _board.GetLength(0); column++)
            {
                for(var row = 0; row < _board.GetLength(1); row++)
                {
                    if(_board[column, row] == '\0')
                    {
                        moves.Add(new Move(column, row));
                    }
                }
            }
           
            return moves;
        }

        private int Evaluate(bool maximisingPlayer, int currentDepth)
        {
            int score;
            
            if(maximisingPlayer)
            {
                score = EvaluateBoard(currentDepth);
            }
            else
            {
                score = -EvaluateBoard(currentDepth);
            }
            
            return score;
        }

        private List<Move[]> _lines = new List<Move[]>
        {
                new[] { new Move(0,0), new Move(0,1), new Move(0,2) }, // Left column
                new[] { new Move(1,0), new Move(1,1), new Move(1,2) }, // Middle column
                new[] { new Move(2,0), new Move(2,1), new Move(2,2) }, // Right column
                    
                new[] { new Move(0,0), new Move(1,0), new Move(2,0) }, // Top row
                new[] { new Move(0,1), new Move(1,1), new Move(2,1) }, // middle row
                new[] { new Move(0,2), new Move(1,2), new Move(2,2) }, // Bottom row
                    
                new[] { new Move(0,0), new Move(1,1), new Move(2,2) }, // top left to bottom right diagonal
                new[] { new Move(2,0), new Move(1,1), new Move(0,2) }  // bottom left to top right diagonal
        };
       
        
        private int EvaluateBoard(int currentDepth)
        {
            foreach (var line in _lines)
            {
                var playerWithLine = PlayerWithLine(line);
                
                if(playerWithLine == 'X')
                {
                    return 1 + currentDepth;
                }
                
                if(playerWithLine == 'O')
                {
                    return -1 - currentDepth;
                }
            }
            
            return 0;
        }
        private char PlayerWithLine(Move[] line)
        {
            if (DoesPlayerHaveLine(line, 'X'))
            {
                return 'X';
            }
            else if (DoesPlayerHaveLine(line, 'O'))
            {
                return 'O';
            }
            
            return '\0';
        }
        private bool DoesPlayerHaveLine(Move[] line, char player)
        {
            if(   _board[line[0].Column, line[0].Row] == player 
               && _board[line[1].Column, line[1].Row] == player
               && _board[line[2].Column, line[2].Row] == player)
            {
                return true;
            }
            
            return false;
        }
        
        internal void AddMove(int column, int row, char piece)
        {
            _board[column, row] = piece;
        }
        
        private void UndoMove(int column, int row)
        {
            _board[column, row] = '\0';
        }

        internal void SetBoard(char[,] board)
        {
            _board = (char[,])board.Clone();
        }
        
        private static char SwapPieces(char piece)
        {
            return piece == 'O' ? 'X' : 'O';
        }
    
        // Returns my placed pieces - opponent placed pieces
        internal int GetNumberOfPiecesScore(char player)
        {
            var playerPieces = 0;
            var opponentPieces = 0;
            
            foreach (var cell in _board)
            {
                if(cell  == player)
                {
                    playerPieces++;
                }
                else if(cell != '\0')
                {
                    opponentPieces++;
                }
            }

            return playerPieces - opponentPieces;
        }
        
        internal bool IsGameOver()
        {
            if(   EvaluateBoard(0) != 0
               || AvailableSpacesOnBoard() > 0)
            {
                return true;
            }
            
            return false;
        }
        
        private int AvailableSpacesOnBoard()
        {
            var availableSpaces = 0;

            foreach (var cell in _board)
            {
                if(cell != '\0')
                {
                    availableSpaces++;
                }
            }
            
            return availableSpaces;
        }

        internal void PrintBoard()
        {
            
            for(var row = 0; row < _board.GetLength(1); row++)
            {
                for(var column = 0; column < _board.GetLength(0); column++)
                {
                    if(_board[column, row] == 'X')
                    {
                        Console.Error.Write("X");
                    }
                    else if(_board[column, row] == 'O')
                    {
                        Console.Error.Write("O");
                    }
                    else
                    {
                        Console.Error.Write(" ");
                    }
                    
                    Console.Error.Write("|");
                }
                
                Console.Error.WriteLine();
                Console.Error.WriteLine("------------");
            }
        }
    }

    internal sealed class Move
    {
        public int Row { get; }
        public int Column { get; }

        public Move(int column, int row)
        {
            Column = column;
            Row = row;
        }
    }
}

// while (true)
// {
//     inputs = Console.ReadLine().Split(' ');
//     int opponentRow = int.Parse(inputs[0]);
//     int opponentCol = int.Parse(inputs[1]);
//     int validActionCount = int.Parse(Console.ReadLine());
//     for (int i = 0; i < validActionCount; i++)
//     {
//         inputs = Console.ReadLine().Split(' ');
//         int row = int.Parse(inputs[0]);
//         int col = int.Parse(inputs[1]);
//     }
//
//     // Write an action using Console.WriteLine()
//     // To debug: Console.Error.WriteLine("Debug messages...");
//
//     Console.WriteLine("0 0");
// }