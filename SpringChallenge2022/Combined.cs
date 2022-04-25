/**************************************************************
  This file was generated by FileConcatenator.
  It combined all classes in the project to work in Codingame.
  This hasn't been put in a namespace to allow for class 
  name duplicates.
***************************************************************/
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;


internal static class Debugger
{
    internal static void DisplayMonsters(List<Monster> monsters)
    {
        Console.Error.WriteLine("Monsters");
        Console.Error.WriteLine("------------------------");

        foreach (var monster in monsters)
        {
            Console.Error.WriteLine($"{monster.Id}: Position-{monster.Position.X},{monster.Position.Y} - ThreatFor:{monster.ThreatFor} - IsControlled={monster.IsControlled} - near base:{monster.NearBase} - ThreatFor:{monster.ThreatFor}");
        }

        Console.Error.WriteLine("------------------------");
    }

    internal static void DisplayPlayerHeroes(List<Hero> heroes)
    {
        Console.Error.WriteLine("Player heroes");
        Console.Error.WriteLine("------------------------");

        foreach (var hero in heroes)
        {
            Console.Error.WriteLine($"{hero.Id}: Postion:({hero.Position.X},{hero.Position.Y}) - Current monster:{hero.CurrentMonster} - isShielding:{hero.IsShielding}");
        }

        Console.Error.WriteLine("------------------------");
    }

    internal static void DisplayEnemyHeroes(List<Hero> heroes)
    {
        Console.Error.WriteLine("Enemy heroes");
        Console.Error.WriteLine("------------------------");

        foreach (var hero in heroes)
        {
            Console.Error.WriteLine($"{hero.Id}: {hero.Position.X}, {hero.Position.Y}");
        }

        Console.Error.WriteLine("------------------------");
    }
}


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


internal sealed class GuardPointGenerator
{
    private readonly Point _playerBaseLocation;

    private int _xMax;
    private int _yMax;
    public GuardPointGenerator(Point playerBaseLocation, int xMax, int yMax)
    {
        _playerBaseLocation = playerBaseLocation;
        _xMax = xMax;
        _yMax = yMax;
    }

    internal List<List<Point>> GetGuardPoints(List<Hero> playerHeroes)
    {
        var guardPoints = new List<List<Point>>();


        guardPoints.AddRange(GetDefenders(playerHeroes));

        guardPoints.AddRange(GetCollectors(playerHeroes));

        guardPoints.AddRange(GetAttackers(playerHeroes));

        // Set guard points
        if (playerHeroes.Count != guardPoints.Count)
        {
            Console.Error.WriteLine("ERROR: Player heroes count doesn't match guard point count");
        }

        // At some point we need to make sure we move heroes around to minimise travel to new spots

        return guardPoints;
    }

