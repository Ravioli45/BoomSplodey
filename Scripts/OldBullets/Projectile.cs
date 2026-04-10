using Godot;
using System;

public partial class Projectile : Node2D
{
	[Export]
	public int damage = 1;
	[Export]
	public float knockback;

	public Node playerOwner;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	public virtual void Hit(Node body)
    {
		if(!Multiplayer.IsServer()) 
			return;

        if (body == playerOwner)
            return;
		else if (body is Player p)
		{
			p.TakeDamage(damage);
			p.TakeKnockback(GetNode<LinearMovement>("LinearMovement").Direction * knockback);
		}
        QueueFree();
    }
}
