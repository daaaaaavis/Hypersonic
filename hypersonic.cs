using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Game 
{
    static void Main(string[] args)
    {
        string[] inputs;
        int turnCount = 0;
        Grid grid = new Grid();
        Player myPlayer = new Player();
               
        inputs = Console.ReadLine().Split(' ');
        int width = int.Parse(inputs[0]);
        int height = int.Parse(inputs[1]);
        int myId = int.Parse(inputs[2]);
        myPlayer.id = myId;

        // game loop
        while (true)
        {
            turnCount++;

            for (int i = 0; i < height; i++)
            {
                string row = Console.ReadLine();
                grid.refreshGrid(row, i, "present");
                grid.refreshGrid(row, i, "future");
                grid.fillReachable(row, i);
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

                if (entityType == 0 && owner == 0) // Mans spēlētājs
                {
                    myPlayer.position.x = x;
                    myPlayer.position.y = y;
                }

                if (entityType == 1)
                {
                    grid.ZeroOutFutureIndexes(param2, x, y);
                    // izreķināt futureGrid laukus
                    // param2 - explosion range for bombs
                    // ja ir bumba, vienkārši ar range sanuļļot apkārt
                }
            }

            grid.fillAdjacentBoxesArray(2);
            
            Coordinates coords = grid.getBestCoordinates(myPlayer);
            string nextCommand = "BOMB " + coords.y + " " + coords.x;
            TestMethod();
            Console.Error.WriteLine("PRESENT: ");
            grid.printGrid("present");
            Console.Error.WriteLine("FUTURE: ");
            grid.printGrid("future");            
            Console.Error.WriteLine("REACHABLE: ");
            grid.printGrid("reachable");
            grid.floodFill(0, 0, '1', '0');
            grid.printGrid("reachable");            
            Console.WriteLine(nextCommand);
        } 
    }

    public static void TestMethod()
    {
        Console.Error.WriteLine("TEST");
    }
}

class Grid
{
    private char[,] presentGrid = new char[11,13];
    private char[,] futureGrid = new char[11, 13];
    private int[,] adjacentBoxesArray = new int[11, 13]; // cik katrām coord. ir kastes, ar attiecīgo bombRange
    private char[,] reachableGrid = new char[11, 13];

    Dictionary <string, char[,]> grids = new Dictionary <string, char[,]>();

    public Grid()
    {
        grids.Add("present", presentGrid);
        grids.Add("future", futureGrid);
        grids.Add("reachable", reachableGrid);

        for (int i = 0; i < 11; i++)
        {
            for (int j = 0; j < 13; j++)
            {
                reachableGrid[i,j] = '0';
            }
        }
    }

    public void fillReachable(string row, int number)
    {
        for (int i = 0; i < 13; i++)
            {
                //presentGrid[number,i] = row[i];
                grids["reachable"][number,i] = (row[i].Equals('.')) ? '0' : 'X';
            }
    }

    public void floodFill(int x, int y, char fill, char old) // flood-fill algorithm
    {
        if ((x < 0) || (x >= 13)) return;
        if ((y < 0) || (y >= 11)) return;
        if (reachableGrid[y,x].Equals(old))
        {
            reachableGrid[y,x] = fill;
            floodFill(x+1, y, fill, old);
            floodFill(x, y+1, fill, old);
            floodFill(x-1, y, fill, old);
            floodFill(x, y-1, fill, old);
        }
    }

    public void refreshGrid(string row, int number, string gridName)
    {
        if (grids.ContainsKey(gridName))
        {
            for (int i = 0; i < 13; i++)
            {
                //presentGrid[number,i] = row[i];
                grids[gridName][number, i] = row[i];
            }
        }
        else
        {
            Console.Error.WriteLine("Error, couldn't refresh grid");
        }
    }
    
