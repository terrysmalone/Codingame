using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Xml.Linq;
using System.Xml.Schema;

namespace _2026_1_WinterChallenge_SnakeByte;

internal class Game
{
    private bool FEATURE_POWER_CLUSTERING_ON = true;
    private bool FEATURE_ENCOURAGE_SPREADING_OUT_ON = true;
    private bool FEATURE_INCREASE_EXCLUDE_MOVES_ON = true;
    private bool FEATURE_BULLYING_ON = true;

    internal int Width { get; private set; }
    internal int Height { get; private set; }   

    internal List<SnakeBot> MySnakeBots { get; set; }
    internal List<SnakeBot> OpponentSnakeBots { get; set; }

    private Level _level;

    private PathFinder _pathFinder;
    private PositionChecker _positionChecker;
    private MovementHelper _movementHelper;
    private MinimaxSearch _minimax;

    private Dictionary<Point, int> _closestSnakeToPowerSourceMap = new Dictionary<Point, int>();

    private HashSet<Point> _powerUpPoints;
    private HashSet<Point> _solidPoints;

    private const int BASE_POWER_SCORE = 100000;
    private const int PATH_LENGTH_PENALTY = 100;
    private const int BASE_CLIMBABLE_LEDGE_SCORE = 5500;
    private const int BASE_WANDER_SCORE = 5000;
    private const int BASE_CRITICAL_MOVE_SCORE = 1000000;
    private const int CLOSER_TO_POWER_SCORE = 500;

    private const int EMERGENCY_BASE_BULLY_SCORE = 50000;
    private const int BASE_BULLY_SCORE = 5500;

    public Game(int width, int height, bool[,] platforms)
    {
        Width = width;
        Height = height;

        _level = new Level(width, height, platforms);
                
        _positionChecker = new PositionChecker(this, _level);
        _movementHelper = new MovementHelper();
        _pathFinder = new PathFinder(this, _positionChecker, _movementHelper);
        _minimax = new MinimaxSearch(width, height, _level.GetAllPlatformPositions());

        MySnakeBots = new List<SnakeBot>();
        OpponentSnakeBots = new List<SnakeBot>();
    }

    internal void AddMySnake(SnakeBot snakeBot)
    {
        MySnakeBots.Add(snakeBot);
    }

    internal void AddOpponentSnake(SnakeBot snakeBot)
    {
        OpponentSnakeBots.Add(snakeBot);
    }

    internal void MarkAllSnakesForRemoval()
    {
       foreach (var snakeBot in MySnakeBots)
        {
            snakeBot.Remove = true;
        }

        foreach (var snakeBot in OpponentSnakeBots)
        {
            snakeBot.Remove = true;
        }
    }

    internal void RemoveMarkedSnakes()
    {
        // Iterate through the snakes backwards to safely remove any where Remove == true
        for (int i = MySnakeBots.Count - 1; i >= 0; --i)
        {
            if (MySnakeBots[i].Remove)
            {
                MySnakeBots.RemoveAt(i);
            }
        }

        for (int i = OpponentSnakeBots.Count - 1; i >= 0; --i)
        {
            if (OpponentSnakeBots[i].Remove)
            {
                OpponentSnakeBots.RemoveAt(i);
            }
        }
    }

    internal SnakeBot GetSnake(int snakebotId)
    {
        return MySnakeBots.FirstOrDefault(s => s.Id == snakebotId) ?? OpponentSnakeBots.FirstOrDefault(s => s.Id == snakebotId);
    }

    internal void AddPowerSource(int x, int y)
    {
        _level.PowerSources.Add(new Point(x, y));
    }

    internal void RemoveAllPowerSources()
    {
        _level.PowerSources.Clear();
    }    

