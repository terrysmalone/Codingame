using System;
using System.Collections.Generic;

namespace SpringChallenge2022;

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
            Console.Error.WriteLine($"{hero.Id}: {hero.Position.X}, {hero.Position.Y}");
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
