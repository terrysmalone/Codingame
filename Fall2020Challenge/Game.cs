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

            int quickest = GetQuickestPathIndex(paths);

            if (quickest >= 0)
            {
                List<string> quickestPath = paths[quickest];

                if (quickestPath.Count > 0)
                {
                    currentRecipe = Recipes[quickest].Id; ;
                    currentPath = quickestPath;
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
            action = "REST";
        }

        turnCount++;

        return action;
    }

    private int GetQuickestPathIndex(List<string>[] paths)
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
        int total = GetTotal(spellIngredientsChange, inventoryIngredients);

        bool haveIngredients = AreNeededIngredientsPresent(spellIngredientsChange, inventoryIngredients);
        

        if (total > 10 || !haveIngredients)
        {
            return false;
        }

        return true;

    }

    private static int GetTotal(int[] spellIngredientsChange, int[] inventoryIngredients)
    {
        int total = 0;

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
        for (int i = 0; i <= availableSpells.Count(); i++)
        {
            madeMove = string.Empty;

            if (i == availableSpells.Count())
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
                // do the changes from casting the spell
                if (availableSpells[i].Castable == true && CanSpellBeCast(availableSpells[i].IngredientsChange, currentIngredients))
                {
                    availableSpells[i].Castable = false;

                    for (int j = 0; j < 4; j++)
                    {
                        ingredientsBefore[j] = currentIngredients[j];
                    }

                    for (int j = 0; j < 4; j++)
                    {
                        currentIngredients[j] += availableSpells[i].IngredientsChange[j];
                        if (currentIngredients[j] < 0)
                            currentIngredients[j] = 0;
                    }

                    madeMove = "CAST";
                    moves.Add($"CAST {availableSpells[i].Id}");
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