using System.Drawing;

namespace Labyrinth
{
    internal static class DebugViewer
    {
        internal static void PrintWorld(Content[,] worldGrid, Point characterLocation)
        {
            var characterView = ContentConverter.ToCharacterGrid(worldGrid);
            for (var y = 0; y < worldGrid.GetLength(1); y++)
            {
                for (var x = 0; x < worldGrid.GetLength(0); x++)
                {
                    if (characterLocation.X == x && characterLocation.Y == y)
                    {
                        Console.Error.Write('+');
                    }
                    else
                    {
                        Console.Error.Write(characterView[x, y]);
                    }
                }

                Console.Error.WriteLine();
            }
        }
    }

}
