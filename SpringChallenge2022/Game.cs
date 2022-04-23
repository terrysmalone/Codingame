using System;
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

    private bool _weGotAController; // If our opponent likes to control our defenders make sure they're always shielded


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

        _defaultStrategies.Add(Strategy.Defend);
        _defaultStrategies.Add(Strategy.Defend);
        _defaultStrategies.Add(Strategy.Collect);
    }

    internal string[] GetMoves(IReadOnlyCollection<Hero> enemyHeroes, IReadOnlyCollection<Monster> monsters, int playerMana)
    {
        _spellGenerator.SetEstimatedMana(playerMana);

        var moves = new string[_heroesPerPlayer];

        ResetHeroes();

        CheckForPhaseChange(playerMana);

        SetGuardPoints();

        CheckForController();
        ClearStaleAttacks(monsters);

        _movementGenerator.AssignHeroMovement(_playerHeroes, monsters);

        if (_weGotAController)
        {
            _spellGenerator.CastProtectiveShieldSpells(_playerHeroes);
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

    private void SetGuardPoints()
    {
        if (_playerHeroes[0].GetNumberOfGuardPoints() == 0)   // or we've changed a Strategy
        {
            var guardPoints = new List<List<Point>>();


            guardPoints.AddRange(GetDefenders());

            guardPoints.AddRange(GetCollectors());

            guardPoints.AddRange(GetAttackers());

            // Set guard points
            if (_playerHeroes.Count != guardPoints.Count)
            {
                Console.Error.WriteLine("ERROR: Player heroes count doesn't match guard point count");
            }

            // At some point we need to make sure we move heroes around to minimise travel to new spots

            Console.Error.WriteLine($"_playerHeroes.Count: {_playerHeroes.Count}");

            for (var i = 0; i < _playerHeroes.Count; i++)
            {
                Console.Error.WriteLine($"i: {i}");
                var hero = _playerHeroes[i];
                hero.SetGuardPoints(guardPoints[i]);
            }
        }
    }

    private List<List<Point>> GetDefenders()
    {
        var numberOfDefenders = _playerHeroes.Count(h => h.Strategy == Strategy.Defend);

        var defendPoints = new List<List<Point>>();

        if (numberOfDefenders == 1)
        {
            if (_playerBaseLocation.X == 0)
            {
                defendPoints.Add(new List<Point> { new Point(4000, 4000) });
            }
            else
            {
                defendPoints.Add(new List<Point> { new Point(_xMax - 4000, _yMax - 4000) });
            }
        }
        else if (numberOfDefenders == 2)
        {
            if (_playerBaseLocation.X == 0)
            {
                defendPoints.Add(new List<Point> { new Point(5700, 2500) });
                defendPoints.Add(new List<Point> { new Point(2500, 5700) });
            }
            else
            {
                defendPoints.Add(new List<Point> { new Point(_xMax - 5700, _yMax - 2500) });
                defendPoints.Add(new List<Point> { new Point(_xMax - 2500, _yMax - 5700) });
            }
        }
        else if (numberOfDefenders == 3)
        {
            if (_playerBaseLocation.X == 0)
            {
                defendPoints.Add(new List<Point> { new Point(5000, 2000) });
                defendPoints.Add(new List<Point> { new Point(4000, 4000) });
                defendPoints.Add(new List<Point> { new Point(2000, 5000) });
            }
            else
            {
                defendPoints.Add(new List<Point> { new Point(_xMax - 5000, _yMax - 2000) });
                defendPoints.Add(new List<Point> { new Point(_xMax - 4000, _yMax - 4000) });
                defendPoints.Add(new List<Point> { new Point(_xMax - 2000, _yMax - 5000) });
            }
        }

        return defendPoints;
    }

    private IEnumerable<List<Point>> GetCollectors()
    {
        var numberOfCollectors = _playerHeroes.Count(h => h.Strategy == Strategy.Collect);

        var collectPoints = new List<List<Point>>();

        if (numberOfCollectors == 1)
        {
            collectPoints.Add(new List<Point>
            {
                new Point(_xMax / 2, _yMax / 2)
            });
        }

        return collectPoints;
    }

    private List<List<Point>> GetAttackers()
    {
        var numberOfAttackers = _playerHeroes.Count(h => h.Strategy == Strategy.Attack);

        var attackPoints = new List<List<Point>>();

        if (numberOfAttackers == 1)
        {
            if (_playerBaseLocation.X == 0)
            {
                attackPoints.Add(new List<Point>
                {
                    new Point(_xMax - 3000, _yMax - 2500)
                });
            }
            else
            {
                attackPoints.Add(new List<Point>
                {
                    new Point(3000, 2500)
                });
            }
        }

        return attackPoints;
    }

    private void CheckForController()
    {
        if (!_weGotAController && _playerHeroes.Any(h => h.IsControlled && h.Strategy == Strategy.Defend))
        {
            _weGotAController = true;
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
