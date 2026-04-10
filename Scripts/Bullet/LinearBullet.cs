using Godot;
using System;

public partial class LinearBullet : Bullet
{
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        Vector2 motion = Velocity * (float)delta;
        var collision = MoveAndCollide(motion);

        if (IsInstanceValid(collision) && Multiplayer.IsServer())
        {
            if (collision.GetCollider() is Player p)
            {
                p.TakeDamage(Damage);
                //p.TakeKnockback(KnockbackStrength * GlobalTransform.X);
                p.TakeKnockback(GlobalPosition.DirectionTo(p.GlobalPosition) * KnockbackStrength);
            }

            QueueFree();
        }
    }
}