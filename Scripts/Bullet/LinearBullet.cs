using Godot;
using System;

public partial class LinearBullet : Bullet
{

    [Export]
    PackedScene explosion;
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        Vector2 motion = Velocity * (float)delta;
        var collision = MoveAndCollide(motion);

        if (IsInstanceValid(collision) && Multiplayer.IsServer())
        {
            if (collision.GetCollider() is Player p && !p.Disabled)
            {
                p.TakeDamage(OwnerId, Damage);
                //p.TakeKnockback(KnockbackStrength * GlobalTransform.X);
                p.TakeKnockback(GlobalPosition.DirectionTo(p.GlobalPosition) * KnockbackStrength);
                
            }
            Explosion();
            QueueFree();
        }
    }

    public void Explosion()
    {
        if (explosion != null)
        {
            GpuParticles2D tempExplosion = explosion.Instantiate<Explosion>();
            //tempExplosion.GlobalPosition = GlobalPosition;
            tempExplosion.Position = Position;
            GetParent().AddChild(tempExplosion, true);
            //tempExplosion.GlobalPosition = this.GlobalPosition;
            //tempExplosion.Emitting = true;
        }
    }
}