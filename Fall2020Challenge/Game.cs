namespace Fall2020Challenge; 

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

internal sealed class Game
{   
    internal List<Recipe> Recipes { get; private set; }
    internal List<Spell> Spells { get; private set; }
    internal List<Spell> TomeSpells { get; private set; }
    internal Inventory PlayerInventory { get; private set; }
    internal Inventory OpponentInventory { get; private set; }

    private List<string> currentPath = new List<string>();
    private int currentRecipe = -1;

    private int maxDepth = 10;

    private int turnCount = 0;
    
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

    public void SetTomeSpells(List<Spell> tomeSpells)
    {
        TomeSpells = tomeSpells;
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
        // Display.DisplaySpells(TomeSpells);

        // Very naieve but lets just get the first 6 spells
        if (turnCount < 6)
        {
            turnCount++;
            return $"LEARN {TomeSpells[0].Id}";
        }

        if (currentRecipe == -1 || !Recipes.Exists(r => r.Id == currentRecipe))
        {
            currentRecipe = -1;

            List<string>[] paths = new List<string>[Recipes.Count];

            // Go through each recipe, starting with the best scoring one
            for (int i = 0; i < Recipes.Count; i++)
            {  
                Recipe currentRecipe = Recipes[i];

                // If it can be brewed then brew it
                if (CanRecipeBeBrewed(currentRecipe))
                    return $"{Recipe.ActionType} {currentRecipe.Id}";

                // Find shortest path to ingredients
                paths[i] = FindShortestPath(PlayerInventory.Ingredients, currentRecipe.Ingredients, Spells);
            }

            Display.DisplayPaths(paths);

            // int bestIndex = GetQuickestPathIndex(paths);
            int bestIndex = GetMostExpensivePathIndex(paths);

            if (bestIndex >= 0)
            {
                List<string> bestPath = paths[bestIndex];

                if (bestPath.Count > 0)
                {
                    currentRecipe = Recipes[bestIndex].Id; ;
                    currentPath = bestPath;
                }
            }
        }

        string action = string.Empty;

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
                    action = $"CAST {Spells[i].Id}";
                }
            }
        }

        if (action == "")
        {
            Console.Error.WriteLine("RESTING");
            action = "REST";
        }

        turnCount++;

        return action;
    }

    private static int GetQuickestPathIndex(List<string>[] paths)
    {
        int quickest = int.MaxValue;
        int quickestIndex = -1;

        for (int i = 0; i < paths.Length; i++)
        {
            if (paths[i].Count > 0 && paths[i].Count < quickest)
            {
                quickest = paths[i].Count;

                quickestIndex = i;
            }
        }

        return quickestIndex;
    }

    private int GetMostExpensivePathIndex(List<string>[] paths)
    {
        int mostExpensive = int.MinValue;
        int mostExpensiveIndex = -1;

        for (int i = 0; i < paths.Length; i++)
        {
            if (paths[i].Count == 0)
                continue;

            var price = Recipes[i].Price;

            // The implementation of this bonus isn't 100% right. I'll fix it at some point:
            // Brewing a potion for the very first client awards a + 3 rupee bonus, but this can only happen 4 times during the game.
            // Brewing a potion for the second client awards a + 1 rupee bonus, but this also can only happen 4 times during the game.
            // If all + 3 bonuses have been used up, the + 1 bonus will be awarded by the first client instead of the second client.
            if (i == 0)
            {
                price += 3;
            }
            else if (i == 1)
            {
                price += 1;
            }

            if (price > mostExpensive)
            {
                mostExpensive = price;

                mostExpensiveIndex = i;
            }
        }

        return mostExpensiveIndex;
    }

    private bool CanRecipeBeBrewed(Recipe recipe)
    {
        return CanRecipeBeBrewed(recipe.Ingredients, PlayerInventory.Ingredients);
    }

    private static bool CanRecipeBeBrewed(int[] needed, int[] have)
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
        return CanSpellBeCast(spellIngredientsChange, inventoryIngredients, 1);
    }

    private static bool CanSpellBeCast(int[] spellIngredientsChange, int[] inventoryIngredients, int timesToCast)
    {
        int total = GetTotal(spellIngredientsChange, inventoryIngredients, timesToCast);

        if (total > 10)
            return false;

        bool haveIngredients = AreNeededIngredientsPresent(spellIngredientsChange, inventoryIngredients, timesToCast);
        
        if (!haveIngredients)
            return false;

        return true;
    }

    private static int GetTotal(int[] spellIngredientsChange, int[] inventoryIngredients, int timesToCast)
    {
        int total = 0;

        for (int times = 1; times <= timesToCast; times++)
        {
            for (int i = 0; i < 4; i++)
            {
                if (inventoryIngredients[i] + spellIngredientsChange[i] >= 0)
                {
                    total += inventoryIngredients[i] + spellIngredientsChange[i];
                }
            }
        }

        return total;
    }

    private static bool AreNeededIngredientsPresent(int[] spellIngredientsChange, int[] inventoryIngredients, int timesToCast)
    {
        int blue = inventoryIngredients[0];
        int green = inventoryIngredients[1];
        int orange = inventoryIngredients[2];
        int yellow = inventoryIngredients[3];

        for (int times = 1; times <= timesToCast; times++)
        {
            blue += spellIngredientsChange[0];
            green += spellIngredientsChange[1];
            orange += spellIngredientsChange[2];
            yellow += spellIngredientsChange[3];
        }

        if (blue < 0) return false;
        if (green < 0) return false;
        if (orange < 0) return false;   
        if (yellow < 0) return false;

        return true;
    }

    private List<string> FindShortestPath(int[] currentIngredients, int[] neededIngredients, List<Spell> availableSpells)
    {
        List<string> moves = new List<string>();

        for (int i = 0; i <= maxDepth; i++)
        {
            moves.Clear();
            int turns = DepthFirstSearch(currentIngredients, neededIngredients, availableSpells, 0, moves, i);

            if (turns != int.MaxValue)
            {
                break;
            }           
        }

        return moves;
    }

    public int DepthFirstSearch(int[] currentIngredients, int[] neededIngredients, List<Spell> availableSpells, int depth, List<string> moves, int maxDepth)
    {
        if (CanRecipeBeBrewed(neededIngredients, currentIngredients))
            return 0;
        
        if (depth == maxDepth)
        {
            return int.MaxValue;
        }

        int minTurns = int.MaxValue;

        string madeMove = string.Empty;
        bool[] castableBefore = new bool[availableSpells.Count];
        int[] ingredientsBefore = new int[4];

        // Get all possible actions
        // Give enough space to try multicasting all spells 3 times. We won't need tis many but lets use it any way
        int numberOfActions = (availableSpells.Count() * 3) + 1;
       
        for (int i = 0; i < numberOfActions; i++)
        {
            madeMove = string.Empty;

            if (i == numberOfActions-1)
            {
                if (availableSpells.Count(s => !s.Castable) > 0)
                {
                    // Make move
                    for (int k = 0; k < availableSpells.Count(); k++)
                    {
                        Spell spellToChange = availableSpells[k];

                        castableBefore[k] = availableSpells[k].Castable;

                        availableSpells[k].Castable = true;
                    }

                    madeMove = "REST";

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
                Spell currentSpell = availableSpells[i/3];
                int timesToCast = (i % 3) + 1;
                              
                // If it's not repeatable we only need to try 1 cast
                if (!currentSpell.Repeatable && timesToCast > 1)
                    continue;

                // do the changes from casting the spell
                if (currentSpell.Castable == true && CanSpellBeCast(currentSpell.IngredientsChange, currentIngredients, timesToCast))
                {
                    currentSpell.Castable = false;

                    
                    for (int j = 0; j < 4; j++)
                    {
                        ingredientsBefore[j] = currentIngredients[j];
                    }

                    for (int j = 0; j < 4; j++)
                    {
                        for (int times = 0; times < timesToCast; times++)
                        {
                            currentIngredients[j] += currentSpell.IngredientsChange[j];                      
                        }

                        if (currentIngredients[j] < 0)
                            currentIngredients[j] = 0;
                    }

                    madeMove = "CAST";
                    moves.Add($"CAST {currentSpell.Id} {timesToCast}");
                }
                else
                {
                    continue;
                }
            }

            int turns = DepthFirstSearch(currentIngredients, neededIngredients, availableSpells, depth + 1, moves, maxDepth);

            // unmake move
            if (madeMove == "REST")
            {
                for (int j = 0; j < availableSpells.Count(); j++)
                {
                    availableSpells[j].Castable = castableBefore[j];
                }
            } 
            else if (madeMove == "CAST")
            {
                for (int j = 0; j < 4; j++)
                {
                    currentIngredients[j] = ingredientsBefore[j];
                }
            }

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
}