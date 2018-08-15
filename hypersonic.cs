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
        bool check = false;
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
            check = false;

            for (int i = 0; i < height; i++)
            {
                string row = Console.ReadLine();
                grid.refreshGrid(row, i, "present");
                grid.refreshGrid(row, i, "future");
                grid.refreshGrid(row, i, "reachable");
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
                    // atrada bumbu
                    grid.ZeroOutFutureIndexes(param2, x, y);
                    if ( owner == 0 )
                    {
                        grid.floodFill(myPlayer.position.x, myPlayer.position.y);
                        Coordinates coordsX = grid.getBestEscape(myPlayer);
                        Console.WriteLine("MOVE " + coordsX.x + " " + coordsX.y);
                        check = true;
                    }
                    // param2 - explosion range for bombs
                }
            }

            if (check) continue;

            grid.fillAdjacentBoxesArray(2);
            
            grid.floodFill(myPlayer.position.x, myPlayer.position.y);
            Coordinates coords = grid.getBestCoordinates(myPlayer); // dabū labākās un "safe" koord. 

            string nextCommand;
            if ( coords.x == myPlayer.position.x && coords.y == myPlayer.position.y )
            {
                nextCommand = "BOMB " + coords.x + " " + coords.y;
            }
            else
            {
                Console.Error.WriteLine(" player x = " + myPlayer.position.x + " player y = " + myPlayer.position.y + " coords x : " + coords.x + " coords y : " + coords.y);
                nextCommand = "MOVE " + coords.x + " " + coords.y;
            }

            // kamēr bumba nav sprāgusi, izmantot simulateExplosion lai uzzinātu - kur nepieciešams atrasties
            // kad bumba uzsprāgusi, sākt procesu no jauna

            // Console.Error.WriteLine("Best coord X : " + coords.x + "Best coord Y : " + coords.y);
            // string nextCommand = "BOMB " + coords.y + " " + coords.x;

            // Console.Error.WriteLine("PRESENT: ");
            // grid.printGrid("present");
            // Console.Error.WriteLine("FUTURE: ");
            // grid.printGrid("future");            
            // Console.Error.WriteLine("REACHABLE: ");
            // grid.printGrid("reachable");
  
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
    private char[,] presentGrid = new char[11, 13];
    private char[,] futureGrid = new char[11, 13];
    private int[,] adjacentBoxesArray = new int[11, 13]; // cik katrām coord. ir kastes, ar attiecīgo bombRange
    private char[,] reachableGrid = new char[11, 13];
    private char[,] simulationGrid = new char[11, 13];
    List<Coordinates> bombList = new List<Coordinates>();

    Dictionary <string, char[,]> grids = new Dictionary <string, char[,]>();

    public Grid()
    {
        grids.Add("present", presentGrid);
        grids.Add("future", futureGrid);
        grids.Add("reachable", reachableGrid);
        grids.Add("simulation", simulationGrid); 

        for (int i = 0; i < 11; i++)
        {
            for (int j = 0; j < 13; j++)
            {
                reachableGrid[i,j] = '.';
            }
        }
    }

    public void floodFill(int x, int y)
    {
        if ((x < 0) || (x >= 13)) return;
        if ((y < 0) || (y >= 11)) return;
        if (reachableGrid[y,x].Equals('.'))
        {
            reachableGrid[y,x] = 'O';        

            floodFill(x+1, y);
            floodFill(x, y+1);
            floodFill(x-1, y);
            floodFill(x, y-1);
        }
    }

    public void refreshGrid(string row, int number, string gridName)
    {
        if (grids.ContainsKey(gridName))
        {
            for (int i = 0; i < 13; i++)
            {
                grids[gridName][number, i] = row[i];
                // grids["reachable"][number,i] = (row[i].Equals('.')) ? '0' : 'X'; // fils reachable-array
            }
        }
        else
        {
            Console.Error.WriteLine("Error, couldn't refresh grid");
        }
    }

    public void copyArray(string from, string to)
    {
        for (int i = 0; i < 11; i++)
        {
            for (int j = 0; j < 13; j++)
            {
                grids[to][i,j] = grids[from][i,j];
            }
        }
    }
    
    public void printGrid(string gridName)
    {
        Console.Error.WriteLine (gridName + " : ");
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

    public void simulateExplosion(int x, int y, int bombRange)
    {
        // fills the grid with expected bomb radius 'H'
        int indexUp, indexDown, indexLeft, indexRight;

        indexUp = ( x < bombRange ) ? x : bombRange;
        indexLeft = ( y < bombRange ) ? y : bombRange;
        indexDown = ( x > (10-bombRange) ) ? (10-x) : bombRange;
        indexRight = ( y > (12-bombRange) ) ? (12-y) : bombRange;

        for (int k = 1; k < indexUp+1; k++)
        {
            if (presentGrid[x-k, y].Equals('X')) break; // uz attiecīgo pusi ir siena
            simulationGrid[x-k,y] = 'H';
        }

        for (int k = 1; k < indexDown+1; k++)
        {
            if (presentGrid[x+k, y].Equals('X')) break; // uz attiecīgo pusi ir siena
            simulationGrid[x+k,y] = 'H' ;
        }

        for (int k = 1; k < indexLeft+1; k++)
        {
            if (presentGrid[x, y-k].Equals('X')) break; // uz attiecīgo pusi ir siena
            simulationGrid[x,y-k] = 'H' ;
        }

        for (int k = 1; k < indexRight+1; k++)
        {
            if (presentGrid[x,y+k].Equals('X')) break;
            simulationGrid[x,y+k] = 'H' ;
        }

        simulationGrid[x,y] = 'H';
    }

    public bool isInDanger(Coordinates playerCoordinates)
    {
        if (futureGrid[playerCoordinates.x, playerCoordinates.y].Equals('H')) return true;
        return false;
    }

    public bool isSafeToPutBomb(int x, int y, int range)
    {
        copyArray("reachable", "simulation");
        simulateExplosion(x, y, range);

        for (int i = 0; i < 11; i++) // rindas
        {
         for (int j = 0; j < 13; j++) // kolonnas
         {
            if (simulationGrid[i,j].Equals('O')) 
            {
                resetArray("simulation");
                return true; // ir kur aiziet
            }
         }
        }
        Console.Error.WriteLine("Simulations : ");
        printGrid("simulation");
        resetArray("simulation");
        return false;
    }

    public void resetArray(string arrayName)
    {
        for (int i = 0; i < 11; i++) // rindas
        {
         for (int j = 0; j < 13; j++) // kolonnas
         {
            grids[arrayName][i,j] = presentGrid[i,j];
         }
        }
    }

    public void fillAdjacentBoxesArray(int bombRange) // cik katrai rūtiņai 'blakus' ir kastes
    {
        int indexUp, indexDown, indexLeft, indexRight; // par cik var iet uz attiecīgo pusi
        int adjacentCount = 0;
        int element;

        printGrid("present");
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
                element = presentGrid[i-k,j];
                if(element.Equals('0') || element.Equals('1') || element.Equals('2')) adjacentCount++;
            }

            for (int k = 1; k < indexDown+1; k++)
            {
                element = presentGrid[i+k,j];
                if(element.Equals('0') || element.Equals('1') || element.Equals('2')) adjacentCount++;
            }

            for (int k = 1; k < indexLeft+1; k++)
            {
                element = presentGrid[i,j-k];
                if(element.Equals('0') || element.Equals('1') || element.Equals('2')) adjacentCount++;
            }

            for (int k = 1; k < indexRight+1; k++)
            {
                element = presentGrid[i,j+k];
                if(element.Equals('0') || element.Equals('1') || element.Equals('2')) adjacentCount++;
            }
            //Console.Error.WriteLine("x = " + i + ", y = " + j + ", " + adjacentCount + " Up:" + indexUp + " Down:" + indexDown + " Left:" + indexLeft + " Right:" + indexRight);
            adjacentBoxesArray[i,j] = adjacentCount;
            adjacentCount = 0;
         }
        }
    }

    public void ZeroOutFutureIndexes(int bombRange, int x, int y) // iznuļļo nākotnes bumbas vietas
    {
            int indexUp, indexDown, indexLeft, indexRight; // par cik var iet uz attiecīgo pusi
            // x - kolonna
            // y - rinda
            indexUp = ( y < bombRange ) ? y : bombRange;
            indexLeft = ( x < bombRange ) ? x : bombRange;
            indexDown = ( y > (10-bombRange) ) ? (10-y) : bombRange;
            indexRight = ( x > (12-bombRange) ) ? (12-x) : bombRange;

            Console.Error.WriteLine("Up:" + indexUp + " Left:" + indexLeft + " Down:" + indexDown + " Right:" + indexRight);
            Console.Error.WriteLine("BombRange:" + bombRange + " x:" + x + " y:" + y);
            // printGrid("present");
            // CHECK HOW MANY BOXES ARE ADJACENT
            for (int k = 1; k < indexUp+1; k++)
            {
                if (presentGrid[y-k, x].Equals('X')) break; // uz attiecīgo pusi ir siena
                futureGrid[y-k,x] = 'H';
            }
 
            for (int k = 1; k < indexDown+1; k++)
            {
                if (presentGrid[y+k, x].Equals('X')) break; // uz attiecīgo pusi ir siena
                futureGrid[y+k,x] = 'H';
            }

            for (int k = 1; k < indexLeft+1; k++)
            {
                if (presentGrid[y,x-k].Equals('X')) break; // uz attiecīgo pusi ir siena
                futureGrid[y,x-k] = 'H';
            }

            for (int k = 1; k < indexRight+1; k++)
            {
                if (presentGrid[y,x+k].Equals('X')) break; // uz attiecīgo pusi ir siena
                futureGrid[y,x+k] = 'H';
            }

            futureGrid[y,x] = 'H';
    }
 
    public Coordinates getBestCoordinates(Player player)
    {
        int maxCount = 0;
        List<Coordinates> list = new List<Coordinates>();
        printGrid("reachable");

        for (int i = 0; i < 11; i++) // rindas
        {
         for (int j = 0; j < 13; j++) // kolonnas
         {
             if (reachableGrid[i,j].Equals('O'))
             {
                if (adjacentBoxesArray[i,j] == maxCount) 
                {
                    if (!isSafeToPutBomb(i,j, 2)) continue; // ja nav safe, ignorē
                    Coordinates temp = new Coordinates(j,i);
                    list.Add(temp);
                }
                if (adjacentBoxesArray[i,j] > maxCount)
                {             
                    if (!isSafeToPutBomb(i,j, 2)) continue;
                    maxCount = adjacentBoxesArray[i,j];
                    list.Clear();
                    Coordinates temp = new Coordinates(j,i);
                    temp.surroundingBoxes = maxCount;
                    list.Add(temp);
                } 
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
        Console.Error.WriteLine("Closest distance: " + closestDistance);        

        for (int i = 0; i < list.Count(); i++)
        {
            tempDistance = player.distance(list[i]);
            if (tempDistance < closestDistance && reachableGrid[list[i].x , list[i].y].Equals('1'))
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

     public Coordinates getBestEscape(Player player)
    {
        int maxCount = 0;
        List<Coordinates> list = new List<Coordinates>();

        printGrid("reachable");
        printGrid("future");

        for (int i = 0; i < 11; i++) // rindas
        {
         for (int j = 0; j < 13; j++) // kolonnas
         {
             if (reachableGrid[i,j].Equals('O') && !futureGrid[i,j].Equals('H'))
             {
                    // ja var aiziet, un ja nav bumba 
                    Coordinates temp = new Coordinates(j,i);
                    list.Add(temp);
             }
         }
        }

        Coordinates closest = new Coordinates();
        int closestDistance = 0;
        int tempDistance = 0;

        closest.x = list[0].x;
        closest.y = list[0].y;
        closestDistance = player.distance(closest);      

        for (int i = 0; i < list.Count(); i++)
        {
            tempDistance = player.distance(list[i]);
            if (tempDistance < closestDistance && reachableGrid[list[i].y,list[i].x].Equals('O'))
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



