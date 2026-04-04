using Godot;

public partial class LinearMovement : Node
{
    [Export] public float ProjectileSpeed = 2000f;
    public Vector2 Direction;

    public override void _PhysicsProcess(double delta)
    {
        var projectile = GetParent<Node2D>();
        projectile.GlobalPosition += Direction * ProjectileSpeed * (float)delta;
		
    }
}