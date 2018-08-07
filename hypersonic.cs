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
        
        Grid grid = new Grid();         // spēles stāvoklis šobrīd
        Grid futureGrid = new Grid();   // spēles stāvoklis, kas ir 'garantēti' nākotnē
        Coordinates playerCoord = new Coordinates();
               
        inputs = Console.ReadLine().Split(' ');
        int width = int.Parse(inputs[0]);
        int height = int.Parse(inputs[1]);
        int myId = int.Parse(inputs[2]);

        // game loop
        while (true)
        {
            for (int i = 0; i < height; i++)
            {
                string row = Console.ReadLine();
                grid.refreshGrid(row, i);
                // futureGrid.refreshGrid(row, i);
            }
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

                if (entityType == 0 && owner == 0)
                {
                    playerCoord.x = x;
                    playerCoord.y = y;
                    // Mans spēlētājs
                }

                if (entityType == 1)
                {
                    // izreķināt futureGrid laukus
                    // param2 - explosion range for bombs
                    // ja ir bumba, vienkārši ar range sanuļļot apkārt
                }
            }
            grid.fillAdjacentBoxesArray(2);
            
            Coordinates coords = grid.getBestCoordinates(playerCoord);
            string nextCommand = "BOMB " + coords.y + " " + coords.x;
            
            grid.printGrid();
            Console.WriteLine(nextCommand);
        }
    }
}

class Grid
{
    private char[,] gameArray = new char[11,13];
    private int[,] adjacentBoxesArray = new int[11, 13];
    private Coordinates nextBox = new Coordinates();
    
    public void refreshGrid(string row, int number)
    {
        for (int i = 0; i < 13; i++)
        {
            gameArray[number, i] = row[i];
        }
    }

    public int distance(Coordinates player, Coordinates point) // finds what is the distance between two points
    {
        var dx = player.x - point.x;
        var dy = player.y - point.y;
        return Math.Abs(dx) + Math.Abs(dy);
    }
    
    public void printGrid()
    {
        string row = String.Empty;
        
        for (int i = 0; i < 11; i++)
        {
         for (int j = 0; j < 13; j++)
         {
             row = row + gameArray[i,j] + " ";
         }
         Console.Error.WriteLine(row);
         row = String.Empty;
        }
    }

    public void fillAdjacentBoxesArray(int bombRange)
    {
        int indexUp, indexDown, indexLeft, indexRight; // par cik var iet uz attiecīgo pusi
        int adjacentCount = 0;

        for (int i = 0; i < 11; i++) // rindas
        {
         for (int j = 0; j < 13; j++) // kolonnas
         {
            indexUp = ( i < bombRange ) ? i : bombRange;
            indexLeft = ( j < bombRange ) ? j : bombRange;
            indexDown = ( i > (10-bombRange) ) ? (10-i) : bombRange;
            indexRight = ( j > (12-bombRange) ) ? (12-j) : bombRange;

            // CHECK HOW MANY BOXES ARE ADJACENT
            for (int k = 1; k < indexUp+1; k++)
            {
                if(!gameArray[i-k,j].Equals('.')) adjacentCount++;
            }

            for (int k = 1; k < indexDown+1; k++)
            {
                if(!gameArray[i+k,j].Equals('.')) adjacentCount++;
            }

            for (int k = 1; k < indexLeft+1; k++)
            {
                if(!gameArray[i,j-k].Equals('.')) adjacentCount++;
            }

            for (int k = 1; k < indexRight+1; k++)
            {
                if(!gameArray[i,j+k].Equals('.')) adjacentCount++;
            }
            //Console.Error.WriteLine("x = " + i + ", y = " + j + ", " + adjacentCount + " Up:" + indexUp + " Down:" + indexDown + " Left:" + indexLeft + " Right:" + indexRight);
            adjacentBoxesArray[i,j] = adjacentCount;
            adjacentCount = 0;
         }
        }
    }
    // END FILL ADJACENT BOXES



    public Coordinates getBestCoordinates(Coordinates player)
    {
        int maxCount = 0;
        List<Coordinates> list = new List<Coordinates>();
        for (int i = 0; i < 11; i++) // rindas
        {
         for (int j = 0; j < 13; j++) // kolonnas
         {
             if (adjacentBoxesArray[i,j] == maxCount) 
             {
                 Coordinates temp = new Coordinates(i,j);
                 list.Add(temp);
             }
             if (adjacentBoxesArray[i,j] > maxCount)
             {
                 maxCount = adjacentBoxesArray[i,j];
                 list.Clear();
                 Coordinates temp = new Coordinates(i,j);
                 temp.surroundingBoxes = maxCount;
                 list.Add(temp);
             } 
         }
        }
        // IR LISTS AR VISĀM "LABĀKAJĀM" KOORD.
        // jāskatās, kurš punkts vistuvāk
        Coordinates closest = new Coordinates();
        int closestDistance = 0;
        int tempDistance = 0;

        closest.x = list[0].x;
        closest.y = list[0].y;
        closestDistance = distance(player, closest);

        Console.Error.WriteLine("Error: " + closest.x + " " + closest.y + " " + closestDistance);
        Console.Error.WriteLine("Player: " + player.x + " " + player.y);
        Console.Error.WriteLine("Closest: " + closest.x + " " + closest.y);

        for (int i = 0; i < list.Count(); i++)
        {
            tempDistance = distance(list[i], player);
            if (tempDistance < closestDistance)
            {
                closest.x = list[i].x;
                closest.y = list[i].y;
                closestDistance = tempDistance;
                //Console.Error.WriteLine("Error2: " + closest.x + " " + closest.y + " " + closestDistance);
            }
        }

        //Console.Error.WriteLine("ErrorFinal: " + closest.x + " " + closest.y + " " + closestDistance);
        return closest;
    }
}


class Coordinates
{
    public int x;
    public int y;
    public int surroundingBoxes;

    public Coordinates(){ }

    public Coordinates(int X, int Y)
    {
        x = X;
        y = Y;
    }
}



