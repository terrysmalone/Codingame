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
            Console.Error.WriteLine($"{monster.Id}: {monster.Position.X}, {monster.Position.Y}");
        }

        Console.Error.WriteLine("------------------------");
    }

    internal static void DisplayPlayerHeroes(List<Hero> heroes)
    {
        Console.Error.WriteLine("Player heroes");
        Console.Error.WriteLine("------------------------");

        foreach (var hero in heroes)
        {
            Console.Error.WriteLine($"{hero.Id}: Postion:({hero.Position.X},{hero.Position.Y}) - Current monster:{hero.CurrentMonster}");
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
    private readonly int _heroesPerPlayer;

    private int _playerBaseHealth;
    private int _enemyBaseHealth;

    private int _mana;

    private List<Monster> _monsters = new List<Monster>();
    private List<Hero> _playerHeroes = new List<Hero>();
    private List<Hero> _enemyHeroes = new List<Hero>();

    internal Game(Point playerBaseLocation, int heroesPerPlayer)
    {
        _playerBaseLocation = playerBaseLocation;
        _heroesPerPlayer = heroesPerPlayer;
    }

    internal string[] GetMoves()
    {
        var moves = new string[_heroesPerPlayer];

        //Debugger.DisplayPlayerHeroes(_playerHeroes);
        //Debugger.DisplayEnemyHeroes(_enemyHeroes);
        //Debugger.DisplayMonsters(_monsters);

        SetGuardPoints();

        ClearDeadMonsters();

        AssignMonstersToAttack();

        AsignWindSpell();

        Debugger.DisplayPlayerHeroes(_playerHeroes);

        for (var i = 0; i < moves.Length; i++)
        {
            moves[i] = _playerHeroes[i].CurrentAction;
        }

        return moves;
    }

    private static double CalculateDistance(Point position, Point position2)
    {
        return Math.Sqrt(Math.Pow(position.X - position2.X, 2)
                         + Math.Pow(position.Y - position2.Y, 2));
    }

    private void SetGuardPoints()
    {
        var xMax = 17630;
        var yMax = 9000;

        if (_playerHeroes[0].GuardPoint.X == 0 && _playerHeroes[0].GuardPoint.Y == 0)
        {
            Console.Error.WriteLine($"_playerBaseLocation.X:{_playerBaseLocation.X }");
            if (_playerBaseLocation.X == 0)
            {
                _playerHeroes[0].GuardPoint = new Point(4000, 1000);
                _playerHeroes[1].GuardPoint = new Point(3000, 3000);
                _playerHeroes[2].GuardPoint = new Point(1000, 4000);
            }
            else
            {
                _playerHeroes[0].GuardPoint = new Point(xMax - 4000, yMax - 1000);
                _playerHeroes[1].GuardPoint = new Point(xMax - 3000, yMax - 3000);
                _playerHeroes[2].GuardPoint = new Point(xMax - 1000, yMax - 4000);
            }
        }
    }

    private void ClearDeadMonsters()
    {
        foreach (var hero in _playerHeroes)
        {
            if (hero.CurrentMonster >= 0)
            {
                if (!_monsters.Any(m => m.Id == hero.CurrentMonster))
                {
                    hero.CurrentMonster = -1;
                }
            }
        }
    }

    private void AssignMonstersToAttack()
    {
        List<Monster> viableMonsters;

        viableMonsters = _monsters.Where(m => m.NearBase && m.ThreatFor == ThreatFor.Player)
                                  .OrderBy(m => CalculateDistance(m.Position, _playerBaseLocation))
                                  .ToList();

        var monsterIndex = 0;
        var freeHeroes = _playerHeroes.Where(h => h.CurrentMonster == -1).ToList();

        if (viableMonsters.Count > 0)
        {
            var closestMonster = viableMonsters.First();

            foreach (var hero in freeHeroes)
            {
                hero.CurrentMonster = closestMonster.Id;
            }
        }

        // This only assigns one one hero per monster per turn
        //
        // while (freeHeroes.Count > 0 && monsterIndex < viableMonsters.Count)
        // {
        //     Console.Error.WriteLine("Assigning hero to monster");
        //
        //     var closestMonster = viableMonsters[monsterIndex];
        //
        //     var closestHero = freeHeroes.OrderBy(h => CalculateDistance(h.Position, closestMonster.Position)).First();
        //
        //     closestHero.CurrentMonster = closestMonster.Id;
        //
        //     Console.Error.WriteLine($"Assigning hero {closestHero.Id} to monster {closestMonster.Id}");
        //
        //     //viableMonsters.Remove(closestMonster);
        //     freeHeroes.Remove(closestHero);
        //     monsterIndex++;
        // }

        // Assign actions
        foreach (var hero in _playerHeroes)
        {
            if (hero.CurrentMonster != -1)
            {
                var hero1 = hero;
                var monsterToAttack = _monsters.Single(m => m.Id == hero1.CurrentMonster);

                hero.CurrentAction = $"MOVE {monsterToAttack.Position.X} {monsterToAttack.Position.Y}";
            }
            else
            {
                hero.CurrentAction = $"MOVE {hero.GuardPoint.X} {hero.GuardPoint.Y}";
            }
        }
    }

    private void AsignWindSpell()
    {
        if (_mana < 10)
        {
            return;
        }

        var closeDistance = 2000;
        var windDistance = 1280;

        var closestMonster = _monsters.FirstOrDefault(m => CalculateDistance(m.Position, _playerBaseLocation) <= closeDistance);

        if (closestMonster != null)
        {
            var closestHero = _playerHeroes.OrderBy(h => CalculateDistance(h.Position, closestMonster.Position)).First();

            if (CalculateDistance(closestHero.Position, closestMonster.Position) <= windDistance)
            {
                int xNew, yNew;

                Console.Error.WriteLine($"_playerBaseLocation.X:{_playerBaseLocation.X}");
                // very crude vector calc
                if (_playerBaseLocation.X == 0)
                {
                    xNew = closestHero.Position.X + 1;
                    yNew = closestHero.Position.Y + 1;
                }
                else
                {
                    xNew = closestHero.Position.X - 1;
                    yNew = closestHero.Position.Y - 1;
                }

                closestHero.CurrentAction = $"SPELL WIND {xNew} {yNew}";
            }
        }
    }

    internal void SetMana(int mana)
    {
        _mana = mana;
    }

    internal void SetPlayerBaseHealth(int playerBaseHealth)
    {
        _playerBaseHealth = playerBaseHealth;
    }

    internal void SetEnemyBaseHealth(int playerBaseHealth)
    {
        _enemyBaseHealth = playerBaseHealth;
    }

    public void UpdatePlayerHero(Hero hero)
    {
        var playerHero = _playerHeroes.SingleOrDefault(h => h.Id == hero.Id);

        if (playerHero == null)
        {
            _playerHeroes.Add(hero);
        }
        else
        {
            playerHero.Position = hero.Position;
        }
    }
    public void UpdateEnemyHero(Hero hero)
    {
        var enemyHero = _enemyHeroes.SingleOrDefault(h => h.Id == hero.Id);

        if (enemyHero == null)
        {
            _enemyHeroes.Add(hero);
        }
        else
        {
            enemyHero.Position = hero.Position;
        }
    }

    internal void AddMonster(Monster monster)
    {
        _monsters.Add(monster);
    }

    internal void ClearMonsters()
    {
        _monsters.Clear();
    }
}


internal sealed class Hero
{
    public int Id { get; }
    public Point Position { get; set; }

    public Point GuardPoint { get; set; }

    internal int CurrentMonster { get; set; } = -1;

    internal string CurrentAction { get; set; } = "WAIT";

    public Hero(int id, Point position)
    {
        Id = id;
        Position = position;
    }
}


internal sealed class Monster
{
    public int Id { get; }
    public Point Position { get; }
    public int Health { get; }
    public int XTrajectory { get; }
    public int YTrajectory { get; }
    public bool NearBase { get; }
    public ThreatFor ThreatFor { get; }
    
    public Monster(int id, Point position, int health, int xTrajectory, int yTrajectory, bool nearBase, ThreatFor threatFor)
    {
        Id = id;
        Position = position;
        Health = health;
        XTrajectory = xTrajectory;
        YTrajectory = yTrajectory;
        NearBase = nearBase;
        ThreatFor = threatFor;
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
            game.ClearMonsters();

            // Player base stats
            inputs = Console.ReadLine().Split(' ');
            var playerBaseHealth = int.Parse(inputs[0]); // Your base health
            var playerMana = int.Parse(inputs[1]); // Ignore in the first league; Spend ten mana to cast a spell
            game.SetMana(playerMana);

            Console.Error.WriteLine($"playerMana: {playerMana}");

            game.SetPlayerBaseHealth(playerBaseHealth);

            // enemy base stats
            inputs = Console.ReadLine().Split(' ');
            var enemyBaseHealth = int.Parse(inputs[0]); // Your base health
            var enemyMana = int.Parse(inputs[1]); // Ignore in the first league; Spend ten mana to cast a spell

            game.SetEnemyBaseHealth(playerBaseHealth);

            var entityCount = int.Parse(Console.ReadLine()); // Amount of heros and monsters you can see

            var playerHeroes = new List<Hero>();
            var enemyHeroes = new List<Hero>();

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
                    var threatForEnum = ThreatFor.None;

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
                    game.AddMonster(new Monster(id, new Point(x, y), health, vx, vy, nearBase != 0, threatForEnum));
                }
                else
                {
                    var hero = new Hero(id, new Point(x, y));

                    if (type == 1)
                    {
                        game.UpdatePlayerHero(hero);
                    }
                    else
                    {
                        game.UpdateEnemyHero(hero);
                    }
                }
            }

            var moves = game.GetMoves();

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

internal enum ThreatFor
{
    None,
    Player,
    Enemy
}
