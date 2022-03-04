using System;

namespace Labyrinth
{
    internal class Player
    {
        static void Main(string[] args)
        {
            string[] inputs;
            inputs = Console.ReadLine().Split(' ');
            var rows = int.Parse(inputs[0]); // number of rows.
            var columns = int.Parse(inputs[1]); // number of columns.
            var alarmCountdown = int.Parse(inputs[2]); // number of rounds between the time the alarm countdown is activated and the time the alarm goes off.

            var game = new Game(columns, rows, alarmCountdown);

            // game loop
            while (true)
            {
                inputs = Console.ReadLine().Split(' ');
                var characterLocationRow = int.Parse(inputs[0]); // row where Rick is located.
                var characterColumn = int.Parse(inputs[1]); // column where Rick is located.

                game.UpdateCharacterLocation(characterColumn, characterLocationRow);

                var world = new Content[columns, rows];

                for (var y = 0; y < rows; y++)
                {
                    var row = Console.ReadLine().ToCharArray(); // C of the characters in '#.TC?' (i.e. one line of the ASCII maze).

                    for (var x = 0; x < columns; x++)
                    {
                        world[x, y] = ContentConverter.ToContent(row[x]);
                    }
                }

                game.UpdateWorld(world);

                var moveDirection = game.GetMove() switch
                {
                    MoveDirection.Right => "RIGHT",
                    MoveDirection.Left => "LEFT",
                    MoveDirection.Up => "UP",
                    MoveDirection.Down => "DOWN",
                    _ => "UP"
                };

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");

                Console.WriteLine(moveDirection); // Rick's next move (UP DOWN LEFT or RIGHT).
            }
        }
    }
}
