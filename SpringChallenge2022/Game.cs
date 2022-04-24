﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SpringChallenge2022;

internal class Game
{
    private readonly Point _playerBaseLocation;
    private readonly Point _enemyBaseLocation;
    private readonly int _heroesPerPlayer;
    private readonly MovementGenerator _movementGenerator;
    private readonly SpellGenerator _spellGenerator;
    private readonly GuardPointGenerator _guardPointGenerator;

    private bool _inCollectionPhase = true;

    private readonly List<Hero> _playerHeroes = new List<Hero>();

    private const int _xMax = 17630;
    private const int _yMax = 9000;

    private const int _windSpellRange = 1280;
    private const int _controlSpellange = 2200;
    private const int _shieldSpellRange = 2200;

    private const int _outskirtsMinDist = 5000;
    private const int _outskirtsMaxDist = 7000;
    private const int _heroRange = 2200;
    private const int _maxDefenderDistanceFromBase = 7500;
    private const int _baseRadius = 5000;
    private const int _closeToBaseRange = 1000;

    private bool _weGotADefenderController; // If our opponent likes to control our defenders make sure they're always shielded
    private bool _weGotAnAttackerController; // If our opponent likes to control our attackers make sure they're always shielded

    private readonly List<Strategy> _defaultStrategies = new List<Strategy>(0);

    internal Game(Point playerBaseLocation, int heroesPerPlayer)
    {
        _playerBaseLocation = playerBaseLocation;
        _heroesPerPlayer = heroesPerPlayer;

        _enemyBaseLocation = new Point(playerBaseLocation.X == 0 ? _xMax : 0, playerBaseLocation.Y == 0 ? _yMax : 0);

        _movementGenerator = new MovementGenerator(_playerBaseLocation,
                                                   _enemyBaseLocation,
                                                   _baseRadius,
                                                   _maxDefenderDistanceFromBase,
                                                   _outskirtsMinDist,
                                                   _outskirtsMaxDist,
                                                   _heroRange);

        _spellGenerator = new SpellGenerator(_playerBaseLocation,
                                             _enemyBaseLocation,
                                             _baseRadius,
                                             _closeToBaseRange,
                                             _outskirtsMinDist,
                                             _outskirtsMaxDist,
                                             _windSpellRange,
                                             _controlSpellange,
                                             _shieldSpellRange);

        _guardPointGenerator = new GuardPointGenerator(_playerBaseLocation, _xMax, _yMax);

        _defaultStrategies.Add(Strategy.Defend);
        _defaultStrategies.Add(Strategy.Defend);
        _defaultStrategies.Add(Strategy.Collect);
    }

    internal string[] GetMoves(IReadOnlyCollection<Hero> enemyHeroes, List<Monster> monsters, int playerMana)
    {
        _spellGenerator.SetEstimatedMana(playerMana);

        var moves = new string[_heroesPerPlayer];

        ResetHeroes();

        CheckForPhaseChange(playerMana);

        if (_playerHeroes[0].GetNumberOfGuardPoints() == 0) // or we've changed a Strategy
        {
            var guardPoints = _guardPointGenerator.GetGuardPoints(_playerHeroes);

            for (var i = 0; i < _playerHeroes.Count; i++)
            {
                var hero = _playerHeroes[i];
                hero.SetGuardPoints(guardPoints[i]);
            }
        }

        CheckForController();
        ClearStaleAttacks(monsters);

        _movementGenerator.AssignHeroMovement(_playerHeroes, monsters);

        if (_weGotADefenderController)
        {
            _spellGenerator.CastProtectiveShieldSpells(_playerHeroes, Strategy.Defend);
        }

        if (_weGotAnAttackerController)
        {
            _spellGenerator.CastProtectiveShieldSpells(_playerHeroes, Strategy.Attack);
        }

        _spellGenerator.AssignDefensiveWindSpell(_playerHeroes, monsters);

        if (!_inCollectionPhase)
        {
            _spellGenerator.AssignDefenderControlSpells(_playerHeroes, monsters);

            _spellGenerator.AssignAttackSpells(_playerHeroes, enemyHeroes, monsters);
        }

        for (var i = 0; i < moves.Length; i++)
        {
            moves[i] = _playerHeroes[i].CurrentAction;
        }

        return moves;
    }