    internal List<string> GetActions()
    {
        List<Plan> plans = new List<Plan>();

        // Create groupings
        List<HashSet<int>> minimaxGroups = CreateMinimaxGroupings();
        Logger.AssignedMinimaxGroups(minimaxGroups);

        Logger.LogTime("Created minimax groupings");

        foreach (HashSet<int> group in minimaxGroups)
        {
            var mine = MySnakeBots.Where(s => group.Contains(s.Id)).ToList();
            var opponents = OpponentSnakeBots.Where(s => group.Contains(s.Id)).ToList();

            // TODO: We'll want to set the depth depending on how many snakes are being checked
            int minimaxDepth = 2;

            plans.AddRange(GetMinimaxMoves(mine, opponents, _level.PowerSources, minimaxDepth));
            Logger.LogTime($"Finished minimax for group with snakes {string.Join(",", group)}");
        }

        List<string> actions = new List<string>();
        actions.AddRange(ConvertToActions(plans));
        return actions;






        var bullyMode = false;
        if (_level.PowerSources.Count == 1 && GetMyScore() - GetEnemyScore() < 0)
        {
            bullyMode = true;
        }

        _closestSnakeToPowerSourceMap = _positionChecker.GetClosestPowerSourceToOpponentSnakeMap();

        // Calculate points we use for collision detection and gravity, then we can use it for every search
        // Note: This doesn't include the current snake body since that will be moving as we simulate movement
        _powerUpPoints = _level.PowerSources;

        foreach (var snakeBot in MySnakeBots)
        {
            snakeBot.ClearAllPlans();
        }

        foreach (var snakeBot in MySnakeBots)
        {
            _solidPoints = BuildSolidPoints(snakeBot.Id, _powerUpPoints);
            // We track power sources we've tried to get to so that we don't keep trying at different depths
            snakeBot.ClearCheckedPowerSources();

            Logger.LogTime($"STARTING FOR SNAKEBOT {snakeBot.Id}. Position:{snakeBot.Body[0].X},{snakeBot.Body[0].Y}");
            
            // ADD DANGER MOVES
            // For each possible direction, make the immediate move, then do a flood fill to see if we're in immediate danger
            // For now just check that we're not instantly blocked (i.e. Nowhere to move next turn)
            List<Plan> blockingPlans = GetBlockingPlans(snakeBot);
            snakeBot.AddPlans(blockingPlans);
            Logger.LogTime($"Added {blockingPlans.Count} blocking move plans");

            HashSet<Point> excludePoints = new HashSet<Point>();

            // If any surrounding points are in danger of the enemy attacking them add to exclude points
            var possibleMoves = new List<Point>()
            {
                new Point(snakeBot.Body[0].X + 1, snakeBot.Body[0].Y),
                new Point(snakeBot.Body[0].X - 1, snakeBot.Body[0].Y),
                new Point(snakeBot.Body[0].X, snakeBot.Body[0].Y + 1),
                new Point(snakeBot.Body[0].X, snakeBot.Body[0].Y - 1)
            };

            var goldenMovesAdded = 0;

            foreach (var possibleMove in possibleMoves)
            {
                (Plan? headClashMove, bool excludeMove) = GetCriticalHeadClashMove(possibleMove, snakeBot);

                if (headClashMove != null)
                {
                    Logger.Plans("Added head clash move", new List<Plan> { headClashMove });
                    snakeBot.AddPlan(headClashMove);
                    goldenMovesAdded++;
                    continue;
                }
                
                if (excludeMove)
                {
                    excludePoints.Add(possibleMove);
                }
            }

            Logger.LogTime($"Added {goldenMovesAdded} head clash plans");            

            if (snakeBot.IsStuck())
            {
                excludePoints.Add(snakeBot.GetLastMove());
            }

            if (FEATURE_BULLYING_ON)
            {
                List<Plan> bullyPlans = GetBullyPlans(snakeBot, bullyMode, excludePoints);
                UpdateScores(bullyPlans, snakeBot);
                snakeBot.AddPlans(bullyPlans);

                // If bully mode is on and we found a plan don't bother looking for more
                if (bullyMode && bullyPlans.Count > 0)
                {
                    continue;
                }
            }

            // Don't go for power source if it's the last one and I'm behind
            if (!bullyMode)
            {
                List<Plan> bestPlansToPowerSources = GetBestPlansToPowerSources(snakeBot, excludePoints);
                UpdateScores(bestPlansToPowerSources, snakeBot);

                snakeBot.AddPlans(bestPlansToPowerSources);
            }

            List<Plan> climbableLedgePlans = GetClimbableLedgePlans(snakeBot, excludePoints);
            UpdateScores(climbableLedgePlans, snakeBot);
            snakeBot.AddPlans(climbableLedgePlans);

            List<Plan> validPlans = GetValidDirectionPointPlans(snakeBot);
            snakeBot.AddPlans(validPlans);
               
            Logger.LogTime($"Finished checking valid directions. Added {validPlans.Count} plans");
        }

        // If any snake has a terrible move (usually meaning it would lower my score)
        // give the same score to any other moves for that snake that has the same first position
        foreach (var snakeBot in MySnakeBots)
        {
            var terribleMoves = snakeBot.GetPlans().Where(p => p.Score < -BASE_CRITICAL_MOVE_SCORE + 10000).ToList();

            foreach (Plan terribleMove in terribleMoves)
            {
                foreach (Plan plan in snakeBot.GetPlans())
                {
                    if (plan.Moves[0] == terribleMove.Moves[0])
                    {
                        plan.Score = terribleMove.Score;
                    }
                }
            }

            Logger.Plans($"Plans for snake {snakeBot.Id}", snakeBot.GetPlans());
        }

        // Get all combinations of all plans. Score them by adding the scores together. Pick the highest scoring combination that doesn't have any clashes.
        Dictionary<List<Plan>, int> planCombinations = GetAllPlanCombinations();
        
        // Logger.PlanCombinations(planCombinations);

        Logger.LogTime($"Got all plan combinations: {planCombinations.Count}");

        // Get the first combination that doesn't have any clashes for the next move
        foreach (var planCombination in planCombinations.OrderByDescending(pc => pc.Value))
        {
            HashSet<Point> plannedStartMoves = new HashSet<Point>();
            Dictionary<Point, int> plannedEndMoves = new Dictionary<Point, int>();
            bool clash = false;

            // TODO: At some point we might want to consider amount
            // For example, if there are 2  power ups, we shouldn't go for both this turn
            Point? lastPowerSource = null;

            if (bullyMode)
            {
                lastPowerSource = _level.PowerSources.First();
            }


            foreach (var plan in planCombination.Key)
            {
                Point endMovePoint = plan.Moves[plan.Moves.Count - 1];

                var endMoveClash = false;

                foreach (var plannedEndMove in plannedEndMoves)
                {
                    if (plannedEndMove.Key == endMovePoint
                        && plannedEndMove.Value < 5
                        && plan.Moves.Count < 5)
                    {
                        endMoveClash = true;
                    }
                }



                if (plannedStartMoves.Contains(plan.Moves[0])
                    || endMoveClash
                    || lastPowerSource == plan.Moves[0])
                {
                    clash = true;
                }
                else
                {
                    plannedStartMoves.Add(plan.Moves[0]);

                    if (plannedEndMoves.ContainsKey(endMovePoint))
                    {
                        plannedEndMoves[endMovePoint] = Math.Max(plannedEndMoves[endMovePoint], plan.Moves.Count);
                    }
                    else
                    {
                        plannedEndMoves.Add(endMovePoint, plan.Moves.Count);
                    }
                }
            }

            if (!clash)
            {
                Logger.LogTime($"Chose plan combination with score {planCombination.Value}");

                foreach (Plan? plan in planCombination.Key)
                {
                    var snakeBot = MySnakeBots.First(s => s.Id == plan.SnakeID);

                    string direction = DirectionHelper.GetDirection(snakeBot.Body[0], plan.Moves[0]);
                    
                    actions.Add($"{snakeBot.Id} {direction} {plan.PlanType}");

                    snakeBot.AddMove(plan.Moves[0]);

                    Point planTarget = plan.Moves[plan.Moves.Count - 1];
                    if (!_positionChecker.IsOutOfMapBounds(planTarget))
                    {
                        actions.Add($"MARK {planTarget.X} {planTarget.Y}");
                    }

                    Logger.Message($"Chose plan {plan.PlanType} with score {plan.Score} and direction {direction}");

                }
                break;
            }
        }

        if (actions.Count == 0)
        {
            actions.Add("WAIT");
        }

        return actions;
    }

