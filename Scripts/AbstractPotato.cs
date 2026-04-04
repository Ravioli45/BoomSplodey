using Godot;
using System;

public partial class AbstractPotato : RigidBody2D
{
	[Export]
	public Vector2 Direction;

	[Export]
	public CollisionShape2D col;

	public int collisionoff = 2;

	[Export]
	public int LaunchSpeed = 1500;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//ApplyImpulse(Vector2.Zero, Direction.Normalized() * LaunchSpeed);
		LinearVelocity = Direction.Normalized() * LaunchSpeed;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (LinearVelocity.Length() > 0.1f)
        {
            Rotation = LinearVelocity.Angle();
        }
		if (collisionoff >= 0)
		{
			collisionoff--;
		}
		else
		{
			col.Disabled = false;
		}

	}

	
}
