using System;
using System.Collections.Generic;
using System.Linq;

namespace RobotShow
{
    class Solution
    {
        static void Main(string[] args)
        {
            int ductLength = int.Parse(Console.ReadLine())+1;
            int numberOfBots = int.Parse(Console.ReadLine());
            string[] inputs = Console.ReadLine().Split(' ');

            List<char[]> ducts = new List<char[]>();

            List<int> botPositions = new List<int>();

            for (int i = 0; i < numberOfBots; i++)
            {
                //Console.Error.WriteLine($"inputs[i]: {inputs[i]}");
                botPositions.Add(int.Parse(inputs[i]));   
            }
            Console.Error.WriteLine($"duct length: {ductLength}");
            Console.Error.WriteLine($"botPositions.Count: {botPositions.Count}");

            int leftMost = botPositions.Min();

            int leftOne = ductLength - leftMost - 1;

            int rightMost = botPositions.Max();
            
            Console.Error.WriteLine($"Left one: {leftOne}");
            Console.Error.WriteLine($"Right one: {rightMost}");

            int longest = Math.Max(leftOne, rightMost);
            
            
            
            //var matrix = new List<char[]>();
            //var count = Math.Pow(2, botPositions.Count);
            //Console.Error.WriteLine($"Combos:{count}");
            
            // for (var i = 0; i < count; i++)
            // {
            //     var str = Convert.ToString(i, 2).PadLeft(botPositions.Count, '0');
            //     //Console.Error.WriteLine($"str: {str}");
            //     
            //     var charArray = str.Select(x => x == '1' ? '>' : '<').ToArray();
            //     
            //     //Convert char array to add in empty spaces
            //     var added = 0;
            //     var duct = new char[ductLength];
            //
            //     for (var j=0; j<ductLength; j++)
            //     {
            //         if(added < botPositions.Count && botPositions.Contains(j))
            //         {
            //             //Console.Error.WriteLine($"Adding {botPositions[added]} to duct position ");
            //             duct[botPositions[added]] = charArray[added];
            //             added++;
            //             
            //             //Console.Error.WriteLine($"Added:{added}");
            //         }
            //     }
            //     
            //     ducts.Add(duct);
            //
            //     //matrix.Add(charArray);
            // }
            
            //PrintDucts(ducts);
            
            //var longest = int.MinValue;
            
            

            // foreach (var duct in ducts)
            // {
            //     var time = GetTime(duct);
            //     
            //     if(time > longest)
            //     {
            //         longest = time;
            //     }
            // }

            // Write an answer using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");

            Console.WriteLine(longest);
        }
        
        private static void PrintDucts(List<char[]> ducts)
        {
            foreach (char[] duct in ducts)
            {
                PrintDuct(duct);
            }
        }
        
        private static void PrintDuct(char[] duct)
        {
            foreach(char cell in duct)
            {
                if(cell == '\0')
                {
                    Console.Error.Write('-');
                }
                else
                {
                    Console.Error.Write(cell);
                }
                   
            }
                
            Console.Error.WriteLine();
        }
        private static int GetTime(char[] duct)
        {
            int seconds = 0;

            bool botInDuct = true;
            
            Console.Error.WriteLine();

            while(botInDuct)
            {
                PrintDuct(duct);
                char[] nextDuct = new char[duct.Length];

                // Move bots
                for (int i = 0; i < duct.Length; i++)
                {
                    if(duct[i] != '\0')
                    {
                        if(duct[i] == '<' && i > 0)
                        {
                            if(nextDuct[i-1] == '\0')
                            {
                                nextDuct[i-1] = '<';
                            }
                            else
                            {
                                nextDuct[i-1] = 'X';
                            }
                        }
                        else if (duct[i] == '>' && i < duct.Length-1)
                        {
                            if(nextDuct[i+1] == '\0')
                            {
                                nextDuct[i+1] = '>';
                            }
                            else
                            {
                                nextDuct[i+1] = 'X';
                            }
                        }
                        else if(duct[i] == 'X')
                        {
                            if( i > 0)
                            {
                                if(nextDuct[i-1] == '\0')
                                {
                                    nextDuct[i-1] = '<';
                                }
                                else
                                {
                                    nextDuct[i-1] = 'X';
                                }
                            }

                            if(i < duct.Length-1)
                            {
                                if(nextDuct[i+1] == '\0')
                                {
                                    nextDuct[i+1] = '>';
                                }
                                else
                                {
                                    nextDuct[i+1] = 'X';
                                }
                            }
                        }
                    }
                }
                
                if(nextDuct[0] == '<')
                {
                    nextDuct[0] = '\0';
                }
                
                if(nextDuct[nextDuct.Length-1] == '>')
                {
                    nextDuct[nextDuct.Length-1] = '\0';
                }
                
                if(!nextDuct.Contains('>') && !nextDuct.Contains('<') && !nextDuct.Contains('X'))
                {
                    botInDuct = false;
                }
                
                
                
                seconds++;
                
                // Now swap them 
                duct = (char[])nextDuct.Clone();
            }  
            
            return seconds;
        }
    }
}