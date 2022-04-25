using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SpringChallenge2022;

internal sealed class SpellGenerator
{
    private int _estimatedManaLeft;
    
    private readonly Point _playerBaseLocation;
    private readonly Point _enemyBaseLocation;
    private readonly ValuesProvider _valuesProvider;

    public SpellGenerator(Point playerBaseLocation,
                          Point enemyBaseLocation,
                          ValuesProvider valuesProvider)
    {
        _playerBaseLocation = playerBaseLocation;
        _enemyBaseLocation = enemyBaseLocation;
        _valuesProvider = valuesProvider;
    }

    internal void CastProtectiveShieldSpells(IEnumerable<Hero> playerHeroes, Strategy strategy, ActionManager actionManager)
    {
        foreach (var hero in playerHeroes.Where(h => h.Strategy == strategy))
        {
            if (_estimatedManaLeft < 10)
            {
                break;
            }

            if (hero.ShieldLife == 0)
            {
                actionManager.AddPossibleAction(hero.Id, 90, ActionType.ShieldSpell, EntityType.Hero, hero.Id, null, null);
                PerformSpell(hero);

                hero.IsShielding = true;
            }
        }
    }

    internal void AssignDefensiveWindSpell(List<Hero> playerHeroes, IEnumerable<Monster> monsters, ActionManager actionManager)
    {
        if (_estimatedManaLeft < 10)
        {
            return;
        }

        var closeDistance = 3000;

        var closestMonster = monsters.FirstOrDefault(m => CalculateDistance(m.Position, _playerBaseLocation) <= closeDistance
                                                               && m.ShieldLife == 0);

        if (closestMonster != null)
        {
            var availableHeroes = playerHeroes.Where(h => h.Strategy == Strategy.Defend && h.IsShielding == false).ToList();

            if (availableHeroes.Count > 0)
            {
                var closestHero = availableHeroes.OrderBy(h => CalculateDistance(h.Position, closestMonster.Position))
                                                 .First();

                if (CalculateDistance(closestHero.Position, closestMonster.Position) <= ValuesProvider.WindSpellRange)
                {
                    actionManager.AddPossibleAction(closestHero.Id, 50, ActionType.WindSpell, EntityType.None, null, _enemyBaseLocation.X, _enemyBaseLocation.Y);
                    PerformSpell(closestHero);
                }
                else
                {
                    // Too far away for wind to work

                    // If he's close and we can control that little shit away do it
                    if (CalculateDistance(closestMonster.Position, _playerBaseLocation) <= _valuesProvider.CloseToBaseRange
                        && CalculateDistance(closestHero.Position, closestMonster.Position) <= _valuesProvider.ControlSpellange)
                    {
                        actionManager.AddPossibleAction(closestHero.Id, 50, ActionType.ControlSpell, EntityType.Monster, closestMonster.Id, _enemyBaseLocation.X, _enemyBaseLocation.Y);
                        PerformSpell(closestHero);
                    }
                }
            }
        }
    }

    internal void AssignDefenderControlSpells(IEnumerable<Hero> playerHeroes, IEnumerable<Monster> monsters, ActionManager actionManager)
    {
        const int healthCutOff = 10;

        if (_estimatedManaLeft < 10)
        {
            return;
        }

        var defendingHeroesOutsideOfBase =
            playerHeroes.Where(h => h.Strategy == Strategy.Defend
                                      && h.IsShielding == false
                                      && CalculateDistance(h.Position, _playerBaseLocation) > _valuesProvider.BaseRadius);

        foreach (var defendingHeroOutsideOfBase in defendingHeroesOutsideOfBase)
        {
            if (_estimatedManaLeft < 10)
            {
                return;
            }

            var monsterWithinSpellRange =
                monsters.Where(m => m.Health > healthCutOff
                                        && m.IsControlled == false
                                        && m.ThreatFor != ThreatFor.Enemy
                                        && m.ShieldLife == 0
                                        && CalculateDistance(m.Position, _playerBaseLocation) > _valuesProvider.BaseRadius)
                        .Select(m => new { m, distance = CalculateDistance(m.Position, defendingHeroOutsideOfBase.Position)})
                        .Where(m => m.distance <= _valuesProvider.ControlSpellange)
                        .OrderBy(m => m.distance)
                        .Select(m => m.m)
                        .FirstOrDefault();

            if (monsterWithinSpellRange != null)
            {
                actionManager.AddPossibleAction(defendingHeroOutsideOfBase.Id, 50, ActionType.ControlSpell, EntityType.Monster, monsterWithinSpellRange.Id, _enemyBaseLocation.X, _enemyBaseLocation.Y);
                PerformSpell(defendingHeroOutsideOfBase);
            }
        }
    }