    private List<HashSet<int>> CreateMinimaxGroupings()
    {
        int minimumDistance = 3;

        List<HashSet<int>> minimaxGroups = new List<HashSet<int>>();

        // First make groupings of just my snakes that are close to each other
        foreach (SnakeBot snake in MySnakeBots)
        {
            HashSet<int> group = new HashSet<int>();
            group.Add(snake.Id);

            foreach (SnakeBot otherSnake in MySnakeBots)
            {
                if (snake.Id == otherSnake.Id)
                {
                    continue;
                }

                int distance = GetClosestBodyPointDistance(snake, otherSnake);

                if (distance <= minimumDistance)
                {
                    group.Add(otherSnake.Id);
                }
            }        

            minimaxGroups.Add(group);
        }

        // Merge groups that have the same ids in them
        for (int i = 0; i < minimaxGroups.Count; i++)
        {
            for (int j = i + 1; j < minimaxGroups.Count; j++)
            {
                if (minimaxGroups[i].Overlaps(minimaxGroups[j]))
                {
                    minimaxGroups[i].UnionWith(minimaxGroups[j]);
                    minimaxGroups.RemoveAt(j);
                    j--;
                }
            }
        }

        // Add any opponent snakes that are close to any of my snakes in the groups
        foreach (HashSet<int> group in minimaxGroups)
        {
            foreach (int snakeId in group.ToList())
            {
                SnakeBot snake = GetSnake(snakeId);
                foreach (SnakeBot opponentSnake in OpponentSnakeBots)
                {
                    int distance = GetClosestBodyPointDistance(snake, opponentSnake);
                    if (distance <= minimumDistance)
                    {
                        group.Add(opponentSnake.Id);
                    }
                }
            }
        }

        return minimaxGroups;
    }

    private int GetClosestBodyPointDistance(SnakeBot snake, SnakeBot otherSnake)
    {
        int minDistance = int.MaxValue;

        foreach (Point bodyPoint in snake.Body)
        {
            foreach (Point otherBodyPoint in otherSnake.Body)
            {
                int distance = CalculationUtil.GetManhattanDistance(bodyPoint, otherBodyPoint);
                if (distance < minDistance)
                {
                    minDistance = distance;
                }
            }
        }

        return minDistance;
    }

    private IEnumerable<string> ConvertToActions(List<Plan> plans)
    {
        List<string> actions = new List<string>();

        foreach (Plan plan in plans)
        {
            actions.Add($"{plan.SnakeID} {DirectionHelper.GetDirection(GetSnake(plan.SnakeID).Body[0], plan.Moves[plan.Moves.Count - 1])} {plan.PlanType}");

            if (Logger.IsLoggingEnabled())
            { 
                actions.Add($"MARK {plan.Moves[plan.Moves.Count - 1].X} {plan.Moves[plan.Moves.Count - 1].Y}");
            }
        }

        return actions;
    }

    private List<Plan> GetMinimaxMoves(List<SnakeBot> mine, List<SnakeBot> opponents, HashSet<Point> powerSources, int minimaxDepth)
    {
        var minimaxResult = _minimax.GetBestMoves(mine, opponents, _level.PowerSources, minimaxDepth);

        if (minimaxResult.BestMoves.Count > 0)
        {
            List<Plan> minimaxPlans = new List<Plan>();

            foreach (var kvp in minimaxResult.BestMoves)
            {
                var snakeBot = MySnakeBots.FirstOrDefault(s => s.Id == kvp.Key);
                if (snakeBot == null) continue;

                minimaxPlans.Add(new Plan(new List<Point> { kvp.Value }, minimaxResult.Score, minimaxResult.Score.ToString(), turnsToFruition: 1, snakeBot.Id));
                snakeBot.AddMove(kvp.Value);
            }

            if (minimaxPlans.Count > 0)
            {
                Logger.LogTime($"Minimax chose moves with score {minimaxResult.Score}");
                return minimaxPlans;
            }
        }

        return new List<Plan>();
    }

