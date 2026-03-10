using System.Drawing;

namespace _2026_1_WinterChallenge_SnakeByte;

internal sealed class GameState
{
    private Game _game;
   
    private List<SnakeBot> _mySnakes;
    private List<SnakeBot> _opponentSnakes;

    private List<Point> _powerUps;

    private int _turnCount;


    internal GameState(Game game, List<SnakeBot> mySnakes, List<SnakeBot> opponentSnakes, List<Point> powerUps, int turnCount) 
    { 
        // Game is only used for reference. Never change it
        _game = game;            

        // deep copy snakes and powerups so that we can modify them without affecting the original game state
        _mySnakes = mySnakes.Select(s => s.Clone()).ToList();
        _opponentSnakes = opponentSnakes.Select(s => s.Clone()).ToList();
        _powerUps = new List<Point>(powerUps);
        _turnCount = turnCount;
    }

    internal bool IsTerminal()
    {
        if (_turnCount == 200)
        {
            return true;
        }

        if (_mySnakes == null || _mySnakes.Count == 0)
        {
            return true;
        }

        if (_opponentSnakes == null || _opponentSnakes.Count == 0)
        {
            return true;
        }

        if (_powerUps == null || _powerUps.Count == 0)
        {
            return true;
        }

        return false;
    }

    internal int Evaluate()
    {
        // get body count for all snakes
        int myBodyCount = _mySnakes.Sum(s => s.Body.Count);
        int opponentBodyCount = _opponentSnakes.Sum(s => s.Body.Count);

        return myBodyCount - opponentBodyCount;
    }

    // Return all possible move sets for players
    internal List<MoveSet> GetMyMoveSets()
    {
        if (_mySnakes == null || _mySnakes.Count == 0)
        {
            return new List<MoveSet>();
        }

        List<MoveSet> moveSets = GetMoveSets(_mySnakes);

        return moveSets;
    }

    internal List<MoveSet> GetOpponentMoveSets()
    {
        if (_opponentSnakes == null || _opponentSnakes.Count == 0)
        {
            return new List<MoveSet>();
        }

        List<MoveSet> moveSets = GetMoveSets(_opponentSnakes);        

        return moveSets;
    }

    internal List<MoveSet> GetMoveSets(List<SnakeBot> snakes)
    {
        // Get all possible moves for each snake (typically 4 directions)
        var allPossibleMoves = new Dictionary<int, List<string>>();
        foreach (var snake in snakes)
        {
            var validMoves = new List<string>();

            // Each snake can move in 4 directions: Up, Down, Left, Right
            // Check up
            Point pointToCheck = new Point(snake.Body[0].X, snake.Body[0].Y - 1);
            if(!snake.Body.Contains(pointToCheck))
            {
                validMoves.Add("UP");
            }

            // Check down
            pointToCheck = new Point(snake.Body[0].X, snake.Body[0].Y + 1);
            if (!snake.Body.Contains(pointToCheck))
            {
                validMoves.Add("DOWN");
            }

            // Check left
            pointToCheck = new Point(snake.Body[0].X - 1, snake.Body[0].Y);
            if (!snake.Body.Contains(pointToCheck))
            {
                validMoves.Add("LEFT");
            }

            // Check right
            pointToCheck = new Point(snake.Body[0].X + 1, snake.Body[0].Y);
            if (!snake.Body.Contains(pointToCheck))
            {
                validMoves.Add("RIGHT");
            }  

            allPossibleMoves.Add(snake.Id, validMoves);
        }

        // Generate all combinations of moves allPossibleMoves
        var moveSets = new List<MoveSet>();
        GenerateMoveSets(moveSets, allPossibleMoves, new Dictionary<int, string>());

        return moveSets;
    }

    private void GenerateMoveSets(List<MoveSet> moveSets, Dictionary<int, List<string>> movesDict, Dictionary<int, string> currentSet)
    {
        if (currentSet.Count == movesDict.Count)
        {
            List<Move> moveSet = new List<Move>();
            foreach (var current in currentSet) 
            {
                moveSet.Add(new Move(current.Key, current.Value));
            }
            moveSets.Add(new MoveSet(moveSet));
            return;
        }
        var nextSnakeId = movesDict.Keys.Except(currentSet.Keys).First();
        foreach (var move in movesDict[nextSnakeId])
        {
            currentSet[nextSnakeId] = move;
            GenerateMoveSets(moveSets, movesDict, currentSet);
            currentSet.Remove(nextSnakeId);
        }
    }

