using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    static void Main(string[] args)
    {
        string[] inputs;
        Grid grid = new Grid();
        inputs = Console.ReadLine().Split(' ');
        int width = int.Parse(inputs[0]);
        int height = int.Parse(inputs[1]);
        int myId = int.Parse(inputs[2]);

        // game loop
        while (true)
        {
            for (int i = 0; i < height; i++) // height lines: a string row representing each row of the grid. Each character can be: "." an empty cell, "0" a box.
            {
                string row = Console.ReadLine();
                grid.refreshGrid(row, i);
            }
            grid.printGrid();
            int entities = int.Parse(Console.ReadLine());
            for (int i = 0; i < entities; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int entityType = int.Parse(inputs[0]);
                int owner = int.Parse(inputs[1]);
                int x = int.Parse(inputs[2]);
                int y = int.Parse(inputs[3]);
                int param1 = int.Parse(inputs[4]);
                int param2 = int.Parse(inputs[5]);
            }

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");

            Console.WriteLine("BOMB 6 5");
        }
    }
    

}

class Grid
{
    private char[,] gameGrid = new char[11,13];
    
    public void refreshGrid(string row, int number)
    {
        for (int i = 0; i < 13; i++)
        {
            gameGrid[number, i] = row[i];
        }
    }
    
    public void printGrid()
    {
        string row = String.Empty;
        
        for (int i = 0; i < 11; i++)
        {
         for (int j = 0; j < 13; j++)
         {
             row = row + gameGrid[i,j] + " ";
         }
         Console.Error.WriteLine(row);
         row = String.Empty;
        }
    }
}