    private List<Plan> GetBullyPlans(SnakeBot snakeBot, bool bullyMode, HashSet<Point> excludePoints)
    {
        int bullyScore = bullyMode ? EMERGENCY_BASE_BULLY_SCORE : BASE_BULLY_SCORE;
        List<Plan> bullyPlans = new List<Plan>();

        List<SnakeBot> smallerEnemySnakes = OpponentSnakeBots.Where(s => s.Body.Count < snakeBot.Body.Count).ToList();

        // Order smallerEnemySnakes by closest to snakeBot
        smallerEnemySnakes = smallerEnemySnakes.OrderBy(s => CalculationUtil.GetManhattanDistance(s.Body[0], snakeBot.Body[0])).ToList();

        int foundBullyMoves = 0;
        foreach (var smallerEnemySnake in smallerEnemySnakes)
        {
            List<Point> path = _pathFinder.GetShortestPath(snakeBot.Body[0], smallerEnemySnake.Body[0], snakeBot, excludePoints.ToList(), _solidPoints, _powerUpPoints);

            if (path.Count > 0)
            {
                // They'll have moved, it's suicide
                if (path.Count == 1)
                {
                    continue;
                }

                var score = bullyScore;
                if (path.Count < 5)
                {
                    score = bullyScore * 3;
                }

                Plan bullyPlan = new Plan(path, score - (path.Count * PATH_LENGTH_PENALTY), "bully", turnsToFruition: path.Count, snakeBot.Id);
                bullyPlans.Add(bullyPlan);
                foundBullyMoves++;
            }
        }

        return bullyPlans;
    }

    private HashSet<Point> BuildSolidPoints(int excludeSnakeId, HashSet<Point> powerUpPoints)
    {
        HashSet<Point> collisionPoints = new HashSet<Point>();

        foreach (var snake in MySnakeBots)
        {
            if (snake.Id == excludeSnakeId)
            {
                continue;
            }
            foreach (var bodyPart in snake.Body)
            {
                collisionPoints.Add(bodyPart);
            }
        }

        foreach (var snake in OpponentSnakeBots)
        {
            foreach (var bodyPart in snake.Body)
            {
                collisionPoints.Add(bodyPart);
            }
        }

        foreach (var platform in _level.GetAllPlatformPositions())
        {
            collisionPoints.Add(platform);
        }

        foreach (var powerUp in powerUpPoints)
        {
            collisionPoints.Add(powerUp);
        }

        return collisionPoints;
    }

    private List<Plan> GetBlockingPlans(SnakeBot snake)
    {
        var plans = new List<Plan>();

        var possibleHeadMoves = new List<Point>()
            {
                new Point(snake.Body[0].X + 1, snake.Body[0].Y),
                new Point(snake.Body[0].X - 1, snake.Body[0].Y),
                new Point(snake.Body[0].X, snake.Body[0].Y + 1),
                new Point(snake.Body[0].X, snake.Body[0].Y - 1)
            };

        foreach (Point possibleMove in possibleHeadMoves)
        {
            if (_positionChecker.IsPointInGivenSnake(snake.Body, possibleMove, countTails: false)
                || _positionChecker.IsPlatform(possibleMove)
                || _positionChecker.IsPointInAnySnake(possibleMove, countTails: false))
            {
                continue;
            }       

            // Make the move
            List<Point> movedBody = _movementHelper.SimulateSnakeMovement(snake.Body, snake.Body[0], possibleMove, _level.PowerSources);

            // Simulate gravity
            List<Point> afterGravityBody = _movementHelper.ApplyGravity(movedBody, _solidPoints);

            // Very small flood fill to check if I'm insta blocked
            int space = _positionChecker.FloodFillCount(afterGravityBody[0], snake.Id, afterGravityBody, 5, includeSelf: false);

            if (space < 2)
            {
                Logger.Message($"Added blocking move plan for snake {snake.Id} at position {possibleMove.X},{possibleMove.Y}");
                Logger.Message($"After simulating the move and gravity, the snake body would be at: {string.Join(";", afterGravityBody.Select(p => $"{p.X},{p.Y}"))}");
                plans.Add(new Plan(new List<Point> { possibleMove }, -BASE_CRITICAL_MOVE_SCORE, "blocking move", turnsToFruition: 1, snake.Id));
            }
        }

        return plans;
    }

    private List<Plan> GetClimbableLedgePlans(SnakeBot snakeBot, HashSet<Point> excludePoints)
    {
        // Exclude blocked ledges
        var possibleHeadMoves = new List<Point>()
            {
                new Point(snakeBot.Body[0].X + 1, snakeBot.Body[0].Y),
                new Point(snakeBot.Body[0].X - 1, snakeBot.Body[0].Y),
                new Point(snakeBot.Body[0].X, snakeBot.Body[0].Y + 1),
                new Point(snakeBot.Body[0].X, snakeBot.Body[0].Y - 1)
            };

        foreach (Point possibleMove in possibleHeadMoves)
        {
            // Before wandering randomly, try to get some paths to nearby platforms that are above me
            if (_positionChecker.IsBlocking(possibleMove, snakeBot))
            {
                excludePoints.Add(possibleMove);
            }
        }

        List<Plan> plans = new List<Plan>();
        var shortestPathPoints = new List<Point>();

        //bool stopLooking = false;
        int maxDistance = 5;

        plans = GetShortestPathToClimbableLedgePlans(snakeBot, maxDistance, excludePoints);

        return plans;
    }