    internal GameState Simulate(MoveSet myMoveSet, MoveSet opponentMoveSet)
    {
        // apply all moves simultaneously
        // for each move, move the snake in the direction specified by the move
        foreach (var move in myMoveSet.Moves)
        {
            var snake = _mySnakes.First(s => s.Id == move.SnakeId);
            MoveSnake(snake, move.Direction);
        }

        foreach (var move in opponentMoveSet.Moves)
        {
            var snake = _opponentSnakes.First(s => s.Id == move.SnakeId);
            MoveSnake(snake, move.Direction);
        }

        // Check for power up consumption and grow snake if necessary
        foreach (var snake in _mySnakes.Concat(_opponentSnakes))
        {
            if (!_powerUps.Any(p => p.X == snake.Body[0].X && p.Y == snake.Body[0].Y))
            {
                // If it has a power up we add a tail
                // Because we moved it forward without removing the tail this effectively negates it
                snake.Body.RemoveAt(snake.Body.Count - 1);
            }
        }

        // Remove any power ups that are now where a snake head is
        _powerUps.RemoveAll(p => _mySnakes.Any(s => s.Body[0].X == p.X && s.Body[0].Y == p.Y) 
                            || _opponentSnakes.Any(s => s.Body[0].X == p.X && s.Body[0].Y == p.Y));

        // Check for head being destroyed
        // If it's now touching any part of any snake destroy the head
        // If it's now touching any paltform destroy the head (note, if two heads are touching we want to destroy them both, so keep note of these and destroy them all afterwards)
        HashSet<int> toRemoveHeads = new HashSet<int>();
        
        foreach (var snake in _mySnakes.Concat(_opponentSnakes))
        {
            if (_game.IsPlatform(snake.Body[0]))
            {
                snake.Body.RemoveAt(0);
                continue;
            }

            foreach (var otherSnake in _mySnakes.Concat(_opponentSnakes))
            {
                if (snake.Id == otherSnake.Id)
                {
                    // Check the snake but skip the head, as we can be on our own head without it being destroyed
                    if (otherSnake.Body.Skip(1).Any(p => p.X == snake.Body[0].X && p.Y == snake.Body[0].Y))
                    {
                        toRemoveHeads.Add(snake.Id);
                        break;
                    }

                }
                else if (otherSnake.Body.Any(p => p.X == snake.Body[0].X && p.Y == snake.Body[0].Y))
                {
                    toRemoveHeads.Add(snake.Id);
                    break;
                }
            }
        }

        foreach (var snake in _mySnakes.Concat(_opponentSnakes))
        {
            if (toRemoveHeads.Contains(snake.Id))
            {
                snake.Body.RemoveAt(0);
            }
        }

        // Check for destroyed snakes
        // If they have less than 3 parts, they are destroyed and removed from the game state
        // If they are wholly off the map, they are destroyed and removed from the game state
        for (int i = _mySnakes.Count - 1; i >= 0; --i)
        {
            if (_mySnakes[i].Body.Count < 3)
            {
                Console.Error.WriteLine($"Removing snake {i} with body count {_mySnakes[i].Body.Count}");
                _mySnakes.RemoveAt(i);
            }
            else if (_mySnakes[i].Body.All(p => p.X < 0 || p.X >= _game.Width || p.Y < 0 || p.Y >= _game.Height))
            {
                Console.Error.WriteLine($"Removing snake {i} that is off the map");
                _mySnakes.RemoveAt(i);
            }
        }

        for (int i = _opponentSnakes.Count - 1; i >= 0; --i)
        {
            if (_opponentSnakes[i].Body.Count < 3)
            {
                _opponentSnakes.RemoveAt(i);
            }
            else if (_opponentSnakes[i].Body.All(p => p.X < 0 || p.X >= _game.Width || p.Y < 0 || p.Y >= _game.Height))
            {
                _opponentSnakes.RemoveAt(i);
            }
        }

        // First, order all snakes by lowest point on the snake, so that we apply gravity to the lowest snakes first
        var allSnakes = _mySnakes.Concat(_opponentSnakes).OrderBy(s => s.Body.Max(p => p.Y)).ToList();

        // debug log all snakes before gravity
        Console.Error.WriteLine("Map before gravity");
        Logger.EntireGame(_game.GetPlatforms(), _mySnakes, _opponentSnakes, _powerUps);

        // Apply gravity
        // Power ups count as platforms, as do other snakes
        foreach (var snake in allSnakes)
        {
            Console.Error.WriteLine($"Applying gravity to snake {snake.Id}");
            bool canMoveDown = true;
            while (canMoveDown)
            {
                // Check if we can move down
                if (snake.Body.Any(p => _game.IsPlatform(new Point(p.X, p.Y + 1)) 
                                   || _mySnakes.Any(s => s.Body.Any(bp => bp.X == p.X && bp.Y == p.Y + 1)) 
                                   || _opponentSnakes.Any(s => s.Body.Any(bp => bp.X == p.X && bp.Y == p.Y + 1))
                                   || _powerUps.Any(pu => pu.X == p.X && pu.Y == p.Y + 1)))
                {
                    canMoveDown = false;
                    Console.Error.WriteLine($"Snake cannot move down due to an obstacle");

                }
                else
                {
                    Console.Error.WriteLine("Moving snale down by 1");
                    // Move the snake down by one
                    for (int i = 0; i < snake.Body.Count; ++i)
                    {
                        snake.Body[i] = new Point(snake.Body[i].X, snake.Body[i].Y + 1);
                    }
                }
            }
        }

        Console.Error.WriteLine("Map after gravity");
        Logger.EntireGame(_game.GetPlatforms(), _mySnakes, _opponentSnakes, _powerUps);

        return new GameState(_game, _mySnakes, _opponentSnakes, _powerUps, _turnCount);
    }

    private void MoveSnake(SnakeBot snake, string direction)
    {
        Point newHead = new Point(snake.Body[0].X, snake.Body[0].Y);
        switch (direction)
        {
            case "UP":
                newHead.Y -= 1;
                break;
            case "DOWN":
                newHead.Y += 1;
                break;
            case "LEFT":
                newHead.X -= 1;
                break;
            case "RIGHT":
                newHead.X += 1;
                break;
        }

        // Move the snake by adding the new head and removing the tail
        snake.Body.Insert(0, newHead);

        // Don't remove it's tail yet, if we need to add one it'll go here
    }
}