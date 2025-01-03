﻿using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Solution
{
    static void Main(string[] args)
    {
        Queue<string> player1Cards = new Queue<string>();
        Queue<string> player2Cards = new Queue<string>();

        int n = int.Parse(Console.ReadLine()); // the number of cards for player 1
        
        for (int i = 0; i < n; i++)
        {
            player1Cards.Enqueue(Console.ReadLine()); // the n cards of player 1
        }

        int m = int.Parse(Console.ReadLine()); // the number of cards for player 2
        
        for (int i = 0; i < m; i++)
        {
            player2Cards.Enqueue(Console.ReadLine()); // the m cards of player 2
        }

        Queue<string> player1WaitingCards = new Queue<string>();
        Queue<string> player2WaitingCards = new Queue<string>();

        bool gameOver = false;

        int roundCount = 1;
        
        PrintCards(player1Cards, "Player 1 cards: ");
        PrintCards(player2Cards, "Player 2 cards: ");

        while(!gameOver)
        {
            bool roundOver = false;
            
            while(!roundOver)
            {
                Console.Error.WriteLine($"Round:{roundCount}");

                // Have fight
                string player1Card = player1Cards.Dequeue();
                string player2Card = player2Cards.Dequeue();
                
                Console.Error.WriteLine($"player1Card:{player1Card}");
                Console.Error.WriteLine($"player2Card:{player2Card}");

                // if(player1Cards.Count == 0 || player1Cards.Count == 0)
                // {
                //     Console.WriteLine("Tie");
                //     break;
                // }

                int player1CardValue = GetValue(player1Card);
                int player2CardValue = GetValue(player2Card);
                
                player1WaitingCards.Enqueue(player1Card);
                player2WaitingCards.Enqueue(player2Card);

                if(player1CardValue > player2CardValue)
                {
                    MoveCards(player1WaitingCards, player1Cards, player1WaitingCards.Count);
                    MoveCards(player2WaitingCards, player1Cards, player2WaitingCards.Count);
                    roundOver = true;
                }
                else if(player2CardValue > player1CardValue)
                {
                    MoveCards(player1WaitingCards, player2Cards, player1WaitingCards.Count);
                    MoveCards(player2WaitingCards, player2Cards, player2WaitingCards.Count);
                    roundOver = true;
                }
                else
                {
                    if(player1Cards.Count <= 3 || player2Cards.Count <= 3)
                    {
                        Console.WriteLine("PAT");
                        gameOver = true;
                        break;
                    }

                    MoveCards(player1Cards, player1WaitingCards, 3);
                    MoveCards(player2Cards, player2WaitingCards, 3);
                }

                // Check for end
                if(player1Cards.Count == 0)
                {
                    Console.WriteLine($"2 {roundCount}");
                    gameOver = true;
                }
                else if(player2Cards.Count == 0)
                {
                    Console.WriteLine($"1 {roundCount}");
                    gameOver = true;
                }
            }
            
            roundCount++;
        }


        // Write an answer using Console.WriteLine()
        // To debug: Console.Error.WriteLine("Debug messages...");
    }
    
    private static void PrintCards(Queue<string> cards, string text)
    {
        Queue<string>.Enumerator enumerator = cards.GetEnumerator();
        
        while(enumerator.MoveNext())
        {
            text += enumerator.Current + " ";
        }
        
        Console.Error.WriteLine(text);
    }

    private static void MoveCards(Queue<string> fromQueue, Queue<string> toQueue, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            //toQueue.Enqueue(fromQueue.Dequeue());

            string card = fromQueue.Dequeue();
            toQueue.Enqueue(card);
        }
    }

    private static int GetValue(string card)
    {
        card = card.Remove(card.Length-1);

        int cardValue = card switch
        {
            "A" => 14,
            "K" => 13,
            "Q" => 12,
            "J" => 11,
            _   => int.Parse(card)
        };
        
        return cardValue;
    }
}