    internal void AssignAttackSpells(IEnumerable<Hero> playerHeroes, IEnumerable<Hero> enemyHeroes, IEnumerable<Monster> monsters, ActionManager actionManager)
    {
        foreach (var attackingHero in playerHeroes.Where(h => h.Strategy == Strategy.Attack))
        {
            if (_estimatedManaLeft < 10)
            {
                return;
            }

            if (CalculateDistance(attackingHero.Position, _enemyBaseLocation) > _valuesProvider.OutskirtsMaxDist)
            {
                continue;
            }

            var closeEnoughForWindMonster = monsters.FirstOrDefault(m => CalculateDistance(m.Position, attackingHero.Position) <= ValuesProvider.WindSpellRange
                                                                                 && m.ShieldLife == 0);

            if (closeEnoughForWindMonster != null)
            {
                actionManager.AddPossibleAction(attackingHero.Id, 50, ActionType.WindSpell, EntityType.None, null, _enemyBaseLocation.X, _enemyBaseLocation.Y);

                PerformSpell(attackingHero);
            }
            else // If we're not close enough for a wind spell try a shield or control
            {
                var closeEnoughForControlEnemy =
                        enemyHeroes.Where(e => e.ShieldLife == 0
                                                 && CalculateDistance(e.Position, attackingHero.Position) <= _valuesProvider.ControlSpellange
                                                 && CalculateDistance(e.Position, _enemyBaseLocation) <= _valuesProvider.BaseRadius)
                                   .OrderBy(e => CalculateDistance(e.Position, _enemyBaseLocation))
                                   .FirstOrDefault();

                var closeEnoughForSpellMonster =
                        monsters.FirstOrDefault(m => m.ShieldLife == 0
                                                         && m.ThreatFor == ThreatFor.Enemy
                                                         && CalculateDistance(m.Position, attackingHero.Position) <= _valuesProvider.ShieldSpellRange
                                                         && CalculateDistance(m.Position, _enemyBaseLocation) <= _valuesProvider.OutskirtsMinDist);

                if (closeEnoughForControlEnemy != null && closeEnoughForSpellMonster != null)
                {
                    if (new Random().Next(1) == 0)
                    {
                        actionManager.AddPossibleAction(attackingHero.Id, 50, ActionType.ShieldSpell, EntityType.Monster, closeEnoughForSpellMonster.Id, null, null);
                        PerformSpell(attackingHero);
                    }
                    else
                    {
                        actionManager.AddPossibleAction(attackingHero.Id, 50, ActionType.ControlSpell, EntityType.Monster, closeEnoughForSpellMonster.Id, _playerBaseLocation.X, _playerBaseLocation.Y);
                        PerformSpell(attackingHero);
                    }
                }
                else if (closeEnoughForSpellMonster != null)
                {
                    actionManager.AddPossibleAction(attackingHero.Id, 50, ActionType.ShieldSpell, EntityType.Monster, closeEnoughForSpellMonster.Id, null, null);
                    PerformSpell(attackingHero);
                }
                else if (closeEnoughForControlEnemy != null)
                {
                    actionManager.AddPossibleAction(attackingHero.Id, 50, ActionType.ControlSpell, EntityType.Enemy, closeEnoughForControlEnemy.Id, _playerBaseLocation.X, _playerBaseLocation.Y);

                    PerformSpell(attackingHero);
                }
            }
        }
    }

    private void PerformSpell(Hero hero)
    {
        hero.CurrentMonster = -1;

        if (hero.UsingSpell == false)
        {
            _estimatedManaLeft -= 10;
            hero.UsingSpell = true;
        }
    }
    
    private static double CalculateDistance(Point position, Point position2)
    {
        return Math.Sqrt(Math.Pow(position.X - position2.X, 2)
                         + Math.Pow(position.Y - position2.Y, 2));
    }

    internal void SetEstimatedMana(int estimate)
    {
        _estimatedManaLeft = estimate;
    }
}
