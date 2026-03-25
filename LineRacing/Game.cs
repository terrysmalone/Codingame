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

    private LightCycle _myLightCycle;
    private List<LightCycle> _enemyLightCycles;

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

        _enemyLightCycles = new List<LightCycle>();
    }

    internal void UpdateMyPosition(Point myPosition)
    {
        _myLightCycle.CurrentPosition = myPosition;
        _myLightCycle.Path.Add(myPosition);

        _grid[myPosition.Y, myPosition.X] = true;        
    }  

    internal void UpdateEnemyPosition(Point enemyStartPosition, Point enemyPosition)
    {
        var enemy = _enemyLightCycles.FirstOrDefault(e => e.StartPosition == enemyStartPosition);

        if (enemy == null)
        {
            Console.Error.WriteLine("ERROR: No enemy found with start position: " + enemyStartPosition);
            return;
        }

        enemy.CurrentPosition = enemyPosition;
        _grid[enemyPosition.Y, enemyPosition.X] = true;
    }

    internal string GetNextMove()
    {
        Point currentPosition = _myLightCycle.CurrentPosition;
        var direction = "LEFT";

        List<Point> possibleMoves = _mapChecker.GetAdjacentPoints(currentPosition).Where(p => _mapChecker.IsEmpty(p)).ToList(); 

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
            Console.Error.WriteLine("Only one possible move, going " + GetDirection(currentPosition, candidateMoves[0].Move));
            return GetDirection(currentPosition, candidateMoves[0].Move);
        }

        int myCurrentSpace = _floodFill.GetAvailableSpace(currentPosition);
        int opponentCurrentSpace = _floodFill.GetAvailableSpace(_enemyLightCycles[0].CurrentPosition);

        if (!_filling)
        {
            // Flood fill all possible spaces. 
            foreach (var candidateMove in candidateMoves)
            {
                candidateMove.MySpace = _floodFill.GetAvailableSpace(candidateMove.Move);
                candidateMove.EnemySpace = _floodFill.GetAvailableSpace(_enemyLightCycles[0].CurrentPosition, new List<List<Point>>() { new List<Point> { candidateMove.Move } });
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
                return GetDirection(currentPosition, candidateMoves[0].Move);
            }

            candidateMoves = candidateMoves.OrderBy(cm => cm.EnemySpace).ToList();

            Console.Error.WriteLine($"OPPONENT SPACE: {opponentCurrentSpace}");
            Logger.CandidateMoves(candidateMoves);
            if (candidateMoves[0].EnemySpace < candidateMoves[1].EnemySpace
                && candidateMoves[0].EnemySpace < opponentCurrentSpace / 2)
            {
                Console.Error.WriteLine("TRAPPING ENEMY!");
                _enemyTrapped = true;
                return GetDirection(currentPosition, candidateMoves[0].Move);
            }

            foreach (var candidateMove in candidateMoves)
            {
                candidateMove.Beam = _mapChecker.GetBlockingLine(currentPosition, candidateMove.Move);
            }

            // 1. If I beam in any direction can I reduce my opponents space significantly. 
            foreach (var candidateMove in candidateMoves)
            {
                int opponentSpaceAfterBeam = _floodFill.GetAvailableSpace(_enemyLightCycles[0].CurrentPosition, new List<List<Point>>() { candidateMove.Beam });
                candidateMove.Score = opponentCurrentSpace - opponentSpaceAfterBeam;
            }

            Logger.CandidateMoves(candidateMoves);

            candidateMoves = candidateMoves.OrderByDescending(cm => cm.Score).ToList();

            Console.Error.WriteLine($"BEAMING!");
            direction = GetDirection(currentPosition, candidateMoves[0].Move);
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
            direction = GetDirection(currentPosition, candidateMoves[lowestIndex].Move);
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

    internal void InitialiseMyLightCycle(Point playerStartPosition)
    {
        _myLightCycle = new LightCycle()
        {
            StartPosition = playerStartPosition,
        };

        _myLightCycle.Path.Add(playerStartPosition);
    }

    internal void InitialiseEnemyLightCycle(Point enemyStartPosition)
    {
        if (_enemyLightCycles.Any(e => e.StartPosition == enemyStartPosition))
        {
            Console.Error.WriteLine("ERROR: Enemy already exists with start position: " + enemyStartPosition);
        }
        else
        {
            var lightCycle = new LightCycle()
            {
                StartPosition = enemyStartPosition
            };

            lightCycle.Path.Add(enemyStartPosition);

            _enemyLightCycles.Add(lightCycle);           
        }
    }

    internal void DestroyEnemy(Point enemyStartPosition)
    {
        _enemyLightCycles.Remove(_enemyLightCycles.First(e => e.StartPosition == enemyStartPosition));
    }
}
