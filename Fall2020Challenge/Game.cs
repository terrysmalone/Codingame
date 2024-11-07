﻿namespace Fall2020Challenge; 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

internal sealed class Game
{   
    internal List<Recipe> Recipes { get; private set; }
    internal List<Spell> Spells { get; private set; }
    internal Inventory PlayerInventory { get; private set; }
    internal Inventory OpponentInventory { get; private set; }
    
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
        //DisplayRecipes();
        // DisplaySpells();

        // Go through each recipe, starting with the best scoring one
        //foreach (var recipe in Recipes.OrderByDescending(s => s.Price))
        //{

        var recipe = Recipes[4];
            Console.Error.WriteLine("Analysing recipe");
            DisplayRecipe(recipe);

            // If it can be brewed then brew it
            if (CanRecipeBeBrewed(recipe))
            {
                return $"{Recipe.ActionType} {recipe.Id}";
            }

            // Work out how many turns until we can brew it

            // What changes do we need to brew it
            //var neededIngredients = CalculateNeededIngredients(recipe.Ingredients, PlayerInventory.Ingredients);

            //Console.Error.WriteLine($"Needed ingredients");
            //DisplayIngredients(neededIngredients);

            // Find shortest path to ingredients
            var path = FindShortestPath(PlayerInventory.Ingredients, recipe.Ingredients, Spells);

            Console.Error.WriteLine("------------------------------------");
       // }









        // Pick the most expensive spell
        //var targetRecipe = Recipes.OrderByDescending(s => s.Price).First();

        //Console.Error.WriteLine($"targetRecipe: {targetRecipe.Id}");

        //var bestSpellId = -1;
        //var lowestNeeds = int.MaxValue;

        //foreach (var spell in Spells.Where(s => s.Castable))
        //{
        //    if(!CanSpellBeCast(spell.IngredientsChange)) { continue; }

        //    var needsAfterSpell = CalculateNeededIngredientsAfterChange(targetRecipe.Ingredients, spell.IngredientsChange);

        //    Console.Error.WriteLine($"Needs after spell {spell.Id}: [{needsAfterSpell[0]},{needsAfterSpell[1]},{needsAfterSpell[2]},{needsAfterSpell[3]}]");
        //    var totalNeedsAfterSpell = needsAfterSpell[0] + needsAfterSpell[1] + needsAfterSpell[2] + needsAfterSpell[3];

        //    // If this spell makes it brewable just cast it
        //    if(totalNeedsAfterSpell == 0)
        //    {
        //        return $"{Spell.ActionType} {spell.Id}";
        //    }

        //    // Check if this spell makes it more castable
        //    if(totalNeedsAfterSpell < lowestNeeds)
        //    {
        //        lowestNeeds = totalNeedsAfterSpell;
        //        bestSpellId = spell.Id;
        //    }
        //}

        //if(bestSpellId != -1)
        //{
        //    return $"{Spell.ActionType} {bestSpellId}";
        //}

        //return $"{Spell.ActionType} {Spells.First(s => s.Castable).Id}";
        // return "REST";
        return "CAST 80";

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

    private bool CanSpellBeCast(int[] spellIngredientsChange)
    {
        var inventoryIngredients = PlayerInventory.Ingredients;

        return CanSpellBeCast(spellIngredientsChange, inventoryIngredients);
    }

    private bool CanSpellBeCast(int[] spellIngredientsChange, int[] inventoryIngredients)
    {
        //return inventoryIngredients[0] + spellIngredientsChange[0] >= 0
        //          && inventoryIngredients[1] + spellIngredientsChange[1] >= 0
        //          && inventoryIngredients[2] + spellIngredientsChange[2] >= 0
        //          && inventoryIngredients[3] + spellIngredientsChange[3] >= 0;

        return true;

    }

    private int[] CalculateNeededIngredients(int[] targetIngredients, int[] currentIngredients)
    {
        var blueDiff = targetIngredients[0] - currentIngredients[0];
        var greenDiff = targetIngredients[1] - currentIngredients[1];
        var orangeDiff = targetIngredients[2] - currentIngredients[2];
        var yellowDiff = targetIngredients[3] - currentIngredients[3];

        var blueNeeds = blueDiff >= 0 ? blueDiff : 0;
        var greenNeeds = greenDiff >= 0 ? greenDiff : 0;
        var orangeNeeds = orangeDiff >= 0 ? orangeDiff : 0;
        var yellowNeeds = yellowDiff >= 0 ? yellowDiff : 0;

        return new[]
        {
            blueNeeds,
            greenNeeds,
            orangeNeeds,
            yellowNeeds
        };
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

        var turns = MiniMax(currentIngredients, neededIngredients, availableSpells, 0, moves);

        Console.Error.WriteLine($"Turns: {turns}");

        Console.Error.WriteLine($"Moves: {moves.Count}");
        DisplayMoves(moves);

        return null;
    }

    public int MiniMax(int[] currentIngredients, int[] neededIngredients, List<Spell> availableSpells, int depth, List<string> moves)
    {
        if (CanRecipeBeBrewed(neededIngredients, currentIngredients))
        {
            Console.Error.WriteLine("SOLUTION FOUND");

            foreach (var move in moves)
            {
                Console.Error.WriteLine(move);
            }

            return 0;
        }

        if (depth == 4)
        {
            Console.Error.WriteLine("HIT MAX DEPTH");
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
                    Console.Error.WriteLine($"Added {moves[depth]} at {depth}");
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
                    Console.Error.WriteLine($"Added {moves[depth]} at {depth}");
                }
                else
                {
                    continue;
                }
            }

            // DisplaySpells(changedSpells);

            int turns = MiniMax(changedIngredients, neededIngredients, changedSpells, depth + 1, moves);

            if (turns == 0)
            {
                Console.Error.WriteLine("SOLUTION STILL FOUND");
                return 0;
            }

            if (turns != int.MaxValue && turns + 1 < minTurns)
            {
                minTurns = turns + 1;
            } 
            else
            {
                Console.Error.WriteLine($"Removed {moves[depth]} at {depth}");
                //moves.RemoveRange(depth, moves.Count - 1 - depth);
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