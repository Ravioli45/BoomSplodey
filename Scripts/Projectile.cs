using Godot;
using System;

public partial class Projectile : Node2D
{
	[Export]
	public int damage = 1;

	public Node playerOwner;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	public virtual void Hit(Node body)
    {
        if (body == playerOwner)
            return;

        QueueFree();
    }
}
