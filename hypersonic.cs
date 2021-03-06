﻿using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

static class Global
{
    public const int HEIGHT = 11;
    public const int WIDTH = 13;

    public const char EXPLOSION = 'H';
    public const char REACHABLE = 'R';
    public const char WALL = 'X';
    public const char BOX0 = '0';
    public const char BOX1 = '1';
    public const char BOX2 = '2';
    public const char EMPTY_CELL = '.';
    public const char POWERUP_RANGE = '5';
    public const char POWERUP_EXTRA = '6';
    public const char BOMB = 'B';
}

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

                switch ( entityType )
                {
                    case 0 : // player
                        if (owner == myPlayer.id)
                        {
                            myPlayer.position.x = x;
                            myPlayer.position.y = y;
                        }
                        break;

                    case 1 : // bomb
                        grid.CalculateFutureExplosions(param2, x, y);
                        grid.changeGridValue(x, y, Global.BOMB);
                        if ( owner == myPlayer.id )
                        {
                            // ja bumba ir mana, tad atrod labāko vietu kur nostāties gaidot sprādzienu
                            // problēma - var likt tikai vienu bumbu. ja ir power-up, tad škrobīgi
                            grid.floodFill(myPlayer.position.x, myPlayer.position.y);
                            Coordinates coordsX = grid.getBestEscape(myPlayer);
                            Console.WriteLine("MOVE " + coordsX.x + " " + coordsX.y);
                            check = true;
                        }
                        else
                        {   

                            // ja citu bumba - uzlikt kur nevar iet un no kā izvairīties
                        }
                        break;

                    case 2 : // items
                        if (param1 == 1)
                        {
                            grid.changeGridValue(x, y, Global.POWERUP_RANGE); 
                        } 
                        else if (param1 == 2)
                        {
                            grid.changeGridValue(x, y, Global.POWERUP_EXTRA);
                        }
                        break;
                }
            }

            if (check) continue;

            grid.fillAdjacentBoxesArray(2);
            grid.printAdjacent();
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
    private char[,] presentGrid = new char[Global.HEIGHT, Global.WIDTH];
    private char[,] futureGrid = new char[Global.HEIGHT, Global.WIDTH];
    private int[,] adjacentBoxesArray = new int[Global.HEIGHT, Global.WIDTH]; // cik katrām coord. ir kastes, ar attiecīgo bombRange
    private char[,] reachableGrid = new char[Global.HEIGHT, Global.WIDTH];
    private char[,] simulationGrid = new char[Global.HEIGHT, Global.WIDTH];
    List<Coordinates> bombList = new List<Coordinates>();

    Dictionary <string, char[,]> grids = new Dictionary <string, char[,]>();

    public Grid()
    {
        grids.Add("present", presentGrid);
        grids.Add("future", futureGrid);
        grids.Add("reachable", reachableGrid);
        grids.Add("simulation", simulationGrid); 

        for (int i = 0; i < Global.HEIGHT; i++)
        {
            for (int j = 0; j < Global.WIDTH; j++)
            {
                reachableGrid[i,j] = Global.EMPTY_CELL;
            }
        }
    }

    public void floodFill(int x, int y)
    {
        if ((x < 0) || (x >= Global.WIDTH)) return;
        if ((y < 0) || (y >= Global.HEIGHT)) return;
        if (reachableGrid[y,x].Equals(Global.EMPTY_CELL) || reachableGrid[y,x].Equals(Global.BOMB))
        {
            reachableGrid[y,x] = Global.REACHABLE;        

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
            for (int i = 0; i < Global.WIDTH; i++)
            {
                grids[gridName][number, i] = row[i];
                // grids["reachable"][number,i] = (row[i].Equals(Global.EMPTY_CELL)) ? Global.BOX2 : Global.WALL; // fils reachable-array
            }
        }
        else
        {
            Console.Error.WriteLine("Error, couldn't refresh grid");
        }
    }

    public void copyArray(string from, string to)
    {
        for (int i = 0; i < Global.HEIGHT; i++)
        {
            for (int j = 0; j < Global.WIDTH; j++)
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
            for (int i = 0; i < Global.HEIGHT; i++)
            {
                for (int j = 0; j < Global.WIDTH; j++)
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

    public void printAdjacent()
    {
        Console.Error.WriteLine("ADJACENT : ");
        string row = String.Empty;
        for (int i = 0; i < Global.HEIGHT; i++)
        {
            for (int j = 0; j < Global.WIDTH; j++)
            {
                row = row + adjacentBoxesArray[i,j] + " ";
            }
            Console.Error.WriteLine(row);
            row = String.Empty;
        }
    }

    public void simulateExplosion(int x, int y, int bombRange)
    {
        // fills the grid with expected bomb radius Global.EXPLOSION
        int indexUp, indexDown, indexLeft, indexRight;

        indexUp = ( x < bombRange ) ? x : bombRange;
        indexLeft = ( y < bombRange ) ? y : bombRange;
        indexDown = ( x > (10-bombRange) ) ? (10-x) : bombRange;
        indexRight = ( y > (12-bombRange) ) ? (12-y) : bombRange;

        for (int k = 1; k < indexUp+1; k++)
        {
            if (presentGrid[x-k, y].Equals(Global.WALL)) break; // uz attiecīgo pusi ir siena
            simulationGrid[x-k,y] = Global.EXPLOSION;
        }

        for (int k = 1; k < indexDown+1; k++)
        {
            if (presentGrid[x+k, y].Equals(Global.WALL)) break; // uz attiecīgo pusi ir siena
            simulationGrid[x+k,y] = Global.EXPLOSION ;
        }

        for (int k = 1; k < indexLeft+1; k++)
        {
            if (presentGrid[x, y-k].Equals(Global.WALL)) break; // uz attiecīgo pusi ir siena
            simulationGrid[x,y-k] = Global.EXPLOSION ;
        }

        for (int k = 1; k < indexRight+1; k++)
        {
            if (presentGrid[x,y+k].Equals(Global.WALL)) break;
            simulationGrid[x,y+k] = Global.EXPLOSION ;
        }

        simulationGrid[x,y] = Global.EXPLOSION;
    }

    public bool isInDanger(Coordinates playerCoordinates)
    {
        if (futureGrid[playerCoordinates.x, playerCoordinates.y].Equals(Global.EXPLOSION)) return true;
        return false;
    }

    public bool isSafeToPutBomb(int x, int y, int range)
    {
        copyArray("reachable", "simulation");
        simulateExplosion(x, y, range);

        for (int i = 0; i < Global.HEIGHT; i++) // rindas
        {
         for (int j = 0; j < Global.WIDTH; j++) // kolonnas
         {
            if (simulationGrid[i,j].Equals(Global.REACHABLE)) 
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
        for (int i = 0; i < Global.HEIGHT; i++) // rindas
        {
         for (int j = 0; j < Global.WIDTH; j++) // kolonnas
         {
            grids[arrayName][i,j] = presentGrid[i,j];
         }
        }
    }

    public void changeGridValue(int x, int y, char value)
    {
        presentGrid[y,x] = value;
    }

    public void fillAdjacentBoxesArray(int bombRange) // cik katrai rūtiņai 'blakus' ir kastes
    {
        int indexUp, indexDown, indexLeft, indexRight; // par cik var iet uz attiecīgo pusi
        int adjacentCount = 0;
        int element;

        printGrid("present");
        for (int i = 0; i < Global.HEIGHT; i++) // rindas
        {
         for (int j = 0; j < Global.WIDTH; j++) // kolonnas
         {
            indexUp = ( i < bombRange ) ? i : bombRange;
            indexLeft = ( j < bombRange ) ? j : bombRange;
            indexDown = ( i > (10-bombRange) ) ? (10-i) : bombRange;
            indexRight = ( j > (12-bombRange) ) ? (12-j) : bombRange;

            // CHECK HOW MANY BOXES ARE ADJACENT
            for (int k = 1; k < indexUp+1; k++)
            {
                element = presentGrid[i-k,j];
                if(element.Equals(Global.WALL) || element.Equals(Global.POWERUP_EXTRA) || element.Equals(Global.POWERUP_RANGE)) break;
                if(element.Equals(Global.BOX0) || element.Equals(Global.BOX1) || element.Equals(Global.BOX2))
                {
                    adjacentCount++;
                    break;
                } 
            }

            for (int k = 1; k < indexDown+1; k++)
            {
                element = presentGrid[i+k,j];
                if(element.Equals(Global.WALL) || element.Equals(Global.POWERUP_EXTRA) || element.Equals(Global.POWERUP_RANGE)) break;
                if(element.Equals(Global.BOX0) || element.Equals(Global.BOX1) || element.Equals(Global.BOX2))
                {
                    adjacentCount++;
                    break;
                } 
            }

            for (int k = 1; k < indexLeft+1; k++)
            {
                element = presentGrid[i,j-k];
                if(element.Equals(Global.WALL) || element.Equals(Global.POWERUP_EXTRA) || element.Equals(Global.POWERUP_RANGE)) break;
                if(element.Equals(Global.BOX0) || element.Equals(Global.BOX1) || element.Equals(Global.BOX2))
                {
                    adjacentCount++;
                    break;
                } 
            }

            for (int k = 1; k < indexRight+1; k++)
            {
                element = presentGrid[i,j+k];
                if(element.Equals(Global.WALL) || element.Equals(Global.POWERUP_EXTRA) || element.Equals(Global.POWERUP_RANGE)) break;
                if(element.Equals(Global.BOX0) || element.Equals(Global.BOX1) || element.Equals(Global.BOX2))
                {
                    adjacentCount++;
                    break;
                } 
            }
            //Console.Error.WriteLine("x = " + i + ", y = " + j + ", " + adjacentCount + " Up:" + indexUp + " Down:" + indexDown + " Left:" + indexLeft + " Right:" + indexRight);
            adjacentBoxesArray[i,j] = adjacentCount;
            adjacentCount = 0;
         }
        }
    }

    public void CalculateFutureExplosions(int bombRange, int x, int y) // iznuļļo nākotnes bumbas vietas
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
                if (presentGrid[y-k, x].Equals(Global.WALL)) break; // uz attiecīgo pusi ir siena
                if (!presentGrid[y-k, x].Equals(Global.EMPTY_CELL)) // uz attiecīgo pusi ir kaut kāds objekts
                {
                    futureGrid[y-k,x] = Global.EXPLOSION;
                    break;
                }
                futureGrid[y-k,x] = Global.EXPLOSION;
            }
 
            for (int k = 1; k < indexDown+1; k++)
            {
                if (presentGrid[y+k, x].Equals(Global.WALL)) break; // uz attiecīgo pusi ir siena
                if (!presentGrid[y+k, x].Equals(Global.EMPTY_CELL)) // uz attiecīgo pusi ir kaut kāds objekts
                {
                    futureGrid[y+k,x] = Global.EXPLOSION;
                    break;
                }
                futureGrid[y+k,x] = Global.EXPLOSION;
            }

            for (int k = 1; k < indexLeft+1; k++)
            {
                if (presentGrid[y,x-k].Equals(Global.WALL)) break; // uz attiecīgo pusi ir siena
                if (!presentGrid[y, x-k].Equals(Global.EMPTY_CELL)) // uz attiecīgo pusi ir kaut kāds objekts
                {
                    futureGrid[y,x-k] = Global.EXPLOSION;
                    break;
                }
                futureGrid[y,x-k] = Global.EXPLOSION;
            }

            for (int k = 1; k < indexRight+1; k++)
            {
                if (presentGrid[y,x+k].Equals(Global.WALL)) break; // uz attiecīgo pusi ir siena
                if (!presentGrid[y, x+k].Equals(Global.EMPTY_CELL)) // uz attiecīgo pusi ir kaut kāds objekts
                {
                    futureGrid[y,x+k] = Global.EXPLOSION;
                    break;
                }
                futureGrid[y,x+k] = Global.EXPLOSION;
            }

            futureGrid[y,x] = Global.EXPLOSION;
    }
 
    public Coordinates getBestCoordinates(Player player)
    {
        int maxCount = 0;
        List<Coordinates> list = new List<Coordinates>();
        printGrid("reachable");

        for (int i = 0; i < Global.HEIGHT; i++) // rindas
        {
         for (int j = 0; j < Global.WIDTH; j++) // kolonnas
         {
             if (reachableGrid[i,j].Equals(Global.REACHABLE) && !presentGrid[i,j].Equals(Global.BOMB))
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
            if (tempDistance < closestDistance && reachableGrid[list[i].x , list[i].y].Equals('R'))
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

        for (int i = 0; i < Global.HEIGHT; i++) // rindas
        {
            for (int j = 0; j < Global.WIDTH; j++) // kolonnas
            {
                if (reachableGrid[i,j].Equals(Global.REACHABLE) && !futureGrid[i,j].Equals(Global.EXPLOSION))
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
            if (tempDistance < closestDistance && reachableGrid[list[i].y,list[i].x].Equals(Global.REACHABLE))
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