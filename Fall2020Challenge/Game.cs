namespace Fall2020Challenge; 

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

internal sealed class Game
{   
    internal List<Recipe> Recipes { get; private set; }
    internal List<Spell> Spells { get; private set; }
    internal Inventory PlayerInventory { get; private set; }
    internal Inventory OpponentInventory { get; private set; }

    private List<string> currentPath = new List<string>();
    private int currentRecipe = -1;

    private int maxDepth = 0;
    
    public Game()
    {
        Recipes = new List<Recipe>();
    }
    
    public void SetRecipes(List<Recipe> recipes)
    {
        Recipes = recipes;
    }
    
    public void SetSpells(List<Spell> spells)
    {
        Spells = spells;
    }
    
    public void SetPlayerInventory(Inventory inventoryItems)
    {
        PlayerInventory = inventoryItems;
    }
    
    public void SetOpponentInventory(Inventory inventoryItems)
    {
        OpponentInventory = inventoryItems;
    }
    public string GetAction()
    {
        if (currentRecipe == -1 || !Recipes.Exists(r => r.Id == currentRecipe))
        {
            Console.Error.WriteLine("FINDING RECIPE");

            maxDepth = 8;

            currentRecipe = -1;

            var paths = new List<string>[Recipes.Count];

            // Go through each recipe, starting with the best scoring one
            for (int i = 0; i < Recipes.Count; i++)
            {
                var currentRecipe = Recipes[i];

                // If it can be brewed then brew it
                if (CanRecipeBeBrewed(currentRecipe))
                    return $"{Recipe.ActionType} {currentRecipe.Id}";

                // Find shortest path to ingredients
                paths[i] = FindShortestPath(PlayerInventory.Ingredients, currentRecipe.Ingredients, Spells);
            }

            // Lets try going for quickest first
            var quickestPath = new List<string>();
            var quickest = int.MaxValue;
            var quickestRecipe = 0;

            for (int i = 0; i < paths.Length; i++)
            {
                if (paths[i].Count > 0 && paths[i].Count < quickest)
                {
                    quickest = paths[i].Count;
                    quickestPath = paths[i];
                    quickestRecipe = Recipes[i].Id;
                }
            }


            if (quickestPath.Count > 0)
            {
                currentRecipe = quickestRecipe;
                currentPath = quickestPath;
            }

            Console.Error.WriteLine($"currentRecipe: {currentRecipe}");
        }

        var action = string.Empty;

        if (currentRecipe != -1)
        {
            if (currentPath.Count > 0)
            {
                action = currentPath[0];
                currentPath.RemoveAt(0);
            }
            else
            {
                action = $"BREW {currentRecipe}";
                currentPath = new List<string>();
                currentRecipe = -1;

            }
        }

        // If we haven't found an action just do something
        if (action == "")
        {
            for (int i = 0; i < Spells.Count; i++)
            {
                if (Spells[i].Castable && CanSpellBeCast(Spells[i].IngredientsChange, PlayerInventory.Ingredients))
                {
                    return $"CAST {Spells[i].Id}";
                }
            }
        }

        if (action == "")
        {
            return "REST";
        }
        
        return action;
    }

    private bool CanRecipeBeBrewed(Recipe recipe)
    {
        return CanRecipeBeBrewed(recipe.Ingredients, PlayerInventory.Ingredients);
    }

    private bool CanRecipeBeBrewed(int[] needed, int[] have)
    {
        if (have[0] >= needed[0]
            && have[1] >= needed[1]
            && have[2] >= needed[2]
            && have[3] >= needed[3])
        {
            return true;
        }

        return false;
    }

    private static bool CanSpellBeCast(int[] spellIngredientsChange, int[] inventoryIngredients)
    {
        var total = GetTotal(spellIngredientsChange, inventoryIngredients);

        var haveIngredients = AreNeededIngredientsPresent(spellIngredientsChange, inventoryIngredients);
        

        if (total > 10 || !haveIngredients)
        {
            return false;
        }

        return true;

    }

    private static int GetTotal(int[] spellIngredientsChange, int[] inventoryIngredients)
    {
        var total = 0;

        for (int i = 0; i < 4; i++)
        {
            if (inventoryIngredients[i] + spellIngredientsChange[i] >= 0)
            {
                total += inventoryIngredients[i] + spellIngredientsChange[i];
            }
        }

        return total;
    }

    private static bool AreNeededIngredientsPresent(int[] spellIngredientsChange, int[] inventoryIngredients)
    {
        return inventoryIngredients[0] + spellIngredientsChange[0] >= 0
            && inventoryIngredients[1] + spellIngredientsChange[1] >= 0
            && inventoryIngredients[2] + spellIngredientsChange[2] >= 0
            && inventoryIngredients[3] + spellIngredientsChange[3] >= 0;
    }
    
