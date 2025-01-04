using System;
using System.Collections.Generic;
using System.Drawing;

namespace WinterChallenge2024;

internal sealed class ActionFinder
{
    private readonly Game _game;
    private readonly DirectionCalculator _directionCalculator;
    private readonly AStar _aStar;

    private List<Protein> _proteinsToCheck = new List<Protein>();

    public ActionFinder(Game game, DirectionCalculator directionCalculator)
    {
        _game = game;
        _directionCalculator = directionCalculator;
        _aStar = new AStar(game);
    }

    internal List<Action> GetProteinActions(Organism organism, List<Protein> proteins)
    {
        List<Action> actions = new List<Action>();

        _proteinsToCheck = new List<Protein>();

        foreach (Protein protein in proteins)
        {
            // If it's harvested or blocked (this can only be from a tentacle facing it) then ignore it
            if (!protein.IsHarvested && !_game.opponentTentaclePath[protein.Position.X, protein.Position.Y])
            {
                _proteinsToCheck.Add(protein.Clone());
            }
        }

        if (_proteinsToCheck.Count == 0) return actions;

        actions.AddRange(GetShortestPathsToProteins(organism, 1, GrowStrategy.ALL_PROTEINS));
        if (_proteinsToCheck.Count == 0) return actions;

        actions.AddRange(GetShortestPathsToProteins(organism, 2, GrowStrategy.NO_PROTEINS));
        if (_proteinsToCheck.Count == 0) return actions;
        actions.AddRange(GetShortestPathsToProteins(organism, 2, GrowStrategy.UNHARVESTED));
        if (_proteinsToCheck.Count == 0) return actions;

        actions.AddRange(GetShortestPathsToProteins(organism, 3, GrowStrategy.NO_PROTEINS));
        if (_proteinsToCheck.Count == 0) return actions;
        actions.AddRange(GetShortestPathsToProteins(organism, 3, GrowStrategy.UNHARVESTED));
        if (_proteinsToCheck.Count == 0) return actions;

        actions.AddRange(GetShortestPathsToProteins(organism, 4, GrowStrategy.NO_PROTEINS));
        if (_proteinsToCheck.Count == 0) return actions;
        actions.AddRange(GetShortestPathsToProteins(organism, 4, GrowStrategy.UNHARVESTED));
        if (_proteinsToCheck.Count == 0) return actions;

        actions.AddRange(GetShortestPathsToProteins(organism, 5, GrowStrategy.NO_PROTEINS));
        if (_proteinsToCheck.Count == 0) return actions;
        actions.AddRange(GetShortestPathsToProteins(organism, 5, GrowStrategy.UNHARVESTED));
        if (_proteinsToCheck.Count == 0) return actions;

        return actions;   
    }

    private IEnumerable<Action> GetShortestPathsToProteins(Organism organism, int maxDistance, GrowStrategy growStrategy)
    {
        List<Action> actions = new List<Action>();

        List<int> proteinsToRemove = new List<int>();
 
        for (int i = 0; i < _proteinsToCheck.Count; i++)
        {
            Protein protein = _proteinsToCheck[i];
            foreach (Organ organ in organism.Organs)
            {
                //Console.Error.WriteLine($"Checking protein {protein.Position} to organ {organ.Position}");
                int manhattanDistance = MapChecker.CalculateManhattanDistance(organ.Position, protein.Position);
                
                if (manhattanDistance > maxDistance)
                {
                    continue;
                }

                List<Point> path = _aStar.GetShortestPath(organ.Position, protein.Position, maxDistance, growStrategy, false);
                
               // Console.Error.WriteLine($"Path from {organ.Position} to {protein.Position} is {path.Count} long");
                if (path.Count > 0)
                {
                    //actions.Add(new Tuple<int, ProteinType, List<Point>>(organ.Id, protein.Type, path));

                    Action? action = CreateAction(organism.RootId, organ.Id, protein.Type, path);

                    if (action != null)
                    {
                        actions.Add(action);
                    }
                   
                    if (!proteinsToRemove.Contains(i) && maxDistance != 1)
                    {
                        proteinsToRemove.Add(i);
                    }
                }
            }
        }

        for (int i = proteinsToRemove.Count - 1; i >= 0; i--)
        {
            _proteinsToCheck.RemoveAt(proteinsToRemove[i]);
        }

        return actions;
    }

    // TODO: Where does it decide on using a tentacle??
    private Action? CreateAction(int organismId, int organId, ProteinType proteinType, List<Point> path)
    {
        Action? action = new Action();

        action.ActionType = ActionType.GROW;
        action.TargetPosition = path[0];
        action.OrganismId = organismId;
        action.OrganId = organId;
        
        action.GoalProteinType = proteinType;
        action.GoalPosition = path[path.Count - 1];

        action.Source = "GetShortestPathsToProteins";

        // TODO: Add longer consume actions (We might need to consume something if we 
        //       Have no stock or harvests for C or alculator.CanProduceOrgan(OrganType.H
        if (path.Count == 1)
        {
            action.TurnsToGoal = 1;
            action.GoalType = GoalType.CONSUME;

            action.OrganType = GetOrgan(path[0]);
          
            if (action.OrganType != OrganType.BASIC)
            {
                action.OrganDirection = _directionCalculator.CalculateClosestOpponentDirection(path[0]);
            }
        }
        else if (path.Count == 2)
        {
            if (!CostCalculator.CanProduceOrgan(OrganType.HARVESTER, _game.PlayerProteinStock))
            {
                return null;
            }
            
            action.TurnsToGoal = 1;
            action.GoalType = GoalType.HARVEST;

            action.OrganType = OrganType.HARVESTER;

            action.OrganDirection = _directionCalculator.GetDirection(path[0], path[1]);
        }
        else
        {
            if (!CostCalculator.CanProduceOrgan(OrganType.HARVESTER, _game.PlayerProteinStock))
            {
                return null;
            }

            action.TurnsToGoal = path.Count - 1;
            action.GoalType = GoalType.HARVEST;

            action.OrganType = GetOrgan(path[0]);

            if (action.OrganType != OrganType.BASIC)
            {
                action.OrganDirection = _directionCalculator.CalculateClosestOpponentDirection(path[0]);
            }
        }

        return action;
    }

    private OrganType GetOrgan(Point point)
    {
        bool hasProtein = _game.hasAnyProtein[point.X, point.Y];
        if (CostCalculator.CanProduceOrgan(OrganType.BASIC, _game.PlayerProteinStock))
        {
            return OrganType.BASIC;
        }
        else if (CostCalculator.CanProduceOrgan(OrganType.SPORER, _game.PlayerProteinStock))
        {
            return OrganType.SPORER;
        }
        else if (CostCalculator.CanProduceOrgan(OrganType.HARVESTER, _game.PlayerProteinStock))
        {
            return OrganType.HARVESTER;
        }
        else if (CostCalculator.CanProduceOrgan(OrganType.TENTACLE, _game.PlayerProteinStock))
        {
            return OrganType.TENTACLE;
        }

        return OrganType.BASIC;
    }
}
