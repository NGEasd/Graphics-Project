using System;
using System.Collections.Generic;
using System;

namespace Labirintus_projekt
{
    class LabirintMap
    {
        private int[,] map;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public LabirintMap()
        {
            GenerateMaze();
        }

        private void GenerateMaze()
        {

            string[] hardcodedMap = new string[] {
                "113311111111111111111",
                "110011111111111111111",
                "110010001000100010011",
                "110011111011111010011",
                "110000000000000000011",
                "110000000000000000011",
                "111111011111011111111",
                "110000000000000000011",
                "110000000000000000011",
                "110011111011111010011",
                "110010001000100010011",
                "111111111111111110211",
                "111111111111111111111"
            };

            Height = hardcodedMap.Length;
            Width = hardcodedMap[0].Length;

            map = new int[Height, Width];

            for (int x = 0; x < Height; x++)
            {
                for (int y = 0; y < Width; y++)
                {
                    int value = int.Parse(hardcodedMap[x][y].ToString());
                    map[x, y] = value;
                }
            }
        }

        public int Get(int x, int y)
        { 
            return map[x, y];
        }

        public int[] GetStartingPosition()
        {
            for (int x = 0; x < Height; x++)
            {
                for (int y = 0; y < Width; y++)
                {
                    if (map[x, y] == 2)
                    {
                        return new int[] { x, y};
                    }
                }
            }

            return new int[] { -1, -1 };
        }

        public bool IsWall(int x, int y)
        {
            return Get(x, y) == 1;
        }

        public void PrintToConsole()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Console.Write(map[y, x] == 1 ? "#" : " ");
                }
                Console.WriteLine();
            }
        }
    }
}
