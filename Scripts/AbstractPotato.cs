using Godot;
using System;

public partial class AbstractPotato : RigidBody2D
{
	[Export]
	public Vector2 Direction;

	[Export]
	public Area2D area;


	[Export]
	public int LaunchSpeed = 2000;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
		
		LinearVelocity = Direction.Normalized() * LaunchSpeed;
		GD.Print("Velocity: ", Direction);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	
	}

	public void Explode()
	{
		
		 if(!Multiplayer.IsServer())return;

        // Detect targets in explosion area
        

        foreach (var body in area.GetOverlappingBodies())
        {
			if(body is Player P){
            GD.Print("Hit: " + P.Name);
			//(Player)body.takedamage
			}
        }
		QueueFree();
	}
	
	public override void _IntegrateForces(PhysicsDirectBodyState2D state)
	{

		if (state.LinearVelocity.Length() > 0.1f)
    	{
        state.Transform = new Transform2D(state.LinearVelocity.Angle(), state.Transform.Origin);
    	}

    	if (state.GetContactCount() > 0)
    	{
        	LinearVelocity *= 0.8f;
    	}
		}
}
