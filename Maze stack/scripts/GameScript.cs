using Godot;
using System;

public class GameScript : Node2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    
    // Called when the node enters the scene tree for the first time.
    public Godot.TileMap maze;
    public Area2D player;
    public override void _Ready()
    {
        var player = GetNode<Godot.KinematicBody2D>("Player");
        var maze = GetNode<Godot.TileMap>("MazeTilemap");
        
        GD.Print("mazeGame: "+maze);
        Vector2 positionV = new Vector2((Vector2)maze.Call("SetPacmanSpawn"));
        positionV = positionV + new Vector2(15,-15);
        player.Position = positionV;
        GD.Print("setpmspwn: "+player.Position);
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
