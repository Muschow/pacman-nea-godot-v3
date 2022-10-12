using Godot;
using System;

public class PlayerScript : KinematicBody2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    
        public Vector2 direction;
        float movementSpeed = 200f;
        Vector2 prevDirection;
        Vector2 storedDir;
        string key;

        private Vector2 moveChar(Vector2 direction){
            direction = MoveAndSlide(direction,Vector2.Up);
            prevDirection = direction;
            return prevDirection;
        }

        public override void _Ready(){

        }
        public override void _PhysicsProcess(float delta){
            

            direction.x = Input.GetActionStrength("move_right")-Input.GetActionStrength("move_left");
            direction.y = Input.GetActionStrength("move_down")-Input.GetActionStrength("move_up");
            GD.Print("directionx"+direction.x);
            GD.Print("directiony"+direction.y);
            direction.x*=movementSpeed;
            direction.y*=movementSpeed;
            if (direction.x !=0 & direction.y != 0){
                direction = new Vector2(0,0);
            }

            //if horizontal block vertical
            //if vertical block horizontal

            if (direction != new Vector2(0,0) || direction == storedDir){
                storedDir = new Vector2(direction);
            }
            //for i in get_slide_count(): var collision = get_slide_collision(i) print("Collided with: ", collision.collider.name)

           moveChar(storedDir);
        }
        //need to make animation flip when going left right up down
    
}