    private List<List<Point>> GetDefenders(List<Hero> playerHeroes)
    {
        var numberOfDefenders = playerHeroes.Count(h => h.Strategy == Strategy.Defend);

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

    private IEnumerable<List<Point>> GetCollectors(List<Hero> playerHeroes)
    {
        var numberOfCollectors = playerHeroes.Count(h => h.Strategy == Strategy.Collect);

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

    private List<List<Point>> GetAttackers(List<Hero> playerHeroes)
    {
        var numberOfAttackers = playerHeroes.Count(h => h.Strategy == Strategy.Attack);

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

}


internal sealed class Hero
{
    private int _currentGuardPoint = 0;
    private List<Point> _guardPoints;

    internal int Id { get; }
    internal Point Position { get; set; }

    internal int CurrentMonster { get; set; } = -1;

    internal string CurrentAction { get; set; } = "WAIT";

    internal bool UsingSpell {get; set; }

    internal bool IsControlled { get; set; }

    internal int ShieldLife { get; set; }

    internal Strategy Strategy { get; set;} = Strategy.Defend;

    internal  bool IsShielding { get; set; }

    internal Hero(int id, Point position, bool isControlled, int shieldLife)
    {
        Id = id;
        Position = position;
        IsControlled = isControlled;
        ShieldLife = shieldLife;

        _guardPoints = new List<Point>();
    }


    internal void SetGuardPoints(List<Point> guardPoints)
    {
        Console.Error.WriteLine($"guardPoints.Count: {guardPoints.Count}");
        _guardPoints = new List<Point>(guardPoints);
    }

    internal Point GetCurrentGuardPoint()
    {
        return new Point(_guardPoints[_currentGuardPoint].X, _guardPoints[_currentGuardPoint].Y);
    }

    internal Point GetNextGuardPoint()
    {
        if (_currentGuardPoint >= _guardPoints.Count - 1)
        {
            _currentGuardPoint = 0;
        }
        else
        {
            _currentGuardPoint++;
        }

        return new Point(_guardPoints[_currentGuardPoint].X, _guardPoints[_currentGuardPoint].Y);
    }

    internal int GetNumberOfGuardPoints()
    {
        return _guardPoints.Count;
    }

    internal void ClearGuardPoints()
    {
        _guardPoints = new List<Point>();
    }
}


internal sealed class Monster
{
    public int Id { get; }
    public Point Position { get; }
    public int Health { get; }
    public int SpeedX { get; }
    public int SpeedY { get; }
    public bool NearBase { get; }
    public ThreatFor ThreatFor { get; }
    public int ShieldLife { get; }
    public bool IsControlled { get; }
    
    public Monster(int id, Point position, int health, int speedX, int speedY, bool nearBase, ThreatFor threatFor, bool isControlled, int shieldLife)
    {
        Id = id;
        Position = position;
        Health = health;
        SpeedX = speedX;
        SpeedY = speedY;
        NearBase = nearBase;
        ThreatFor = threatFor;
        IsControlled = isControlled;
        ShieldLife = shieldLife;
    }
}


internal sealed class MovementGenerator
{
    private readonly Point _playerBaseLocation;
    private readonly Point _enemyBaseLocation;

    private readonly int _baseRadius;
    private readonly int _maxDefenderDistanceFromBase;

    private readonly int _outskirtsMinDist;
    private readonly int _outskirtsMaxDist;

    private readonly int _heroRange;



    public MovementGenerator(Point playerBaseLocation,
                             Point enemyBaseLocation,
                             int baseRadius,
                             int maxDefenderDistanceFromBase,
                             int outskirtsMinDist,
                             int outskirtsMaxDist,
                             int heroRange)
    {
        _playerBaseLocation = playerBaseLocation;
        _enemyBaseLocation = enemyBaseLocation;
        _baseRadius = baseRadius;
        _maxDefenderDistanceFromBase = maxDefenderDistanceFromBase;
        _outskirtsMinDist = outskirtsMinDist;
        _outskirtsMaxDist = outskirtsMaxDist;
        _heroRange = heroRange;
    }

    internal void AssignHeroMovement(List<Hero> playerHeroes, List<Monster> monsters)
    {
        var defendingHeroesOutsideOfBase = playerHeroes.Where(h => h.Strategy == Strategy.Defend
                                                                   && CalculateDistance(h.Position, _playerBaseLocation) > _baseRadius);

        foreach (var defendingHeroOutsideOfBase in defendingHeroesOutsideOfBase)
        {
            defendingHeroOutsideOfBase.CurrentMonster = -1;
        }

        CalculateDefenderMovement(playerHeroes, monsters);

        CalculateCollectorMovement(playerHeroes, monsters);

        CalculateAttackerMovement(playerHeroes, monsters);

        // Assign actions
        foreach (var hero in playerHeroes)
        {
            if (hero.CurrentMonster != -1)
            {
                var monsterToAttack = monsters.Single(m => m.Id == hero.CurrentMonster);

                hero.CurrentAction = $"MOVE {monsterToAttack.Position.X} {monsterToAttack.Position.Y}";
            }
            else
            {
                var currentGuardPoint = hero.GetCurrentGuardPoint();

                if (!(hero.Position.X == currentGuardPoint.X && hero.Position.Y == currentGuardPoint.Y))
                {
                    hero.CurrentAction = $"MOVE {currentGuardPoint.X} {currentGuardPoint.Y}";
                }
                else
                {
                    var nextGuardPoint = hero.GetNextGuardPoint();
                    hero.CurrentAction = $"MOVE {nextGuardPoint.X} {nextGuardPoint.Y}";
                }
            }
        }
    }
    private void CalculateAttackerMovement(IEnumerable<Hero> playerHeroes, IEnumerable<Monster> monsters)
    {
        var freeAttackingHeroes = playerHeroes.Where(h => h.Strategy == Strategy.Attack && h.CurrentMonster == -1).ToList();

        if (freeAttackingHeroes.Count <= 0)
        {
            return;
        }

        // Get any monsters on the edge of the enemies base
        var monstersOnOutskirts = monsters.Where(m => CalculateDistance(m.Position, _enemyBaseLocation) > _outskirtsMinDist
                                                      && CalculateDistance(m.Position, _enemyBaseLocation) < _outskirtsMaxDist).ToList();

        // Go to them
        if (monstersOnOutskirts.Count > 1)
        {
            foreach (var freeAttackingHero in freeAttackingHeroes)
            {
                var closestMonster = monstersOnOutskirts.OrderBy(m => CalculateDistance(freeAttackingHero.Position, m.Position))
                                                        .First();

                freeAttackingHero.CurrentMonster = closestMonster.Id;
            }
        }
    }

    private void CalculateDefenderMovement(IReadOnlyCollection<Hero> playerHeroes, List<Monster> monsters)
    {
        Debugger.DisplayMonsters(monsters);

        // if a hero is not in the base, and a spider is, drop everything and defend
        var monstersThreateningBase = monsters.Where(m => m.ThreatFor == ThreatFor.Player
                                                                         && CalculateDistance(m.Position, _playerBaseLocation) <= 6000)
                                                         .OrderBy(m => CalculateDistance(m.Position, _playerBaseLocation))
                                                         .ToList();

        var freeDefendingHeroes = playerHeroes.Where(h => h.Strategy == Strategy.Defend && h.CurrentMonster == -1).ToList();

        if (monstersThreateningBase.Count == 0 && freeDefendingHeroes.Count <= 0)
        {
            return;
        }

        if (monstersThreateningBase.Count > 0)
        {
            var closestMonster = monstersThreateningBase.First();

            foreach (var hero in freeDefendingHeroes)
            {
                hero.CurrentMonster = closestMonster.Id;
            }
        }
        else
        {
            // Monsters on outskirts might have to be considered

            foreach (var freeDefendingHero in freeDefendingHeroes)
            {
                var monsterWithinRange = monsters.Where(m => CalculateDistance(m.Position, _playerBaseLocation) <= _maxDefenderDistanceFromBase
                                                             && m.ThreatFor != ThreatFor.Enemy)
                                                 .Select(m => new { m, distance = CalculateDistance(m.Position, freeDefendingHero.Position) })
                                                 .Where(m => m.distance <= _heroRange)
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

        var collectingHeroes = playerHeroes.Where(h => h.Strategy == Strategy.Collect && h.CurrentMonster == -1).ToList();

        if (collectingHeroes.Count > 0)
        {
            foreach (var collectingHero in collectingHeroes)
            {
                var closestMonster = monsters.Where(m => CalculateDistance(m.Position, _playerBaseLocation) > _outskirtsMaxDist)
                                             .Select(m => new { m, distance = CalculateDistance(m.Position, collectingHero.Position) })
                                             .Where(m => m.distance <= _heroRange)
                                             .OrderBy(m => m.distance)
                                             .Select(m => m.m)
                                             .FirstOrDefault();

                if (closestMonster != null)
                {
                    collectingHero.CurrentMonster = closestMonster.Id;
                }
                else
                {
                    var currentGuardPoint = collectingHero.GetCurrentGuardPoint();

                    if (!(collectingHero.Position.X == currentGuardPoint.X && collectingHero.Position.Y == currentGuardPoint.Y))
                    {
                        collectingHero.CurrentAction = $"MOVE {currentGuardPoint.X} {currentGuardPoint.Y}";
                    }
                    else
                    {
                        var nextGuardPoint = collectingHero.GetNextGuardPoint();
                        collectingHero.CurrentAction = $"MOVE {nextGuardPoint.X} {nextGuardPoint.Y}";
                    }
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



/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
internal sealed class Player
{
    static void Main(string[] args)
    {
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');

        var baseX = int.Parse(inputs[0]); // The corner of the map representing your base
        var baseY = int.Parse(inputs[1]);
        var heroesPerPlayer = int.Parse(Console.ReadLine()); // Always 3

        var game = new Game(new Point(baseX, baseY), heroesPerPlayer);

        // game loop
        while (true)
        {
            // Don't bother persisting monsters. It's quicker just to re-add them every time.
            // At least until we need to persist them
            var enemyHeroes = new List<Hero>();
            var monsters = new List<Monster>();

            // Player base stats
            inputs = Console.ReadLine().Split(' ');
            var playerBaseHealth = int.Parse(inputs[0]); // Your base health
            var playerMana = int.Parse(inputs[1]); // Ignore in the first league; Spend ten mana to cast a spell

            // enemy base stats
            inputs = Console.ReadLine().Split(' ');
            var enemyBaseHealth = int.Parse(inputs[0]); // Your base health
            var enemyMana = int.Parse(inputs[1]); // Ignore in the first league; Spend ten mana to cast a spell

            var entityCount = int.Parse(Console.ReadLine()); // Amount of heros and monsters you can see

            for (var i = 0; i < entityCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');

                var id = int.Parse(inputs[0]); // Unique identifier
                var type = int.Parse(inputs[1]); // 0=monster, 1=your hero, 2=opponent hero
                var x = int.Parse(inputs[2]); // Position of this entity
                var y = int.Parse(inputs[3]);
                var shieldLife = int.Parse(inputs[4]); // Ignore for this league; Count down until shield spell fades
                var isControlled = int.Parse(inputs[5]); // Ignore for this league; Equals 1 when this entity is under a control spell
                var health = int.Parse(inputs[6]); // Remaining health of this monster
                var vx = int.Parse(inputs[7]); // Trajectory of this monster
                var vy = int.Parse(inputs[8]);
                var nearBase = int.Parse(inputs[9]); // 0=monster with no target yet, 1=monster targeting a base
                var threatFor = int.Parse(inputs[10]); // Given this monster's trajectory, is it a threat to 1=your base, 2=your opponent's base, 0=neither

                if (type == 0)
                {
                    ThreatFor threatForEnum;

                    switch (threatFor)
                    {
                        case 0:
                            threatForEnum = ThreatFor.None;
                            break;
                        case 1:
                            threatForEnum = ThreatFor.Player;
                            break;
                        case 2:
                            threatForEnum = ThreatFor.Enemy;
                            break;
                        default:
                            threatForEnum = ThreatFor.None;
                            break;
                    }

                    monsters.Add(new Monster(id, new Point(x, y), health, vx, vy, nearBase != 0, threatForEnum, isControlled == 1, shieldLife));
                }
                else
                {
                    var hero = new Hero(id, new Point(x, y), isControlled == 1, shieldLife);

                    if (type == 1)
                    {
                        game.UpdatePlayerHero(hero);
                    }
                    else
                    {
                        enemyHeroes.Add(hero);
                    }
                }
            }

            var moves = game.GetMoves(enemyHeroes, monsters, playerMana);

            for (var i = 0; i < moves.Length; i++)
            {
                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");

                // In the first league: MOVE <x> <y> | WAIT; In later leagues: | SPELL <spellParams>;
                Console.WriteLine(moves[i]);
            }
        }
    }
}


internal sealed class SpellGenerator
{
    private int _estimatedManaLeft;
    
    private readonly Point _playerBaseLocation;
    private readonly Point _enemyBaseLocation;

    private readonly int _baseRadius;
    private readonly int _closeToBaseRange;
    private readonly int _outskirtsMinDist;
    private readonly int _outskirtsMaxDist;

    private readonly int _windSpellRange;
    private readonly int _controlSpellange;
    private readonly int _shieldSpellRange;

    public SpellGenerator(Point playerBaseLocation,
                          Point enemyBaseLocation,
                          int baseRadius,
                          int closeToBaseRange,
                          int outskirtsMinDist,
                          int outskirtsMaxDist,
                          int windSpellRange,
                          int controlSpellange,
                          int shieldSpellRange)
    {
        _playerBaseLocation = playerBaseLocation;
        _enemyBaseLocation = enemyBaseLocation;
        _baseRadius = baseRadius;
        _closeToBaseRange = closeToBaseRange;
        _outskirtsMinDist = outskirtsMinDist;
        _outskirtsMaxDist = outskirtsMaxDist;
        _windSpellRange = windSpellRange;
        _controlSpellange = controlSpellange;
        _shieldSpellRange = shieldSpellRange;
    }

    internal void CastProtectiveShieldSpells(IEnumerable<Hero> playerHeroes, Strategy strategy)
    {
        foreach (var hero in playerHeroes.Where(h => h.Strategy == strategy))
        {
            if (_estimatedManaLeft < 10)
            {
                break;
            }

            if (hero.ShieldLife == 0)
            {
                PerformSpell(hero, $"SPELL SHIELD {hero.Id}");

                hero.IsShielding = true;
            }
        }
    }

    internal void AssignDefensiveWindSpell(List<Hero> playerHeroes, IEnumerable<Monster> monsters)
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
            Debugger.DisplayPlayerHeroes(playerHeroes);

            var availableHeroes = playerHeroes.Where(h => h.Strategy == Strategy.Defend && h.IsShielding == false).ToList();

            if (availableHeroes.Count > 0)
            {
                var closestHero = availableHeroes.OrderBy(h => CalculateDistance(h.Position, closestMonster.Position))
                                                 .First();

                if (CalculateDistance(closestHero.Position, closestMonster.Position) <= _windSpellRange)
                {
                    Console.Error.WriteLine("Hero casting wind");
                    PerformSpell(closestHero, $"SPELL WIND {_enemyBaseLocation.X} {_enemyBaseLocation.Y}");
                }
                else
                {
                    // Too far away for wind to work

                    // If he's close and we can control that little shit away do it
                    if (CalculateDistance(closestMonster.Position, _playerBaseLocation) <= _closeToBaseRange
                        && CalculateDistance(closestHero.Position, closestMonster.Position) <= _controlSpellange)
                    {
                        Console.Error.WriteLine("Hero casting control");
                        PerformSpell(closestHero, $"SPELL CONTROL {closestMonster.Id} {_enemyBaseLocation.X} {_enemyBaseLocation.Y}");
                    }
                }
            }
        }
    }

    internal void AssignDefenderControlSpells(IEnumerable<Hero> playerHeroes, IEnumerable<Monster> monsters)
    {
        const int healthCutOff = 10;

        if (_estimatedManaLeft < 10)
        {
            return;
        }

        var defendingHeroesOutsideOfBase =
            playerHeroes.Where(h => h.Strategy == Strategy.Defend
                                      && h.IsShielding == false
                                      && CalculateDistance(h.Position, _playerBaseLocation) > _baseRadius);

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
                                        && CalculateDistance(m.Position, _playerBaseLocation) > _baseRadius)
                        .Select(m => new { m, distance = CalculateDistance(m.Position, defendingHeroOutsideOfBase.Position)})
                        .Where(m => m.distance <= _controlSpellange)
                        .OrderBy(m => m.distance)
                        .Select(m => m.m)
                        .FirstOrDefault();

            if (monsterWithinSpellRange != null)
            {
                PerformSpell(defendingHeroOutsideOfBase, $"SPELL CONTROL {monsterWithinSpellRange.Id} {_enemyBaseLocation.X} {_enemyBaseLocation.Y}");
            }
        }
    }

    internal void AssignAttackSpells(IEnumerable<Hero> playerHeroes, IEnumerable<Hero> enemyHeroes, IEnumerable<Monster> monsters)
    {
        foreach (var attackingHero in playerHeroes.Where(h => h.Strategy == Strategy.Attack))
        {
            if (_estimatedManaLeft < 10)
            {
                return;
            }

            if (CalculateDistance(attackingHero.Position, _enemyBaseLocation) > _outskirtsMaxDist)
            {
                continue;
            }

            var closeEnoughForWindMonster = monsters.FirstOrDefault(m => CalculateDistance(m.Position, attackingHero.Position) <= _windSpellRange
                                                                                 && m.ShieldLife == 0);

            if (closeEnoughForWindMonster != null)
            {
                Console.Error.WriteLine($"Atacking hero {attackingHero.Id} to cast WIND on monster {closeEnoughForWindMonster.Id}");

                PerformSpell(attackingHero, $"SPELL WIND {_enemyBaseLocation.X} {_enemyBaseLocation.Y}");
            }
            else // If we're not close enough for a wind spell try a shield or control
            {
                var closeEnoughForControlEnemy =
                        enemyHeroes.Where(e => e.ShieldLife == 0
                                                 && CalculateDistance(e.Position, attackingHero.Position) <= _controlSpellange
                                                 && CalculateDistance(e.Position, _enemyBaseLocation) <= _baseRadius)
                                   .OrderBy(e => CalculateDistance(e.Position, _enemyBaseLocation))
                                   .FirstOrDefault();

                var closeEnoughForSpellMonster =
                        monsters.FirstOrDefault(m => m.ShieldLife == 0
                                                         && m.ThreatFor == ThreatFor.Enemy
                                                         && CalculateDistance(m.Position, attackingHero.Position) <= _shieldSpellRange
                                                         && CalculateDistance(m.Position, _enemyBaseLocation) <= _outskirtsMinDist);

                if (closeEnoughForControlEnemy != null && closeEnoughForSpellMonster != null)
                {
                    if (new Random().Next(1) == 0)
                    {
                        PerformSpell(attackingHero, $"SPELL SHIELD {closeEnoughForSpellMonster.Id}");
                    }
                    else
                    {
                        PerformSpell(attackingHero, $"SPELL CONTROL {closeEnoughForControlEnemy.Id} {_playerBaseLocation.X} {_playerBaseLocation.Y}");
                    }
                }
                else if (closeEnoughForSpellMonster != null)
                {
                    PerformSpell(attackingHero, $"SPELL SHIELD {closeEnoughForSpellMonster.Id}");
                }
                else if (closeEnoughForControlEnemy != null)
                {
                    PerformSpell(attackingHero, $"SPELL CONTROL {closeEnoughForControlEnemy.Id} {_playerBaseLocation.X} {_playerBaseLocation.Y}");
                }
            }
        }
    }

    private void PerformSpell(Hero hero, string action)
    {
        hero.CurrentAction = action;
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


internal enum Strategy
{
    Defend,
    Attack,
    Collect
}


internal enum ThreatFor
{
    None,
    Player,
    Enemy
}