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

    private readonly ActionManager _actionManager;
    private readonly ValuesProvider _valuesProvider;
    private readonly MovementGenerator _movementGenerator;
    private readonly SpellGenerator _spellGenerator;
    private readonly GuardPointGenerator _guardPointGenerator;

    private bool _inCollectionPhase = true;
    private bool _alreadyAttacked;

    private readonly List<Hero> _playerHeroes = new List<Hero>();

    private bool _weGotADefenderController; // If our opponent likes to control our defenders make sure they're always shielded
    private bool _weGotAnAttackerController; // If our opponent likes to control our attackers make sure they're always shielded

    private readonly List<Strategy> _defaultStrategies = new List<Strategy>(0);

    internal Game(Point playerBaseLocation, int heroesPerPlayer)
    {
        _playerBaseLocation = playerBaseLocation;
        _heroesPerPlayer = heroesPerPlayer;

        _valuesProvider = new ValuesProvider();

        if (playerBaseLocation.X == 0)
        {
            _enemyBaseLocation = new Point(_valuesProvider.XMax, _valuesProvider.YMax);
            _actionManager = new ActionManager(true);
        }
        else
        {
            _enemyBaseLocation = new Point(0, 0);
            _actionManager = new ActionManager(false);
        }

        _movementGenerator = new MovementGenerator(_playerBaseLocation,
                                                   _enemyBaseLocation,
                                                   _valuesProvider);

        _spellGenerator = new SpellGenerator(_playerBaseLocation,
                                             _enemyBaseLocation,
                                             _valuesProvider);

        _guardPointGenerator = new GuardPointGenerator(_playerBaseLocation, _valuesProvider);

        _defaultStrategies.Add(Strategy.Defend);
        _defaultStrategies.Add(Strategy.Defend);
        _defaultStrategies.Add(Strategy.Collect);
    }

    internal string[] GetMoves(IReadOnlyCollection<Hero> enemyHeroes, List<Monster> monsters, int playerMana)
    {
        _actionManager.ClearPossibleActions();
        _actionManager.SetMana(playerMana);

        string[] moves = new string[_heroesPerPlayer];

        ResetHeroes();

        CheckForPhaseChange(playerMana);

        if (_playerHeroes[0].GetNumberOfGuardPoints() == 0) // or we've changed a Strategy
        {
            List<List<Point>> guardPoints = _guardPointGenerator.GetGuardPoints(_playerHeroes);

            for (int i = 0; i < _playerHeroes.Count; i++)
            {
                Hero hero = _playerHeroes[i];
                hero.SetGuardPoints(guardPoints[i]);
            }
        }

        CheckForController();
        ClearStaleAttacks(monsters);

        _movementGenerator.AssignHeroMovement(_playerHeroes, monsters, _actionManager);

        if (_weGotADefenderController)
        {
            _spellGenerator.CastProtectiveShieldSpells(_playerHeroes, Strategy.Defend, _actionManager);
        }

        if (_weGotAnAttackerController)
        {
            _spellGenerator.CastProtectiveShieldSpells(_playerHeroes, Strategy.Attack, _actionManager);
        }

        _spellGenerator.AssignDefensiveWindSpell(_playerHeroes, monsters, _actionManager);

        if (!_inCollectionPhase)
        {
            if (playerMana > 100)
            {
                _spellGenerator.AssignDefenderControlSpells(_playerHeroes, monsters, _actionManager);
            }

            _spellGenerator.AssignAttackSpells(_playerHeroes, enemyHeroes, monsters, _actionManager);
        }

        return _actionManager.GetBestActions();
    }

    private void ResetHeroes()
    {
        foreach (Hero hero in _playerHeroes)
        {
            hero.IsShielding = false;
        }
    }

    private void CheckForPhaseChange(int mana)
    {
        if (_inCollectionPhase)
        {

            if(mana > 300 || (_alreadyAttacked && mana > 100))
            {
                _inCollectionPhase = false;
                _alreadyAttacked = true;

                ClearGuardPoints();
                ChangeCollectorToAttacker();
            }
        }
        else
        {
            if(mana <= 10)
            {
                _inCollectionPhase = true;

                ClearGuardPoints();
                ChangeAttackerToCollector();
            }
        }
    }

    private void ClearGuardPoints()
    {
        foreach (Hero hero in _playerHeroes)
        {
            hero.ClearGuardPoints();
        }
    }

    private void ChangeCollectorToAttacker()
    {
        IEnumerable<Hero> heroes = _playerHeroes.Where(h => h.Strategy == Strategy.Collect);

        foreach (Hero? hero in heroes)
        {
            hero.Strategy = Strategy.Attack;
        }
    }

    private void ChangeAttackerToCollector()
    {
        IEnumerable<Hero> heroes = _playerHeroes.Where(h => h.Strategy == Strategy.Attack);

        foreach (Hero? hero in heroes)
        {
            hero.Strategy = Strategy.Collect;
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
        foreach (Hero hero in _playerHeroes)
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
        foreach (Hero? hero in _playerHeroes.Where(h => h.Strategy == Strategy.Defend))
        {
            if (hero.CurrentMonster >= 0)
            {
                if (CalculateDistance(hero.Position, _playerBaseLocation) > _valuesProvider.MaxDefenderDistanceFromBase)
                {
                    hero.CurrentMonster = -1;
                }
            }
        }
    }

    private void ClearMonstersIfTheyreAThreatForTheEnemy(IReadOnlyCollection<Monster> monsters)
    {
        foreach (Hero hero in _playerHeroes)
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
        foreach (Hero? hero in _playerHeroes.Where(h => h.Strategy == Strategy.Attack))
        {
            if (hero.CurrentMonster >= 0)
            {
                Monster currentMonster = monsters.First(m => m.Id == hero.CurrentMonster);

                if (CalculateDistance(currentMonster.Position, _enemyBaseLocation) < _valuesProvider.OutskirtsMinDist
                    || CalculateDistance(currentMonster.Position, _enemyBaseLocation) > _valuesProvider.OutskirtsMaxDist)
                {
                    hero.CurrentMonster = -1;
                }
            }
        }
    }

    internal void UpdatePlayerHero(Hero hero)
    {
        Hero? playerHero = _playerHeroes.SingleOrDefault(h => h.Id == hero.Id);

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
