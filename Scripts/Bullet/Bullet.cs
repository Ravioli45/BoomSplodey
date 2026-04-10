using Godot;
using System;

public partial class Bullet : CharacterBody2D
{
    [Export]
    public long OwnerId = 1;
    [Export]
    public int Damage = 1;
    [Export]
    public float KnockbackStrength;

    [Export]
    public CollisionShape2D Collider;

    public override void _Ready()
    {
        base._Ready();

        //SetPhysicsProcess(Multiplayer.IsServer());

        // disable collision if not the player
        Collider?.SetDeferred(CollisionShape2D.PropertyName.Disabled, !Multiplayer.IsServer());
    }
}
