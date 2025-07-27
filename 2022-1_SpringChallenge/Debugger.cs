using System;
using System.Collections.Generic;
using System.Linq;

namespace SpringChallenge2022;

internal static class Debugger
{
    internal static void DisplayMonsters(List<Monster> monsters)
    {
        Console.Error.WriteLine("Monsters");
        Console.Error.WriteLine("------------------------");

        foreach (Monster monster in monsters)
        {
            Console.Error.WriteLine($"{monster.Id}: Position-{monster.Position.X},{monster.Position.Y} - ThreatFor:{monster.ThreatFor} - IsControlled={monster.IsControlled} - near base:{monster.NearBase} - ThreatFor:{monster.ThreatFor}");
        }

        Console.Error.WriteLine("------------------------");
    }

    internal static void DisplayPlayerHeroes(List<Hero> heroes)
    {
        Console.Error.WriteLine("Player heroes");
        Console.Error.WriteLine("------------------------");

        foreach (Hero hero in heroes)
        {
            Console.Error.WriteLine($"{hero.Id}: Postion:({hero.Position.X},{hero.Position.Y}) - Current monster:{hero.CurrentMonster} - isShielding:{hero.IsShielding}");
        }

        Console.Error.WriteLine("------------------------");
    }

    internal static void DisplayEnemyHeroes(List<Hero> heroes)
    {
        Console.Error.WriteLine("Enemy heroes");
        Console.Error.WriteLine("------------------------");

        foreach (Hero hero in heroes)
        {
            Console.Error.WriteLine($"{hero.Id}: {hero.Position.X}, {hero.Position.Y}");
        }

        Console.Error.WriteLine("------------------------");
    }

    internal static void DisplayPossibleAction(List<PossibleAction> possibleActions, int playerOffset)
    {
        Console.Error.WriteLine("Possible actions");
        Console.Error.WriteLine("------------------------");

        for (int i = 0; i < 3; i++)
        {
            Console.Error.WriteLine($"Hero {i + playerOffset}");

            IOrderedEnumerable<PossibleAction> heroActions = possibleActions.Where(a => a.HeroId == i + playerOffset)
                                                                       .OrderByDescending(a => a.Priority);

            foreach (PossibleAction? action in heroActions)
            {
                Console.Error.WriteLine($"{action.Priority}:{action.ActionType} {action.EntityType} {action.TargetId} {action.TargetXPos} {action.TargetYPos}");
            }
        }

        Console.Error.WriteLine("------------------------");
    }
}
