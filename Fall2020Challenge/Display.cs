namespace Fall2020Challenge; 

using System;
using System.Collections.Generic;

internal static class Display
{
    internal static void DisplayRecipes(List<Recipe> recipes)
    {
        Console.Error.WriteLine("Recipes");

        foreach (Recipe recipe in recipes)
        {
            DisplayRecipe(recipe);
        }
    }

    internal static void DisplayRecipe(Recipe recipe)
    {
        Console.Error.WriteLine("actionId: " + recipe.Id);
        DisplayIngredients(recipe.Ingredients);
        Console.Error.WriteLine("Price: " + recipe.Price);
        Console.Error.WriteLine();
    }

    internal static void DisplayIngredients(int[] ingredients)
    {
        Console.Error.WriteLine("blueIngredients:   " + ingredients[0]);
        Console.Error.WriteLine("greenIngredients:  " + ingredients[1]);
        Console.Error.WriteLine("orangeIngredients: " + ingredients[2]);
        Console.Error.WriteLine("yellowIngredients: " + ingredients[3]);
    }

    internal static void DisplaySpells(List<Spell> spells)
    {
        foreach (Spell spell in spells)
        {
            DisplaySpell(spell);
        }
    }

    internal static void DisplaySpell(Spell spell)
    {
        Console.Error.WriteLine($"actionId: {spell.Id}");
        Console.Error.WriteLine($"Castable: {spell.Castable}");
        Console.Error.WriteLine($"Repeatable: {spell.Repeatable}");
        DisplayIngredients(spell.IngredientsChange);
        Console.Error.WriteLine();        
    }

    internal static void DisplayMoves(List<string> moves)
    {
        foreach (string move in moves)
        {
            Console.Error.WriteLine(move);
        }
    }

    internal static void DisplayPaths(List<string>[] paths)
    {
        foreach (var path in paths)
        {
            Console.Error.WriteLine("-----------------------------");

            foreach (var p in path)
            {
                Console.Error.WriteLine(p);
            }

        }
    }
}
