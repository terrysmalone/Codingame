using System.Collections.Generic;

namespace UltimateTicTacToe
{
    internal sealed class Game
    {
        internal List<Move> ValidActions { get; set; }

        public char PlayerPiece { get; private set; }
        public char EnemyPiece { get; private set; }

        private readonly MultiTicTacToe _multiTicTacToe;
        private readonly MoveCalculator _moveCalculator;

        private readonly int _depth = 3;

        public Game()
        {
            _moveCalculator = new MoveCalculator();
            _multiTicTacToe = new MultiTicTacToe();
        }

        public Move GetAction()
        {
            //_ultimateTicTacToe.PrintBoard();
        
            return _moveCalculator.GetBestMoveUsingAlphaBeta(_multiTicTacToe, _depth, PlayerPiece);
        }

        internal void AddMove(int column, int row, char piece)
        {
            _multiTicTacToe.AddMove(column, row, piece);
        }

        internal void SetPlayer(char playerPiece)
        {
            PlayerPiece = playerPiece;

            PlayerPiece = playerPiece;
            EnemyPiece = playerPiece == 'O' ? 'X' : 'O';
        }
    }
}