    private List<Plan> GetShortestPathToClimbableLedgePlans(SnakeBot snakeBot, int maxDistance, HashSet<Point> excludePoints)
    {
        List<Plan> plans = new List<Plan>();

        // Get lowest point on SnakeBot body
        var lowestSnakePoint = snakeBot.Body.OrderByDescending(p => p.Y).First();

        foreach (Point ledge in _level.GetWalkableLedges())
        {
            if(ledge.Y >= lowestSnakePoint.Y)
            {
                continue;
            }

            // Don't bother trying if it's further away than maxDistance
            int manhattanDistance = CalculationUtil.GetManhattanDistance(snakeBot.Body[0], ledge);
            if (manhattanDistance >= maxDistance)
            {
                continue;
            }

            // Don't try if something is on the ledge
            if(_positionChecker.IsPointInAnySnake(ledge, countTails: false))
            {
                continue;
            }

            List<Point> path = _pathFinder.GetShortestPath(snakeBot.Body.First(), ledge, snakeBot, excludePoints.ToList(), _solidPoints, _powerUpPoints);

            if (path?.Count > 0)
            {
                // Create a plan for this path
                int score = BASE_CLIMBABLE_LEDGE_SCORE;

                plans.Add(new Plan(path, score, "climbing", turnsToFruition: path.Count, snakeBot.Id));
                
                if (maxDistance >= 10)
                {
                    // Exit early. Paths of 10 can be expensive
                    break;
                }
            }
        }

        Logger.LogTime($"Finished looking for ledges with max distance {maxDistance}");

        return plans;
    }

    private Dictionary<List<Plan>, int> GetAllPlanCombinations()
    {
        var allSnakePlans = MySnakeBots.Select(s => s.GetPlans())
                                       .Where(plans => plans.Count > 0)
                                       .ToList();

        var planCombinations = new Dictionary<List<Plan>, int>();

        if (allSnakePlans.Count == 0)
        {
            return planCombinations;
        }

        GetAllPlanCombinationsRecursive(allSnakePlans, 0, new List<Plan>(), planCombinations);

        return planCombinations;
    }

    private void GetAllPlanCombinationsRecursive(List<List<Plan>> allSnakePlans, int snakeIndex, List<Plan> plans, Dictionary<List<Plan>, int> planCombinations)
    {
        if (snakeIndex == allSnakePlans.Count)
        {
            int score = plans.Sum(p => p.Score);
            planCombinations.Add(plans.ToList(), score);
            return;
        }
        foreach (var plan in allSnakePlans[snakeIndex])
        {
            plans.Add(plan);
            GetAllPlanCombinationsRecursive(allSnakePlans, snakeIndex + 1, plans, planCombinations);
            plans.RemoveAt(plans.Count - 1);
        }
    }

    private List<Plan> GetBestPlansToPowerSources(SnakeBot snakeBot, HashSet<Point> excludePoints)
    {
        List<Plan> plans = new List<Plan>();
        var shortestPathPoints = new List<Point>();

        // Use an iterative deepening approach to finding targets
        bool stopLooking = false;
        int maxDistance = 6;

        while (stopLooking == false)
        {
            plans = GetShortestPathToPowerSourcePlans(snakeBot, maxDistance, excludePoints);

            if (plans.Count > 0)
            {
                stopLooking = true;
            }

            maxDistance += 2;
            if (maxDistance > 10)
            {
                stopLooking = true;
            }
        }

        return plans;
    }

    private List<Plan> GetValidDirectionPointPlans(SnakeBot snakeBot)
    {        
        // Prioritise moving towards the nearest powersource
        // TODO: We should also prioritise climbing. For example, if I'm below a platform and I can get up there,
        // I should prioritise that as it opens up the map and gives more options.
        Point nearestPowerSource = GetNearestPowerSource(snakeBot);

        List<string> possibleDirections = nearestPowerSource.X > snakeBot.Body[0].X ? new List<string>() { "RIGHT", "UP", "DOWN", "LEFT" } 
                                                                                    : new List<string>() { "LEFT", "UP", "DOWN", "RIGHT" };

        Dictionary<Point, int> directionScores = new Dictionary<Point, int>();

        foreach (string direction in possibleDirections)
        {
            Point checkPoint = DirectionHelper.GetNewPosition(snakeBot.Body[0], direction);
            directionScores.Add(checkPoint, BASE_WANDER_SCORE);
        }

        RemoveAllHardNos(directionScores, snakeBot);

        if (directionScores.Count == 0)
        {
            return new List<Plan>();
        }

        UpdateScores(directionScores, snakeBot);
         
        // Make plans from the direction scores

        List<Plan> plans = new List<Plan>();

        foreach (var directionScore in directionScores)
        {
            plans.Add(new Plan(new List<Point> { directionScore.Key }, directionScore.Value, "wander", turnsToFruition: 1, snakeBot.Id));
        }

        return plans;
    }

    private void UpdateScores(List<Plan> plans, SnakeBot snakeBot)
    {
        
        foreach (Plan plan in plans)
        {
            int scoreChange = 0;
            Point newHeadPosition = plan.Moves[0];
            Point targetPoint = plan.Moves[plan.Moves.Count - 1];

            scoreChange += ScoreChangeForOtherSnakeBodyPositions(newHeadPosition, snakeBot);
            scoreChange += ScoreChangeForSpaceCreated(newHeadPosition, snakeBot);
            scoreChange += ScoreChangeForStuckDirections(newHeadPosition, snakeBot);
            scoreChange += ScoreChangeForPosition(newHeadPosition, snakeBot);

            if (FEATURE_POWER_CLUSTERING_ON)
            {
                scoreChange += ScoreChangeForPowerSourceClustering(targetPoint);
            }

            if (FEATURE_ENCOURAGE_SPREADING_OUT_ON)
            {
                scoreChange += ScoreChangeForSpreading(targetPoint, snakeBot);
            }

            plan.Score = plan.Score += scoreChange;
        }
    }

    private int ScoreChangeForPowerSourceClustering(Point targetPoint)
    {
        int xMin = Math.Max(0, targetPoint.X - 1);
        int xMax = Math.Min(Width - 1, targetPoint.X + 1);
        int yMin = Math.Max(0, targetPoint.Y - 1);
        int yMax = Math.Min(Height - 1, targetPoint.Y + 1);

        int scoreChange = 0;

        for (int y = yMin; y <= yMax; y++)
        {
            for (int x = xMin; x <= xMax; x++)
            {
                if (_powerUpPoints.Contains(new Point(x, y)))
                {
                    scoreChange += 100;
                }
            }
        }

        return scoreChange;
    }