    public void printGrid(string gridName)
    {
        string row = String.Empty;

        if (grids.ContainsKey(gridName))
        {
            for (int i = 0; i < 11; i++)
            {
            for (int j = 0; j < 13; j++)
            {
                row = row + grids[gridName][i,j] + " ";
            }
            Console.Error.WriteLine(row);
            row = String.Empty;
            }
        }
        else
        {
            Console.Error.WriteLine("Couldn't print out the grid");
        }        
    }

    public bool isInDanger(Coordinates playerCoordinates)
    {
        return true;
    }

    public bool isSafeToPutBomb(Coordinates bombPosition)
    {
        // funkcija, kas aprēķina un uztaisa array(vai tamlīdzīgi)...
        // kurās pozīcijās ir safe likt bumbu
        // principā tas nozīmē arī - vai man pēc uzlikšanas būs kur palikt
        return true;
    }

    public void fillAdjacentBoxesArray(int bombRange) // cik katrai rūtiņai 'blakus' ir kastes
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
                if(!presentGrid[i-k,j].Equals('.')) adjacentCount++;
            }

            for (int k = 1; k < indexDown+1; k++)
            {
                if(!presentGrid[i+k,j].Equals('.')) adjacentCount++;
            }

            for (int k = 1; k < indexLeft+1; k++)
            {
                if(!presentGrid[i,j-k].Equals('.')) adjacentCount++;
            }

            for (int k = 1; k < indexRight+1; k++)
            {
                if(!presentGrid[i,j+k].Equals('.')) adjacentCount++;
            }
            //Console.Error.WriteLine("x = " + i + ", y = " + j + ", " + adjacentCount + " Up:" + indexUp + " Down:" + indexDown + " Left:" + indexLeft + " Right:" + indexRight);
            adjacentBoxesArray[i,j] = adjacentCount;
            adjacentCount = 0;
         }
        }
    }

    public void ZeroOutFutureIndexes(int bombRange, int y, int x) // iznuļļo nākotnes bumbas vietas
    {
            int indexUp, indexDown, indexLeft, indexRight; // par cik var iet uz attiecīgo pusi
            // x - rinda
            // y - kolonna
            indexUp = ( x < bombRange ) ? x : bombRange;
            indexLeft = ( y < bombRange ) ? y : bombRange;
            indexDown = ( x > (10-bombRange) ) ? (10-x) : bombRange;
            indexRight = ( y > (12-bombRange) ) ? (12-y) : bombRange;

            // CHECK HOW MANY BOXES ARE ADJACENT
            for (int k = 1; k < indexUp+1; k++)
            {
                futureGrid[x-k,y] = 'H';
            }

            for (int k = 1; k < indexDown+1; k++)
            {
                futureGrid[x+k,y] = 'H';
            }

            for (int k = 1; k < indexLeft+1; k++)
            {
                futureGrid[x,y-k] = 'H';
            }

            for (int k = 1; k < indexRight+1; k++)
            {
                futureGrid[x,y+k] = 'H';
            }
    }
 
    public Coordinates getBestCoordinates(Player player)
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
        closestDistance = player.distance(closest);

        Console.Error.WriteLine("Player: " + player.position.x + " " + player.position.y);
        Console.Error.WriteLine("Closest: " + closest.x + " " + closest.y);
        Console.Error.WriteLine("Closest distanceh: " + closestDistance);        

        for (int i = 0; i < list.Count(); i++)
        {
            tempDistance = player.distance(list[i]);
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

class Player
{
    public int id;
    public Coordinates position; // var taisīt kā private un ar getteriem/setteriem, bet ko no tā iegūs?
    public char powerUp;


    public Player (int x, int y)
    {
        position = new Coordinates(x, y);
    }

    public Player() 
    { 
        position = new Coordinates();
    }

    public int distance(Coordinates point) // finds what is the distance between two points
    {
        var dx = position.x - point.x;
        var dy = position.y - point.y;
        return Math.Abs(dx) + Math.Abs(dy);
    }

    public void returnNextMove()
    {
        // if ()
        // {
        //     // MOVE
        // }
        // else
        // {

        // }
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



