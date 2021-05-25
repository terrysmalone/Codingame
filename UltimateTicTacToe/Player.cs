using System;
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
                        game.SetPlayer('O');
                        game.AddMove(opponentCol, opponentRow, game.EnemyPiece);
                        moveNum++;
                    }
                    else
                    {
                        game.SetPlayer('X');
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

        public char PlayerPiece { get; private set; }
        public char EnemyPiece { get; private set; }

        private readonly UltimateTicTacToe _ultimateTicTacToe;
        private readonly MoveCalculator _moveCalculator;

        private readonly int _depth = 4;

        public Game()
        {
            _moveCalculator = new MoveCalculator();
            _ultimateTicTacToe = new UltimateTicTacToe();
        }

        public Move GetAction()
        {
            return _moveCalculator.GetBestMoveUsingAlphaBeta(_ultimateTicTacToe, _depth, PlayerPiece);
        }

        internal void AddMove(int column, int row, char piece)
        {
            _ultimateTicTacToe.AddMove(column, row, piece);
        }

        internal void SetPlayer(char playerPiece)
        {
            PlayerPiece = playerPiece;

            PlayerPiece = playerPiece;
            EnemyPiece = playerPiece == 'O' ? 'X' : 'O';
        }
    }

    internal sealed class MoveCalculator
    {
        private ITicTacToe _board;

        internal Move GetBestMoveUsingAlphaBeta(ITicTacToe ticTacToeBoard, int depth, char startingPlayer)
        {
            return GetMoveScoresUsingAlphaBeta(ticTacToeBoard, depth, startingPlayer).OrderByDescending((m => m.Item2)).First().Item1;
        }

        internal List<Tuple<Move, int>> GetMoveScoresUsingAlphaBeta(ITicTacToe ticTacToeBoard, int depth, char player)
        {
            _board = ticTacToeBoard;

            var moveScores = new List<Tuple<Move, int>>();

            var validMoves = _board.CalculateValidMoves();

            foreach (var validAction in validMoves)
            {
                var isX = player == 'X';

                _board.AddMove(validAction.Column, validAction.Row, player);

                var score = -Calculate(int.MinValue, int.MaxValue, depth-1, !isX, SwapPieces(player));

                moveScores.Add(new Tuple<Move, int>(new Move(validAction.Column, validAction.Row), score));

                _board.UndoMove(validAction.Column, validAction.Row);
            }

            //PrintMovesList(moveScores);

            return moveScores;
        }

        private int Calculate(int alpha, int beta, int depth, bool isX, char piece)
        {
            if (depth == 0)
            {
                return _board.Evaluate(isX, depth);
            }

            var validMoves = _board.CalculateValidMoves();

            if(validMoves.Count == 0)
            {
                return _board.Evaluate(isX, depth);
            }

            var evaluation = _board.Evaluate(isX, depth);

            if(evaluation != 0)
            {
                return evaluation;
            }

            var score = int.MinValue;

            foreach (var move in validMoves)
            {
                _board.AddMove(move.Column, move.Row, piece);
                NodesVisited++;
                score = Math.Max(score, -Calculate(-beta, -alpha,depth-1, !isX, SwapPieces(piece)));

                _board.UndoMove(move.Column, move.Row);

                alpha = Math.Max(alpha, score);

                if (alpha >= beta)
                {
                    return alpha;
                }
            }

            return score;
        }

        //------------------------------------------------------------------
        internal Move GetBestMove(ITicTacToe ticTacToeBoard, int depth, char startingPlayer)
        {
            return GetMoveScores(ticTacToeBoard, depth, startingPlayer).OrderByDescending((m => m.Item2)).First().Item1;
        }

        private List<Tuple<Move, int>> GetMoveScores(ITicTacToe ticTacToeBoard, int depth, char player)
        {
            _board = ticTacToeBoard;

            var moveScores = new List<Tuple<Move, int>>();

            var validMoves = _board.CalculateValidMoves();

            foreach (var validAction in validMoves)
            {
                var maximisingPlayer = player == 'X';

                _board.AddMove(validAction.Column, validAction.Row, player);

                var score = -Calculate(depth-1, !maximisingPlayer, SwapPieces(player));

                moveScores.Add(new Tuple<Move, int>(new Move(validAction.Column, validAction.Row), score));

                _board.UndoMove(validAction.Column, validAction.Row);
            }

            return moveScores;
        }

        private int Calculate(int depth, bool maximisingPlayer, char piece)
        {
            if (depth == 0)
            {
                return _board.Evaluate(maximisingPlayer, depth);
            }

            var validMoves = _board.CalculateValidMoves();

            if(validMoves.Count == 0)
            {
                return _board.Evaluate(maximisingPlayer, depth);
            }

            var evaluation = _board.Evaluate(maximisingPlayer, depth);

            if(evaluation != 0)
            {
                return evaluation;
            }

            var maxScore = int.MinValue;

            foreach (var move in validMoves)
            {
                _board.AddMove(move.Column, move.Row, piece);

                var score = -Calculate(depth-1, !maximisingPlayer, SwapPieces(piece));

                _board.UndoMove(move.Column, move.Row);

                if (score > maxScore)
                {
                    maxScore = score;
                }
            }

            return maxScore;
        }

        private static char SwapPieces(char piece)
        {
            return piece == 'O' ? 'X' : 'O';
        }

        private void PrintMovesList(List<Move> moves)
        {
            Console.Error.WriteLine("======================");

            foreach (var move in moves)
            {
                Console.Error.WriteLine($"Move:{move.Column}, {move.Row}");
            }
        }

        private void PrintMovesList(List<Tuple<Move, int>> moveScores)
        {
            Console.Error.WriteLine("======================");

            foreach (var moveScore in moveScores)
            {
                Console.Error.WriteLine($"Move:{moveScore.Item1.Column}, {moveScore.Item1.Row} - {moveScore.Item2}");
            }
        }
    }

    internal sealed class UltimateTicTacToe : ITicTacToe
    {
        private Move _previousActiveBoard;
        private Move ActiveBoard { get; set; } = new Move(-1, -1);
        public TicTacToe[,] SubBoards { get; }
        public TicTacToe Board { get;  }

        internal UltimateTicTacToe()
        {
            Board = new TicTacToe();

            SubBoards = new TicTacToe[3,3];
            SubBoards[0,0] = new TicTacToe();
            SubBoards[0,1] = new TicTacToe();
            SubBoards[0,2] = new TicTacToe();
            SubBoards[1,0] = new TicTacToe();
            SubBoards[1,1] = new TicTacToe();
            SubBoards[1,2] = new TicTacToe();
            SubBoards[2,0] = new TicTacToe();
            SubBoards[2,1] = new TicTacToe();
            SubBoards[2,2] = new TicTacToe();
        }

        public void AddMove(int column, int row, char piece)
        {
            _previousActiveBoard = ActiveBoard;

            var localColumn = column % 3;
            var localRow = row % 3;

            SubBoards[column / 3, row / 3].AddMove(localColumn, localRow, piece);

            // update active board
            var probablyNextActiveBoard = SubBoards[localColumn, localRow];

            if (!probablyNextActiveBoard.IsGameOver())
            {
                ActiveBoard = new Move(localColumn, localRow);
            }
            else
            {
                ActiveBoard = new Move(-1, -1);
            }

            UpdateOverallBoard();
        }

        public void UndoMove(int column, int row)
        {
            SubBoards[column / 3, row / 3].UndoMove(column % 3, row % 3);

            // Set active board back
            ActiveBoard = _previousActiveBoard;

            UpdateOverallBoard();
        }

        private void UpdateOverallBoard()
        {
            for(var column = 0; column < 3; column++)
            {
                for(var row = 0; row < 3; row++)
                {
                    var evaluation = SubBoards[column, row].EvaluateBoard();

                    if(evaluation > 0)
                    {
                        Board.AddMove(column, row, 'O');
                    }
                    else if(evaluation < 0)
                    {
                        Board.AddMove(column, row, 'X');
                    }
                }
            }
        }

        public List<Move> CalculateValidMoves()
        {
            var moves = new List<Move>();

            if (ActiveBoard.Column == -1)
            {
                for (var column = 0; column < 3; column++)
                {
                    for (var row = 0; row < 3; row++)
                    {
                        var subBoard = SubBoards[column, row];

                        if (!subBoard.IsGameOver())
                        {
                            moves.AddRange(TranslateToGlobalMoves(subBoard.CalculateValidMoves(), new Move(column, row)));
                        }
                    }
                }
            }
            else
            {
                moves.AddRange(TranslateToGlobalMoves(SubBoards[ActiveBoard.Column, ActiveBoard.Row].CalculateValidMoves(), ActiveBoard));
            }

            return moves;
        }

        private List<Move> TranslateToGlobalMoves(List<Move> moves, Move activeBoard)
        {
            var translatedMoves = new List<Move>();

            foreach (var move in moves)
            {
                translatedMoves.Add(TranslateToGlobalMove(move, activeBoard));
            }

            return translatedMoves;
        }

        private Move TranslateToGlobalMove(Move move, Move activeBoard)
        {
            return new Move(activeBoard.Column * 3 + move.Column, activeBoard.Row * 3 + move.Row);
        }

        public int Evaluate(bool isX, int depth)
        {
            var score = 0;

            foreach (var board in SubBoards)
            {
                score += board.Evaluate(isX, depth);
            }

            score += Board.Evaluate(isX, depth) * 10;

            return score;
        }
    }

    internal interface ITicTacToe
    {
        List<Move> CalculateValidMoves();
        int Evaluate(bool isX, int currentDepth);
        void AddMove(int column, int row, char piece);
        void UndoMove(int column, int row);

    }

    internal sealed class TicTacToe : ITicTacToe
    {
        private char[,] _board = new char[3,3];

        public List<Move> CalculateValidMoves()
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

        public int Evaluate(bool isX, int currentDepth)
        {
            int score;

            if(isX)
            {
                score = -EvaluateBoard(currentDepth);
            }
            else
            {
                score = EvaluateBoard(currentDepth);
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

        internal int EvaluateBoard(int currentDepth = 0)
        {
            foreach (var line in _lines)
            {
                var playerWithLine = PlayerWithLine(line);

                if(playerWithLine == 'O')
                {
                    return 1 + currentDepth;
                }

                if(playerWithLine == 'X')
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

        public void AddMove(int column, int row, char piece)
        {
            _board[column, row] = piece;
        }

        public void UndoMove(int column, int row)
        {
            _board[column, row] = '\0';
        }

        internal void SetBoard(char[,] board)
        {
            _board = (char[,])board.Clone();
        }

        internal char[,] GetBoard()
        {
            return (char[,])_board.Clone();
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
            if(   EvaluateBoard() != 0
               || AvailableSpacesOnBoard() == 0)
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
                if(cell == '\0')
                {
                    availableSpaces++;
                }
            }

            return availableSpaces;
        }

        internal void PrintBoard()
        {
            Console.Error.WriteLine("------");

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
                Console.Error.WriteLine("------");
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

        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            else
            {
                var m = (Move) obj;
                return (Column == m.Column) && (Row == m.Row);
            }
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