    private void UpdateScores(Dictionary<Point, int> directionScores, SnakeBot snakeBot)
    {
        foreach (var direction in directionScores)
        {
            int scoreChange = 0;
            Point newHeadPosition = direction.Key;

            scoreChange += ScoreChangeForOtherSnakeBodyPositions(newHeadPosition, snakeBot);
            scoreChange +=  ScoreChangeForSpaceCreated(newHeadPosition, snakeBot);
            scoreChange += ScoreChangeForStuckDirections(newHeadPosition, snakeBot);
            scoreChange += ScoreChangeForPosition(newHeadPosition, snakeBot);

            if (FEATURE_ENCOURAGE_SPREADING_OUT_ON)
            {
                scoreChange += ScoreChangeForSpreading(newHeadPosition, snakeBot);
            }

            directionScores[direction.Key] = directionScores[direction.Key] += scoreChange;
        }
    }

    private int ScoreChangeForSpreading(Point newHeadPosition, SnakeBot snakeBot)
    {
        // We don't want to encourage spreading out of the map
        if (_positionChecker.IsOutOfMapBounds(newHeadPosition))
        {
            return 0;
        }

        int currentDistanceToClosestAlly = MySnakeBots.Where(s => s.Id != snakeBot.Id)
                                                      .Select(s => CalculationUtil.GetManhattanDistance(snakeBot.Body[0], s.Body[0]))
                                                      .DefaultIfEmpty(0)
                                                      .Min();

        int newDistanceToClosestAlly = MySnakeBots.Where(s => s.Id != snakeBot.Id)
                                                  .Select(s => CalculationUtil.GetManhattanDistance(newHeadPosition, s.Body[0]))
                                                  .DefaultIfEmpty(0)
                                                  .Min();

        // TODO: At some point we should maybe score more based on the distance 
        int scoreChange = 0;

        if (newDistanceToClosestAlly > currentDistanceToClosestAlly)
        {
            scoreChange += (newDistanceToClosestAlly - currentDistanceToClosestAlly) * 50;
        }
        else if (newDistanceToClosestAlly < currentDistanceToClosestAlly)
        {
            scoreChange -= (currentDistanceToClosestAlly - newDistanceToClosestAlly) * 50;
        }

        return scoreChange;
    }

    private int ScoreChangeForOtherSnakeBodyPositions(Point movePoint, SnakeBot snakeBot)
    {
        int scoreChange = 0;

        if (_positionChecker.IsPointInAnySnake(movePoint, countTails: true, snakeBot.Id)
            || _positionChecker.IsPointInGivenSnake(snakeBot.Body, movePoint, countTails: false))
        {
            // We never want to do this unless it's the only choice. Give it a preposterously low score
            scoreChange =  -BASE_CRITICAL_MOVE_SCORE;
        }

        return scoreChange;
        
    }

    private int ScoreChangeForSpaceCreated(Point movePoint, SnakeBot snakeBot)
    {
        // Use flood fill to either move to a more open space, or to give the opponent less space
        // Score the current position:
        // For every direction score all my snake flood fills minus all opponent square flood fills.
        // The highest one wins.
        // TODO: At some point check for all opponent moves here too. 
        // For example, If I go left, give a score for all opponent moves. Count the worse one for me as the score. 

        // TODO: Simulate the movement (just adding a head and removing a tail. At some point we might want to think about
        // simulating gravity but not yet
        List<Point> newSnakeBody = new List<Point>() { movePoint };
        newSnakeBody.AddRange(snakeBot.Body.Take(snakeBot.Body.Count - 1));

        int scoreChange = 0;

        // For the flood fill I want to exclude this snake ID, but include newSnakeBody
        foreach (var mySnake in MySnakeBots)
        {
            if (mySnake.Id == snakeBot.Id)
            {
                scoreChange += _positionChecker.FloodFillCount(newSnakeBody[0], snakeBot.Id, newSnakeBody, 20);
            }
            else
            {
                scoreChange += _positionChecker.FloodFillCount(mySnake.Body[0], snakeBot.Id, newSnakeBody, 20);
            }
        }

        foreach (var opponentSnake in OpponentSnakeBots)
        {
            scoreChange -= _positionChecker.FloodFillCount(opponentSnake.Body[0], snakeBot.Id, newSnakeBody, 20);
        }

        return scoreChange; 
    }

    private int ScoreChangeForPosition(Point movePoint, SnakeBot snakeBot)
    {
        int scoreChange = 0;
        // Add small position bonuses
        // At the start of the game move towards the centre and up. When there are hardly any 
        // power sources left, head towards the nearest one
        // TODO: Add bonus for heading towards the most power sources
  
        if (_level.PowerSources.Count > 2)
        {
            // If the head is out of bounds, and this move will bring it back in, give it a stronger bonus

            if (_positionChecker.IsOutOfMapBounds(snakeBot.Body[0]) && !_positionChecker.IsOutOfMapBounds(movePoint))
            {
                scoreChange += 10;
            }

            int distanceFromCentre = CalculationUtil.GetManhattanDistance(movePoint, new Point(Width / 2, Height / 2));
            scoreChange -= distanceFromCentre;

            // Add a small bonus for moving towards the top of the map
            int distanceFromTop = movePoint.Y;
            scoreChange -= distanceFromTop;
        }
        else
        {
            // Bonus for moving nearer to the nearest powersource
            int distanceToPowerSource = CalculationUtil.GetManhattanDistance(
                movePoint,
                GetNearestPowerSource(movePoint));

            scoreChange -= distanceToPowerSource;
        }

        return scoreChange;
    }

