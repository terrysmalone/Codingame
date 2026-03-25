using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace LineRacing;

internal sealed class Game
{
    private int _width;
    private int _height;

    private bool[,] _grid;

    private Point _myPosition;
    private Point _enemyposition;

    private List<Point> _myPath = new List<Point>();

    private FloodFill _floodFill;
    private MapChecker _mapChecker;

    private bool _filling = false;
    private bool _enemyTrapped = false;

    internal Game(int width, int height)
    {
        _width = width;
        _height = height;

        _grid = new bool[height, width];

        _floodFill = new FloodFill(_grid);
        _mapChecker = new MapChecker(_grid);
    }

    internal void UpdateMyPosition(Point myPosition)
    {
        _myPosition = myPosition;
        _myPath.Add(myPosition);
        _grid[_myPosition.Y, _myPosition.X] = true;        
    }  

    internal void UpdateEnemyPosition(Point enemyPosition)
    {
        _enemyposition = enemyPosition;
        _grid[_enemyposition.Y, _enemyposition.X] = true;
    }

    internal string GetNextMove()
    {
        var direction = "LEFT";

        List<Point> possibleMoves = _mapChecker.GetAdjacentPoints(_myPosition).Where(p => _mapChecker.IsEmpty(p)).ToList(); 

        List<CandidateMove> candidateMoves = new List<CandidateMove>();
        foreach (var move in possibleMoves)
        {
            candidateMoves.Add(new CandidateMove()
            {
                Move = move
            });
        }

        if (candidateMoves.Count == 0)
        {
            Console.Error.WriteLine("No possible moves, going left by default");
            return "LEFT";
        }

        if (candidateMoves.Count == 1)
        {
            Console.Error.WriteLine("Only one possible move, going " + GetDirection(_myPosition, candidateMoves[0].Move));
            return GetDirection(_myPosition, candidateMoves[0].Move);
        }

        int myCurrentSpace = _floodFill.GetAvailableSpace(_myPosition);
        int opponentCurrentSpace = _floodFill.GetAvailableSpace(_enemyposition);

        if (!_filling)
        {
            // Flood fill all possible spaces. 
            foreach (var candidateMove in candidateMoves)
            {
                candidateMove.MySpace = _floodFill.GetAvailableSpace(candidateMove.Move);
                candidateMove.EnemySpace = _floodFill.GetAvailableSpace(_enemyposition, new List<List<Point>>() { new List<Point> { candidateMove.Move } });
            }

            candidateMoves = candidateMoves.OrderByDescending(cm => cm.MySpace).ToList();

            Console.Error.WriteLine($"MY SPACE: {myCurrentSpace}");
            Logger.CandidateMoves(candidateMoves);

            if (_enemyTrapped && candidateMoves[0].MySpace > candidateMoves[1].MySpace)
            {
                Console.Error.WriteLine("CHOOSING FREEDOM!");

                // TODO: at some point use pathfinding to define this better.
                // If we have different space and we can't touch paths, we are split
                _filling = true;
                return GetDirection(_myPosition, candidateMoves[0].Move);
            }

            candidateMoves = candidateMoves.OrderBy(cm => cm.EnemySpace).ToList();

            Console.Error.WriteLine($"OPPONENT SPACE: {opponentCurrentSpace}");
            Logger.CandidateMoves(candidateMoves);
            if (candidateMoves[0].EnemySpace < candidateMoves[1].EnemySpace
                && candidateMoves[0].EnemySpace < opponentCurrentSpace / 2)
            {
                Console.Error.WriteLine("TRAPPING ENEMY!");
                _enemyTrapped = true;
                return GetDirection(_myPosition, candidateMoves[0].Move);
            }

            foreach (var candidateMove in candidateMoves)
            {
                candidateMove.Beam = _mapChecker.GetBlockingLine(_myPosition, candidateMove.Move);
            }

            // 1. If I beam in any direction can I reduce my opponents space significantly. 
            foreach (var candidateMove in candidateMoves)
            {
                int opponentSpaceAfterBeam = _floodFill.GetAvailableSpace(_enemyposition, new List<List<Point>>() { candidateMove.Beam });
                candidateMove.Score = opponentCurrentSpace - opponentSpaceAfterBeam;
            }

            Logger.CandidateMoves(candidateMoves);

            candidateMoves = candidateMoves.OrderByDescending(cm => cm.Score).ToList();

            Console.Error.WriteLine($"BEAMING!");
            direction = GetDirection(_myPosition, candidateMoves[0].Move);
        }
        else
        {
            int lowestValidMove = int.MaxValue;
            int lowestIndex = 0;

            for (int i = 0; i < candidateMoves.Count; i++)
            {
                var candidateMove = candidateMoves[i];

                int validMoves = _mapChecker.GetValidMoves(candidateMove.Move);
                if (validMoves < lowestValidMove)
                {
                    lowestValidMove = validMoves;
                    lowestIndex = i;
                }
            }
            
            Console.Error.WriteLine($"FILLING!");
            direction = GetDirection(_myPosition, candidateMoves[lowestIndex].Move);
        }

        return direction;

        // 2. If I beam in two directions can I reduce my opponents space significantly.
        //      If so pick the one thats's better and go in that direction.


        // 3. If not pick the direction that gives me the most space and go in that direction.


    }

    private string GetDirection(Point myPosition, Point point)
    {
        if (point.X < myPosition.X)
        {
            return "LEFT";
        }
        else if (point.X > myPosition.X)
        {
            return "RIGHT";
        }
        else if (point.Y < myPosition.Y)
        {
            return "UP";
        }
        else if (point.Y > myPosition.Y)
        {
            return "DOWN";
        }
        return string.Empty;
    }
}
