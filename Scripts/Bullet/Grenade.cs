using Godot;
using System;

public partial class Grenade : Bullet
{
    [Export]
    private Area2D ExplosionArea;
    [Export]
    private float GravityScale = 1;
    [Export]
    private float Bounciness = 1;

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        Velocity += GetGravity() * GravityScale * (float)delta;
        Rotation = Velocity.Angle();

        Vector2 motion = Velocity * (float)delta;
        var collision = MoveAndCollide(motion);

        if (IsInstanceValid(collision) && Multiplayer.IsServer())
        {

            if (collision.GetCollider() is Player p)
            {

                Explode();
            }

            Velocity = Velocity.Bounce(collision.GetNormal()) * Bounciness;
        }
    }

    private void OnExplodeTimeout()
    {

        Explode();
        //Rpc(Bullet.MethodName.Explode);
        //QueueFree();
    }

    private void Explode()
    {
        if (!Multiplayer.IsServer())
        {
            return;
        }

        foreach (Node2D body in ExplosionArea.GetOverlappingBodies())
        {
            if (body is Player p)
            {
                p.TakeDamage(Damage);
                p.TakeKnockback(GlobalPosition.DirectionTo(p.GlobalPosition) * KnockbackStrength);
            }
        }

        QueueFree();
    }
}