    private void RemoveAllHardNos(Dictionary<Point, int> directionScores, SnakeBot snakeBot)
    {
        HashSet<Point> pointsToRemove = new HashSet<Point>();

        foreach (var directionScore in directionScores)
        {
            Point newHeadPosition = directionScore.Key;

            if (newHeadPosition.X < -1
                || newHeadPosition.X > Width
                || newHeadPosition.Y < -1
                || newHeadPosition.Y > Height
                || _positionChecker.IsPlatform(newHeadPosition)
                || _positionChecker.IsPointInAnySnake(newHeadPosition, countTails: false))
            {
                pointsToRemove.Add(newHeadPosition);
            }

            // If the new position is out of bounds, And the tail is the only one in bounds, hard no the move.
            if (_positionChecker.IsOutOfMapBounds(newHeadPosition))
            {
                int inBoundsCount = 0;

                foreach (var bodyPart in snakeBot.Body)
                {
                    if (!_positionChecker.IsOutOfMapBounds(bodyPart))
                    {
                        inBoundsCount++;
                    }
                }

                if (inBoundsCount <= 1)
                {
                    pointsToRemove.Add(newHeadPosition);
                }
            }
        }

        foreach (var point in pointsToRemove)
        {
            directionScores.Remove(point);
        }
    }

    

    private int ScoreChangeForStuckDirections(Point movePoint, SnakeBot snakeBot)
    {
        int scoreChange = 0;

        if (_positionChecker.IsStuckMove(movePoint, snakeBot))
        {
            scoreChange = -50;
        }

        return scoreChange;
    }

    // Create plans for any clashing moves that would result in a positive headclash
    // For example, if I can move into a position where an enemy snake could move into
    // on their next turn but they are smaller than me, then it's worth it
    // TODO: Only make it a critical move if it's guaranteed. If the opponent can avoid it just make it a good
    // // move
    private (Plan?, bool) GetCriticalHeadClashMove(Point newHeadPosition, SnakeBot snakeBot)
    {
        var excludeMove = false;

        List<int> clashingEnemyBodySizes = new List<int>();

        bool powerUpOnSpot = _level.PowerSources.Contains(newHeadPosition);

        foreach (var opponentSnake in OpponentSnakeBots)
        {
            var possibleHeadMoves = new List<Point>()
            {
                new Point(opponentSnake.Body[0].X + 1, opponentSnake.Body[0].Y),
                new Point(opponentSnake.Body[0].X - 1, opponentSnake.Body[0].Y),
                new Point(opponentSnake.Body[0].X, opponentSnake.Body[0].Y + 1),
                new Point(opponentSnake.Body[0].X, opponentSnake.Body[0].Y - 1)
            };

            foreach (Point possibleMove in possibleHeadMoves)
            {
               // Don't check invalid moves
                if (possibleMove.X < -1
                    || possibleMove.X > Width
                    || possibleMove.Y < -1
                    || possibleMove.Y > Height
                    || _positionChecker.IsPlatform(possibleMove)
                    || _positionChecker.IsPointInAnySnake(possibleMove, countTails: powerUpOnSpot))
                {
                    continue;
                }

                if (possibleMove == newHeadPosition)
                {
                    clashingEnemyBodySizes.Add(opponentSnake.Body.Count);
                }
            }
        }

        if (clashingEnemyBodySizes.Count == 0)
        {
            return (null, false);
        }

        int myLossOnImpact = 0;
        int enemyLossOnImpact = 0;

        if (powerUpOnSpot)
        {
            myLossOnImpact = 1;

            foreach (var enemyBodySize in clashingEnemyBodySizes)
            {
                enemyLossOnImpact += 1;
            }
        }
        else
        {
            myLossOnImpact = snakeBot.Body.Count <= 3 ? 3 : 1;

            foreach (var enemyBodySize in clashingEnemyBodySizes)
            {
                enemyLossOnImpact += enemyBodySize <= 3 ? 3 : 1;
            }
        }

        // If I win overall, it's a golden move (highest score)
        // If it's neutral, and I'm bigger than the enemy snake, it's a golden move (mid score)
        // If it's 100% neutral, it's only a golden move if I have the highest score (the game ending 
        // is in my favour.

        int diff = enemyLossOnImpact - myLossOnImpact;

        int score = 0;

        if (diff > 0)
        {
            score = BASE_CRITICAL_MOVE_SCORE + 5000;           
        }
        else if (diff == 0 && snakeBot.Body.Count > clashingEnemyBodySizes.Min())
        {
            score = BASE_CRITICAL_MOVE_SCORE + 4000;
        }
        else if (diff == 0 && powerUpOnSpot)
        {
            score = BASE_CRITICAL_MOVE_SCORE + 3500;
        }
        else if (diff == 0 && snakeBot.Body.Count == clashingEnemyBodySizes.Min() && GetMyScore() > GetEnemyScore())
        {
            score = BASE_CRITICAL_MOVE_SCORE + 3000;
        }
        else if (diff == 0 && snakeBot.Body.Count < clashingEnemyBodySizes.Min())
        {
            if (FEATURE_INCREASE_EXCLUDE_MOVES_ON)
            {
                excludeMove = true;
            }
        }
        else if (diff < 0)
        {
            excludeMove = true;
        }

        Plan? plan = null;
        
        if (score > 0)
        {
            score += _positionChecker.FloodFillCount(newHeadPosition, snakeBot.Id, snakeBot.Body, 20);
            plan = new Plan(new List<Point> { newHeadPosition }, score, "attack", turnsToFruition: 1, snakeBot.Id);
        }

        return (plan, excludeMove);
    }

