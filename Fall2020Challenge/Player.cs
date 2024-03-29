﻿using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace Fall2020Challenge
{
    internal sealed class Player
    {
        static void Main(string[] args)
        {
            var game = new Game();
            
            // game loop
            while (true)
            {
                ParseActions(game);

                game.SetPlayerInventory(GetInventoryItems());
                game.SetOpponentInventory(GetInventoryItems());
                
                //DisplayInventory(game.PlayerInventory, true);
                //DisplayInventory(game.OpponentInventory, false);

                var action = game.GetAction();
                Console.WriteLine(action);

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");

                // in the first league: BREW <id> | WAIT; later: BREW <id> | CAST <id> [<times>] | LEARN <id> | REST | WAIT
                //Console.WriteLine(actionType + " " + recipeId);
            }
        }

        private static void ParseActions(Game game)
        {        
            var actionCount = int.Parse(Console.ReadLine()); // the number of spells and recipes in play
            
            var recipes = new List<Recipe>();
            var spells = new List<Spell>();
            

            for (var i = 0; i < actionCount; i++)
            {               
                var input = Console.ReadLine();
                var inputs = input.Split(' ');
                
                if(inputs[1] == "BREW")
                {
                    recipes.Add(new Recipe(int.Parse(inputs[0]),
                                           new int[] { Math.Abs(int.Parse(inputs[2])),
                                                       Math.Abs(int.Parse(inputs[3])),
                                                       Math.Abs(int.Parse(inputs[4])),
                                                       Math.Abs(int.Parse(inputs[5]))},
                                           int.Parse(inputs[6])));   
                }
                else if(inputs[1] == "CAST")
                {
                    spells.Add(new Spell(int.Parse(inputs[0]),
                                         new int[] { int.Parse(inputs[2]),
                                                     int.Parse(inputs[3]),
                                                     int.Parse(inputs[4]),
                                                     int.Parse(inputs[5])},
                                         int.Parse(inputs[9]) == 1));   
                }       
            }
            
            game.SetRecipes(recipes);
            game.SetSpells(spells);
        }
        
        private static Inventory GetInventoryItems()
        {
            var input = Console.ReadLine();
            var inputs = input.Split(' ');
            
            var inventory = new Inventory(new int[] {Math.Abs(int.Parse(inputs[0])),
                                          Math.Abs(int.Parse(inputs[1])),
                                          Math.Abs(int.Parse(inputs[2])),
                                          Math.Abs(int.Parse(inputs[3]))},
                                          int.Parse(inputs[4]));

            return inventory;
        }

        private static void DisplayInventory (Inventory inventory, bool mine)
        {
            Console.Error.WriteLine(mine ? "My inventory" : "Opponent inventory");
            Console.Error.WriteLine("-----------");
            
            Console.Error.WriteLine("blueIngredients: " + inventory.Ingredients[0]);   
            Console.Error.WriteLine("greenIngredients: " + inventory.Ingredients[1]);   
            Console.Error.WriteLine("orangeIngredients: " + inventory.Ingredients[2]);   
            Console.Error.WriteLine("yellowIngredients: " + inventory.Ingredients[3]); 
        }
    }
    
    // for (int i = 0; i < actionCount; i++)
// {
//     inputs = Console.ReadLine().Split(' ');
//     int actionId = int.Parse(inputs[0]); // the unique ID of this spell or recipe
//     string actionType = inputs[1]; // in the first league: BREW; later: CAST, OPPONENT_CAST, LEARN, BREW
//     int delta0 = int.Parse(inputs[2]); // tier-0 ingredient change
//     int delta1 = int.Parse(inputs[3]); // tier-1 ingredient change
//     int delta2 = int.Parse(inputs[4]); // tier-2 ingredient change
//     int delta3 = int.Parse(inputs[5]); // tier-3 ingredient change
//     int price = int.Parse(inputs[6]); // the price in rupees if this is a potion
//     int tomeIndex = int.Parse(inputs[7]); // in the first two leagues: always 0; later: the index in the tome if this is a tome spell, equal to the read-ahead tax; For brews, this is the value of the current urgency bonus
//     int taxCount = int.Parse(inputs[8]); // in the first two leagues: always 0; later: the amount of taxed tier-0 ingredients you gain from learning this spell; For brews, this is how many times you can still gain an urgency bonus
//     bool castable = inputs[9] != "0"; // in the first league: always 0; later: 1 if this is a castable player spell
//     bool repeatable = inputs[10] != "0"; // for the first two leagues: always 0; later: 1 if this is a repeatable player spell
// }
// for (int i = 0; i < 2; i++)
// {
//     inputs = Console.ReadLine().Split(' ');
//     int inv0 = int.Parse(inputs[0]); // tier-0 ingredients in inventory
//     int inv1 = int.Parse(inputs[1]);
//     int inv2 = int.Parse(inputs[2]);
//     int inv3 = int.Parse(inputs[3]);
//     int score = int.Parse(inputs[4]); // amount of rupees
// }
}