namespace Fall2020Challenge; 

using System;
using System.Collections.Generic;

internal static class Display
{
    private static void DisplayRecipes(List<Recipe> recipes)
    {
        Console.Error.WriteLine("Recipes");

        foreach (var recipe in recipes)
        {
            DisplayRecipe(recipe);
        }
    }

    private static void DisplayRecipe(Recipe recipe)
    {
        Console.Error.WriteLine("actionId: " + recipe.Id);
        DisplayIngredients(recipe.Ingredients);
        Console.Error.WriteLine("Price: " + recipe.Price);
        Console.Error.WriteLine();
    }

    private static void DisplayIngredients(int[] ingredients)
    {
        Console.Error.WriteLine("blueIngredients:   " + ingredients[0]);
        Console.Error.WriteLine("greenIngredients:  " + ingredients[1]);
        Console.Error.WriteLine("orangeIngredients: " + ingredients[2]);
        Console.Error.WriteLine("yellowIngredients: " + ingredients[3]);
    }

    private static void DisplaySpells(List<Spell> spells)
    {
        foreach (var spell in spells)
        {
            Console.Error.WriteLine($"actionId: {spell.Id}");
            Console.Error.WriteLine($"Castable: {spell.Castable}");
            DisplayIngredients(spell.IngredientsChange);
            Console.Error.WriteLine();
        }
    }

    private static void DisplayMoves(List<string> moves)
    {
        foreach (var move in moves)
        {
            Console.Error.WriteLine(move);
        }
    }
}