    private int GetEnemyScore()
    {
        return GetTotalBodyCount(OpponentSnakeBots);
    }

    private int GetMyScore()
    {
        return GetTotalBodyCount(MySnakeBots);
    }

    private static int GetTotalBodyCount(List<SnakeBot> snakes)
    {
        int bodyCount = 0;

        foreach (var snake in snakes)
        {
            bodyCount += snake.Body.Count;
        }

        return bodyCount;
    }

    private Point GetNearestPowerSource(SnakeBot snakeBot)
    {
        return GetNearestPowerSource(snakeBot.Body[0]);
    }

    private Point GetNearestPowerSource(Point point)
    {
        int nearestDistance = int.MaxValue;
        Point nearestPowerSource = new Point(-1, -1);

        foreach (var powerSource in _level.PowerSources)
        {
            int distance = CalculationUtil.GetManhattanDistance(point, powerSource);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestPowerSource = powerSource;
            }
        }

        return nearestPowerSource;
    }

    private List<Plan> GetShortestPathToPowerSourcePlans(SnakeBot snakeBot, int maxDistance, HashSet<Point> excludePoints)
    {
        List<Plan> plans = new List<Plan>();

        foreach (Point powerSource in _level.PowerSources)
        {
            if (snakeBot.HasCheckedPowerSource(powerSource))
            {
                continue;
            }

            // Don't bother trying if it's further away than maxDistance
            int manhattanDistance = CalculationUtil.GetManhattanDistance(snakeBot.Body[0], powerSource);
            if (manhattanDistance >= maxDistance)
            {
                continue;
            }

            // If the snake can't reach the power source from a platform don't even bother trying
            if (snakeBot.Body.Count < _positionChecker.GetNearestPlatformDistance(powerSource, snakeBot.Id) - 1)
            {
                continue;
            }

            if (snakeBot.GetAttemptsAtPowerSource(powerSource) > 20)
            {
                snakeBot.ClearAttemptsAtPowerSource(powerSource);
                continue;                
            }

            snakeBot.AddAttemptAtPowerSource(powerSource);

            List<Point> path = _pathFinder.GetShortestPath(snakeBot.Body.First(), powerSource, snakeBot, excludePoints.ToList(), _solidPoints, _powerUpPoints);

            snakeBot.AddCheckedPowerSource(powerSource);

            if (path?.Count > 0)
            {
                // Create a plan for this path
                int score = BASE_POWER_SCORE - (path.Count * PATH_LENGTH_PENALTY); // Small penalty for longer paths

                // If it's a small path and doing this blocks an opponent from getting to a power source
                // give bonus points
                if (path.Count <= 5)
                {
                    if (_closestSnakeToPowerSourceMap.ContainsKey(powerSource))
                    {                        
                        int closestSnakeId;
                        _closestSnakeToPowerSourceMap.TryGetValue(powerSource, out closestSnakeId);

                        Logger.Message($"Checking if going for power source at {powerSource.X},{powerSource.Y} would block opponent snake {closestSnakeId} from getting it.");

                        SnakeBot closestSnake = OpponentSnakeBots.First(s => s.Id == closestSnakeId);

                        if (closestSnake != null)
                        {
                            List<Point> closestSnakePath = _pathFinder.GetShortestPath(closestSnake.Body.First(), powerSource, closestSnake, new List<Point>(), _solidPoints, _powerUpPoints);
                            
                            Logger.Message($"Closest snake {closestSnakeId} path to power source at {powerSource.X},{powerSource.Y}: {(closestSnakePath == null ? "null" : string.Join(", ", closestSnakePath.Select(p => $"({p.X},{p.Y})")))}. Path length: {(closestSnakePath == null ? "null" : closestSnakePath.Count.ToString())}");


                            if (closestSnakePath?.Count > 0)
                            {
                                // EMERGENCY DUCK OUT IF IT'S COMPLETELY UNWINNABLE
                                if (path.Count > 1 && closestSnakePath.Count == 1)
                                {
                                    continue;
                                }

                                if (path.Count > 2 && closestSnakePath.Count == 2)
                                {
                                    continue;
                                }

                                if (path.Count > 3 && closestSnakePath.Count == 3)
                                {
                                    continue;
                                }

                                if (path.Count > 4 && closestSnakePath.Count == 4)
                                {
                                    continue;
                                }

                                if (closestSnakePath.Count < path.Count)
                                {
                                    score -= CLOSER_TO_POWER_SCORE;                                    
                                }
                                else
                                {
                                    score += CLOSER_TO_POWER_SCORE;
                                }
                            }
                        }
                    }
                }

                plans.Add(new Plan(path, score, "power", turnsToFruition: path.Count, snakeBot.Id));


                if (maxDistance >= 10)
                {
                    // Exit early. Paths of 10 can be expensive
                    break;
                }
            }
            else
            {
                Logger.LogTime($"No path to power source {powerSource.X}, {powerSource.Y}.");
            }

            if (maxDistance == 10)
            {
                if (plans.Count > 0)
                {
                    Logger.Message("We found at least one power source at max distance 10. Don't try any more.");
                    break;
                }
            }
        }

        Logger.LogTime($"Finished looking for power sources with max distance {maxDistance}");

        return plans;
    }

    internal HashSet<Point> GetPowerSources()
    {
        return _level.PowerSources;
    }
}