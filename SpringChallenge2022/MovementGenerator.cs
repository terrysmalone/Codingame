using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SpringChallenge2022;

internal sealed class MovementGenerator
{
    private readonly Point _playerBaseLocation;
    private readonly Point _enemyBaseLocation;
    private readonly ValuesProvider _valuesProvider;

    public MovementGenerator(Point playerBaseLocation,
                             Point enemyBaseLocation,
                             ValuesProvider valuesProvider)
    {
        _playerBaseLocation = playerBaseLocation;
        _enemyBaseLocation = enemyBaseLocation;
        _valuesProvider = valuesProvider;
    }

    internal void AssignHeroMovement(List<Hero> playerHeroes, List<Monster> monsters, ActionManager actionManager)
    {
        IEnumerable<Hero> defendingHeroesOutsideOfBase = playerHeroes.Where(h => h.Strategy == Strategy.Defend
                                                                                   && CalculateDistance(h.Position, _playerBaseLocation) > _valuesProvider.BaseRadius);

        foreach (Hero? defendingHeroOutsideOfBase in defendingHeroesOutsideOfBase)
        {
            defendingHeroOutsideOfBase.CurrentMonster = -1;
        }

        CalculateDefenderMovement(playerHeroes, monsters);

        CalculateCollectorMovement(playerHeroes, monsters);

        CalculateAttackerMovement(playerHeroes, monsters);

        // Assign actions
        foreach (Hero hero in playerHeroes)
        {
            if (hero.CurrentMonster != -1)
            {
                Monster monsterToAttack = monsters.Single(m => m.Id == hero.CurrentMonster);

                actionManager.AddPossibleAction(hero.Id, 0, ActionType.Move, EntityType.None, null, monsterToAttack.Position.X, monsterToAttack.Position.Y);
            }
            else
            {
                Point currentGuardPoint = hero.GetCurrentGuardPoint();

                if (!(hero.Position.X == currentGuardPoint.X && hero.Position.Y == currentGuardPoint.Y))
                {
                    actionManager.AddPossibleAction(hero.Id, 0, ActionType.Move, EntityType.None, null, currentGuardPoint.X, currentGuardPoint.Y);
                }
                else
                {
                    Point nextGuardPoint = hero.GetNextGuardPoint();
                    actionManager.AddPossibleAction(hero.Id, 0, ActionType.Move, EntityType.None, null, nextGuardPoint.X, nextGuardPoint.Y);
                }
            }
        }
    }

    private void CalculateAttackerMovement(IEnumerable<Hero> playerHeroes, IEnumerable<Monster> monsters)
    {
        List<Hero> freeAttackingHeroes = playerHeroes.Where(h => h.Strategy == Strategy.Attack && h.CurrentMonster == -1).ToList();

        if (freeAttackingHeroes.Count <= 0)
        {
            return;
        }

        // Get any monsters on the edge of the enemies base
        List<Monster> monstersOnOutskirts = monsters.Where(m => CalculateDistance(m.Position, _enemyBaseLocation) > _valuesProvider.OutskirtsMinDist
                                                      && CalculateDistance(m.Position, _enemyBaseLocation) < _valuesProvider.OutskirtsMaxDist).ToList();

        // Go to them
        if (monstersOnOutskirts.Count > 1)
        {
            foreach (Hero? freeAttackingHero in freeAttackingHeroes)
            {
                Monster closestMonster = monstersOnOutskirts.OrderBy(m => CalculateDistance(freeAttackingHero.Position, m.Position))
                                                        .First();

                freeAttackingHero.CurrentMonster = closestMonster.Id;
            }
        }
    }

    private void CalculateDefenderMovement(IReadOnlyCollection<Hero> playerHeroes, List<Monster> monsters)
    {
        // if a hero is not in the base, and a spider is, drop everything and defend
        List<Monster> monstersThreateningBase = monsters.Where(m => m.ThreatFor == ThreatFor.Player
                                                                         && CalculateDistance(m.Position, _playerBaseLocation) <= 6000)
                                                         .OrderBy(m => CalculateDistance(m.Position, _playerBaseLocation))
                                                         .ToList();

        List<Hero> freeDefendingHeroes = playerHeroes.Where(h => h.Strategy == Strategy.Defend && h.CurrentMonster == -1).ToList();

        if (monstersThreateningBase.Count == 0 && freeDefendingHeroes.Count <= 0)
        {
            return;
        }

        if (monstersThreateningBase.Count > 0)
        {
            Monster closestMonster = monstersThreateningBase.First();

            foreach (Hero? hero in freeDefendingHeroes)
            {
                hero.CurrentMonster = closestMonster.Id;
            }
        }
        else
        {
            foreach (Hero? freeDefendingHero in freeDefendingHeroes)
            {
                Monster? monsterWithinRange = monsters.Where(m => CalculateDistance(m.Position, _playerBaseLocation) <= _valuesProvider.MaxDefenderDistanceFromBase
                                                             && m.ThreatFor != ThreatFor.Enemy)
                                                 .Select(m => new { m, distance = CalculateDistance(m.Position, freeDefendingHero.Position) })
                                                 .Where(m => m.distance <= _valuesProvider.HeroRange)
                                                 .OrderBy(m => m.distance)
                                                 .Select(m => m.m)
                                                 .FirstOrDefault();

                if (monsterWithinRange != null)
                {
                    freeDefendingHero.CurrentMonster = monsterWithinRange.Id;
                }
            }
        }
    }

    private void CalculateCollectorMovement(IEnumerable<Hero> playerHeroes, IEnumerable<Monster> monsters)
    {

        List<Hero> collectingHeroes = playerHeroes.Where(h => h.Strategy == Strategy.Collect && h.CurrentMonster == -1).ToList();

        if (collectingHeroes.Count > 0)
        {
            foreach (Hero? collectingHero in collectingHeroes)
            {
                Monster? closestMonster = monsters.Where(m => CalculateDistance(m.Position, _playerBaseLocation) > _valuesProvider.OutskirtsMaxDist)
                                             .Select(m => new { m, distance = CalculateDistance(m.Position, collectingHero.Position) })
                                             .Where(m => m.distance <= _valuesProvider.HeroRange)
                                             .OrderBy(m => m.distance)
                                             .Select(m => m.m)
                                             .FirstOrDefault();

                if (closestMonster != null)
                {
                    collectingHero.CurrentMonster = closestMonster.Id;
                }
            }
        }
    }

    private static double CalculateDistance(Point position, Point position2)
    {
        return Math.Sqrt(Math.Pow(position.X - position2.X, 2)
                         + Math.Pow(position.Y - position2.Y, 2));
    }
}

