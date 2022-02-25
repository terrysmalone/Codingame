using System;

namespace WinamaxGolf
{
    class Solution
    {
        static void Main(string[] args)
        {
            string[] inputs = Console.ReadLine().Split(' ');
            int width = int.Parse(inputs[0]);
            int height = int.Parse(inputs[1]);

            var course = new char[width, height];

            for (int y = 0; y < height; y++)
            {
                string row = Console.ReadLine();

                var cols = row.ToCharArray();

                for (int x = 0; x < width; x++)
                {
                    course[x,y] = cols[x];
                }

                Console.Error.WriteLine(row);
            }

            DisplayCourse(course);

            var moves = CalculateMoves(course);

            // while all balls are not in holes

            // calculate all possible moves

            // recursively try all moves

            // Verify if move is possible
        }

        private static string CalculateMoves(char[,] course)
        {


            return string.Empty;
        }

        private static void DisplayCourse(char[,] course)
        {
            var display = string.Empty;

            for (int y = 0; y < course.GetLength(1); y++)
            {
                for (int x = 0; x < course.GetLength(0); x++)
                {
                    display += course[x,y];
                }

                display += "\n";
            }

            Console.Error.WriteLine(display);
        }
    }
}
