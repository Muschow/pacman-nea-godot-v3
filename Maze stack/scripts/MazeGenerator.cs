using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class MazeGenerator : Godot.TileMap
{
    const int path = 0;
    const int wall = 1;
    [Export] private int width = 31;
    [Export] private int height = 19;
    [Export] private float tickDuration = 0.0f; //was 0.03f

    [Export] private int mazeOriginY = 0;
    [Export] private int maxNumMazes = 3; //maybe make public

    //private int mazeOriginX = 0; //most likely delete this
    private bool generationComplete = false;

    public int mazesOnScreen = 0; //have the ghost maze wall decrease this number when passing a maze chunk mazeOnScreen -= 1; //maybe make public

    //int[,] mazeArray = new int[height*maxNumMazes,width];
    static Vector2 north = new Vector2(0, -1);
    static Vector2 east = new Vector2(1, 0);
    static Vector2 south = new Vector2(0, 1);
    static Vector2 west = new Vector2(-1, 0);
    Vector2[] directions = new Vector2[] { north, east, south, west };
    List<Vector2> visited = new List<Vector2>();

    Stack<Vector2> rdfStack = new Stack<Vector2>();

    List<Vector2> wallEdgeList = new List<Vector2>();
    private int backtrackCount = 0;

    public Vector2 pacmanSpawn = new Vector2(970,778);
    private void CorrectMazeSize()
    {
        if (width % 2 != 1)
        {
            width -= 1;
        }
        if (height % 2 != 1)
        {
            height -= 1;
        }
        GD.Print("width " + width);
        GD.Print("height " + height);
    }

    // private void ScaleScreenSize()
    // {
    //     float xScale = 1920 / (width * 32f);
    //     float yScale = 1080 / (height * 32f);
    //     Scale = new Vector2(xScale, yScale);

    // }

    private void CreateStartingGrid()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                //wall tile edges
                if (i == 0 || j == 0 || i == width - 1 || j == height - 1)
                {
                    SetCell(i, j + mazeOriginY, wall);
                    //mazeArray[j+mazeOriginY,i] = wall;

                    Vector2 wallEdge = new Vector2(i, j + mazeOriginY);
                    wallEdgeList.Add(wallEdge);
                }
                //alternating wall tiles
                else if (i % 2 == 0 || j % 2 == 0)
                {
                    SetCell(i, j + mazeOriginY, wall);
                    //mazeArray[j+mazeOriginY, i] = wall;
                }
                //path tiles
                else
                {
                    SetCell(i, j + mazeOriginY, path);
                    //mazeArray[j+mazeOriginY, i] = path;
                }

            }
        }
    }

    private void AddLoops(Vector2 currentV)
    {
        bool complete = false;

        for (int i = 0; i < directions.Length; i++)
        {
            if (!complete)
            {
                Vector2 newCell = new Vector2(currentV + directions[i]);
                if ((GetCellv(newCell) == wall) && (!wallEdgeList.Contains(newCell)) && (!visited.Contains(newCell)))
                {
                    SetCellv(currentV + directions[i], path);
                    //mazeArray[(int)(currentV + directions[i]).x,(int)(currentV + directions[i]).y] = path;
                    complete = true;
                }
            }
        }
    }

    private void JoinMazes()
    {
        Random rnd = new Random();
        List<Vector2> usedCells = new List<Vector2>();

        int oldY = mazeOriginY + height - 1;
        //GD.Print("Maze+height " + oldY); //debug

        double numHoles = Math.Round((double)width / 4); //maybe have numHoles be width/4 rounded up/down to nearest integer
        while (usedCells.Count < numHoles * 3)
        {
            int cellX = rnd.Next(1, width);
            Vector2 cell = new Vector2(cellX, oldY);

            if ((GetCellv(cell + south)) == path && (GetCellv(cell + north) == path) && (!usedCells.Contains(cell)))
            {
                SetCellv(cell, path);
                //mazeArray[(int)cell.x,(int)cell.y] = path;
                usedCells.Add(cell);
                usedCells.Add(cell + east);
                usedCells.Add(cell + west);
                //GD.Print("SetCellx path:" + cell); //debug
            }
            //GD.Print("usedCellsCount: "+usedCells.Count); //debug
        }
    }

    private Vector2 SetPacmanSpawn(){
        Random rnd = new Random();
        int x = rnd.Next(1,width);
        while (GetCell(x,height-2) == wall){
            x = rnd.Next(1,width);
        }

        pacmanSpawn = new Vector2(x,height-1);
        GD.Print("pacmanspawn: "+pacmanSpawn);
        pacmanSpawn = new Vector2(MapToWorld(pacmanSpawn));
        GD.Print("MTWpacmanspawn: "+pacmanSpawn);
        return pacmanSpawn;
    }

    private void rdfInit()
    {

        generationComplete = false;


        CorrectMazeSize();
        CreateStartingGrid();

        //startVector x and y must be odd, between 1+mazeOriginX/Y & height-1 / width-1 
        Vector2 startVector = new Vector2(1, mazeOriginY + 1); //Choose the initial cell,
        GD.Print("StartV: " + startVector); //debug

        visited.Add(startVector); //Mark initial cell as visited,
        rdfStack.Push(startVector); //and push it to the stack,

        rdfStep();
    }

    private void rdfStep()
    {

        while (!generationComplete)
        {
            Vector2 curr = rdfStack.Pop(); //Pop a cell from the stack and make it a current cell.
            Vector2 next = new Vector2(0, 0);
            bool found = false;

            //check neighbours in random order //N,E,S,W walls instead of their paths, so *2
            Random rnd = new Random();
            var rndDirections = directions.OrderBy(_ => rnd.Next()).ToList(); //found this online, randomly shuffle the list.

            for (int i = 0; i < rndDirections.Count; i++)
            {
                next = 2 * rndDirections[i];
                if (GetCellv(curr + next) == path && (!visited.Contains(curr + next)))
                { //If the current cell has any neighbours which have not been visited,
                    found = true;
                    break; //Choose one of the unvisited neighbours (next),
                }
            }

            if (found)
            {
                rdfStack.Push(curr); //Push the current cell to the stack,
                SetCellv(curr + (next / 2), path); // Remove the wall between the current cell and the chosen cell,
                //mazeArray[(int)(curr+(next/2)).y,(int)(curr+(next/2)).x] = path;
                visited.Add(curr + next); //Mark the chosen cell as visited,
                rdfStack.Push(curr + next); //and push it to the stack.  
                backtrackCount = 0;
            }
            else
            {
                backtrackCount++;
                if (backtrackCount == 1)
                {
                    AddLoops(curr);
                }
            }

            if (rdfStack.Count <= 0)
            { //While stack is not empty, (if stack is empty)
                AddLoops(curr);
                generationComplete = true;
                mazesOnScreen++;
                //GD.Print("mazesOnScreen: "+mazesOnScreen); //debug

                if (mazesOnScreen > 1)
                    JoinMazes();

                GD.Print("Maze Generation Complete!"); //debug
                return;
            }
        }

    }

    private void printGraph()
    {

        for (int x = 0; x < width;x++){
            for (int y = 0; y < height*maxNumMazes; y++){
                    //GD.Print(mazeArray[y,x]);
            }
        }
    }

    //Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        
        GD.Randomize();
        rdfInit();
        SetPacmanSpawn();
    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.

    double timeSinceTick = 0.0;
    public override void _Process(float delta)
    {
        if (mazesOnScreen < maxNumMazes)
        {
            //GD.Print("MazeOriginY: " + mazeOriginY); //debug
            mazeOriginY -= height - 1;
            rdfInit();
            //GD.Print("MazeOriginY: " + mazeOriginY); //debug
        }
        else if (mazesOnScreen == maxNumMazes){
            printGraph();
        }
        

    }
}