    private int[] CalculateNeededIngredientsAfterChange(IReadOnlyList<int> targetIngredients, IReadOnlyList<int> ingredientsChange)
    {
        var blueDiff = targetIngredients[0] - (PlayerInventory.Ingredients[0] + ingredientsChange[0]);
        var greenDiff = targetIngredients[1] - (PlayerInventory.Ingredients[1] + ingredientsChange[1]);
        var orangeDiff = targetIngredients[2] - (PlayerInventory.Ingredients[2] + ingredientsChange[2]);
        var yellowDiff = targetIngredients[3] - (PlayerInventory.Ingredients[3] + ingredientsChange[3]);
        
        var blueNeeds = blueDiff >= 0 ? blueDiff : 0;
        var greenNeeds = greenDiff >= 0 ? greenDiff : 0;
        var orangeNeeds = orangeDiff >= 0 ? orangeDiff : 0;
        var yellowNeeds = yellowDiff >= 0 ? yellowDiff : 0;

        return new []
        {
            blueNeeds,
            greenNeeds,
            orangeNeeds,
            yellowNeeds
        };
    }

    private List<string> FindShortestPath(int[] currentIngredients, int[] neededIngredients, List<Spell> availableSpells)
    {
        var actionCount = 0;

        // get all possible actions (all castable spells plus rest)
        // for each action
        //    Do the action

        var moves = new List<string>();

        for (int i = 0; i <= maxDepth; i++)
        {
            moves.Clear();
            var turns = MiniMax(currentIngredients, neededIngredients, availableSpells, 0, moves, i);

            if (turns != int.MaxValue)
            {
                Console.Error.WriteLine($"Breaking at {i}");

                // We care about the quickest ones right now. Don't look beyond something we've already found
                maxDepth = i;
                break;
            }
        }

        //DisplayMoves(moves);

        return moves;
    }

    public int MiniMax(int[] currentIngredients, int[] neededIngredients, List<Spell> availableSpells, int depth, List<string> moves, int maxDepth)
    {
        if (CanRecipeBeBrewed(neededIngredients, currentIngredients))
        {
            return 0;
        }

        if (depth == maxDepth)
        {
            // No solution found
            return int.MaxValue;
        }

        int minTurns = int.MaxValue;
        // Get all possible actions
        for (int i = 0; i <= availableSpells.Count(); i++)
        {
            var currentMove = string.Empty;
            var changedIngredients = currentIngredients.ToArray();

            var copiedSpells = new Spell[availableSpells.Count];

            availableSpells.CopyTo(copiedSpells);
            var changedSpells = copiedSpells.ToList();

            if (i == changedSpells.Count())
            {
                if (changedSpells.Count(s => !s.Castable) > 0)
                {
                    for (int k = 0; k < changedSpells.Count(); k++)
                    {
                        var spellToChange = changedSpells[k];

                        spellToChange.Castable = true;

                        changedSpells.RemoveAt(k);
                        changedSpells.Insert(k, spellToChange);
                    }

                    moves.Add("REST");
                }
                else
                {
                    // if all spells are active don't try this
                    continue;
                }
            }
            else
            {
                // do the changes from casting the spell
                if (changedSpells[i].Castable == true && CanSpellBeCast(changedSpells[i].IngredientsChange, changedIngredients))
                {
                    var currentSpell = changedSpells[i];
                    currentSpell.Castable = false;

                    changedSpells.RemoveAt(i);
                    changedSpells.Insert(i, currentSpell);

                    for (int j = 0; j < 4; j++)
                    {
                        changedIngredients[j] += currentSpell.IngredientsChange[j];
                        if (changedIngredients[j] < 0)
                            changedIngredients[j] = 0;
                    }

                    moves.Add($"CAST {currentSpell.Id}");
                }
                else
                {
                    continue;
                }
            }

            // DisplaySpells(changedSpells);

            int turns = MiniMax(changedIngredients, neededIngredients, changedSpells, depth + 1, moves, maxDepth);

            if (turns == 0)
            {
                return 0;
            }

            if (turns != int.MaxValue && turns + 1 < minTurns)
            {
                minTurns = turns + 1;
            } 
            else
            {
                moves.RemoveAt(depth);
            }            
        }

        return minTurns;
    }

    private void DisplayRecipes()
    {
        Console.Error.WriteLine("Recipes");
        
        foreach (var recipe in Recipes)
        {
            DisplayRecipe(recipe);         
        }   
    }

    private void DisplayRecipe(Recipe recipe)
    {
        Console.Error.WriteLine("actionId: " + recipe.Id);
        DisplayIngredients(recipe.Ingredients);
        Console.Error.WriteLine("Price: " + recipe.Price);
        Console.Error.WriteLine();
    }

    private void DisplayIngredients(int[] ingredients)
    {
        Console.Error.WriteLine("blueIngredients:   " + ingredients[0]);
        Console.Error.WriteLine("greenIngredients:  " + ingredients[1]);
        Console.Error.WriteLine("orangeIngredients: " + ingredients[2]);
        Console.Error.WriteLine("yellowIngredients: " + ingredients[3]);
    }

    private void DisplaySpells()
    {
        Console.Error.WriteLine("Spells");

        DisplaySpells(Spells);
    }

    private void DisplaySpells(List<Spell> spells)
    {
        foreach (var spell in spells)
        {
            Console.Error.WriteLine($"actionId: {spell.Id}");
            Console.Error.WriteLine($"Castable: {spell.Castable}");
            DisplayIngredients(spell.IngredientsChange);
            Console.Error.WriteLine();
        }
    }

    private void DisplayMoves(List<string> moves)
    {
        foreach (var move in moves)
        {
            Console.Error.WriteLine(move);
        }
    }
}