using System;
using System.Collections.Generic;
using System.Linq;

namespace RobotShow
{
    class Solution
    {
        static void Main(string[] args)
        {
            var L = int.Parse(Console.ReadLine());
            var N = int.Parse(Console.ReadLine());
            var inputs = Console.ReadLine().Split(' ');

            var duct = new char[L];
            
            List<int> botPositions = new List<int>();

            for (var i = 0; i < N; i++)
            {
                botPositions.Add(int.Parse(inputs[i]));   
            }
            
            var matrix = new List<char[]>();
            var count = Math.Pow(2, botPositions.Count);
            
            for (var i = 0; i < count; i++)
            {
                var str = Convert.ToString(i, 2).PadLeft(botPositions.Count, '0');
                var charArray = str.Select(x => x == '1' ? '>' : '<').ToArray();
                
                //Convert char array to add in empty spaces
                


                matrix.Add(charArray);
            }
            
            
            for(var i=0; i<botPositions.Count; i++)
            {
                for(var j=0; j<botPositions.Count; j++)
                {
                    if (i != j)
                    {
                        
                    }
                }
            }
            
            

            duct[2] = '>';
            duct[6] = '<'; 
            
            var time = GetTime(duct);

            // Write an answer using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");

            Console.WriteLine(time);
        }
        private static object GetTime(char[] duct)
        {
            var seconds = 0;
            
            var botInDuct = true;

            while(botInDuct)
            {
                var nextDuct = new char[duct.Length];

                // Move bots
                for (var i = 0; i < duct.Length; i++)
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