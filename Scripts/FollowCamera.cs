using Godot;
using System;

public partial class FollowCamera : Camera2D
{
    [Export]
    public Node2D Target;
    [Export]
    public bool Following = false;

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (IsInstanceValid(Target) && Following)
        {
            //GlobalPosition = Target.GlobalPosition;
            GlobalPosition = GlobalPosition.Lerp(Target.GlobalPosition, 0.1f);
        }
        else
        {
            Following = false;
        }
    }
}