    private void ResetHeroes()
    {
        foreach (var hero in _playerHeroes)
        {
            hero.CurrentAction = string.Empty;
            hero.UsingSpell = false;
            hero.IsShielding = false;
        }
    }

    private void CheckForPhaseChange(int mana)
    {
        if (_inCollectionPhase)
        {
            if(mana > 300)
            {
                _inCollectionPhase = false;

                ClearGuardPoints();
                ChangeCollectorToAttacker();
            }
        }
    }

    private void ClearGuardPoints()
    {
        foreach (var hero in _playerHeroes)
        {
            hero.ClearGuardPoints();
        }
    }

    private void ChangeCollectorToAttacker()
    {
        var heroes = _playerHeroes.Where(h => h.Strategy == Strategy.Collect);

        foreach (var hero in heroes)
        {
            hero.Strategy = Strategy.Attack;
        }
    }

    private void CheckForController()
    {
        if (!_weGotADefenderController && _playerHeroes.Any(h => h.IsControlled && h.Strategy == Strategy.Defend))
        {
            _weGotADefenderController = true;
        }

        if (!_weGotAnAttackerController && _playerHeroes.Any(h => h.IsControlled && h.Strategy == Strategy.Attack))
        {
            _weGotAnAttackerController = true;
        }
    }

    private void ClearStaleAttacks(IReadOnlyCollection<Monster> monsters)
    {
        ClearDeadMonsters(monsters);

        ClearMonstersIfDefenderIsTooFarAway();
        ClearMonstersIfTheyreAThreatForTheEnemy(monsters);

        ClearMonstersFromEnemyOutskirts(monsters);
    }

    private static double CalculateDistance(Point position, Point position2)
    {
        return Math.Sqrt(Math.Pow(position.X - position2.X, 2)
                         + Math.Pow(position.Y - position2.Y, 2));
    }

    private void ClearDeadMonsters(IReadOnlyCollection<Monster> monsters)
    {
        foreach (var hero in _playerHeroes)
        {
            if (hero.CurrentMonster >= 0)
            {
                if (!monsters.Any(m => m.Id == hero.CurrentMonster))
                {
                    hero.CurrentMonster = -1;
                }
            }
        }
    }

    private void ClearMonstersIfDefenderIsTooFarAway()
    {
        foreach (var hero in _playerHeroes.Where(h => h.Strategy == Strategy.Defend))
        {
            if (hero.CurrentMonster >= 0)
            {
                if (CalculateDistance(hero.Position, _playerBaseLocation) > _maxDefenderDistanceFromBase)
                {
                    hero.CurrentMonster = -1;
                }
            }
        }
    }

    private void ClearMonstersIfTheyreAThreatForTheEnemy(IReadOnlyCollection<Monster> monsters)
    {
        foreach (var hero in _playerHeroes)
        {
            if (hero.CurrentMonster >= 0)
            {
                if (monsters.Any(m => m.Id == hero.CurrentMonster && m.ThreatFor == ThreatFor.Enemy))
                {
                    hero.CurrentMonster = -1;
                }
            }
        }
    }

    private void ClearMonstersFromEnemyOutskirts(IReadOnlyCollection<Monster> monsters)
    {
        foreach (var hero in _playerHeroes.Where(h => h.Strategy == Strategy.Attack))
        {
            if (hero.CurrentMonster >= 0)
            {
                var currentMonster = monsters.First(m => m.Id == hero.CurrentMonster);

                if (CalculateDistance(currentMonster.Position, _enemyBaseLocation) < _outskirtsMinDist
                    || CalculateDistance(currentMonster.Position, _enemyBaseLocation) > _outskirtsMaxDist)
                {
                    hero.CurrentMonster = -1;
                }
            }
        }
    }

    internal void UpdatePlayerHero(Hero hero)
    {
        var playerHero = _playerHeroes.SingleOrDefault(h => h.Id == hero.Id);

        if (playerHero == null)
        {
            // Assign a strategy
            Console.Error.WriteLine($"Assigning strategy {_defaultStrategies[_playerHeroes.Count]}");
            hero.Strategy = _defaultStrategies[_playerHeroes.Count];

            _playerHeroes.Add(hero);
        }
        else
        {
            playerHero.Position = hero.Position;
            playerHero.IsControlled = hero.IsControlled;
            playerHero.ShieldLife = hero.ShieldLife;
        }
    }
}
