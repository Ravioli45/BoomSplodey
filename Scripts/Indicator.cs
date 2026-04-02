using Godot;
using System;

[GlobalClass]
[Tool]
public partial class Indicator : Node2D
{
    [Signal]
    public delegate void InEditorMovedEventHandler(Vector2 newPosition);

    [Export]
    public float Radius
    {
        get;
        set
        {
            field = value;
            QueueRedraw();
        }
    }

    public override void _Ready()
    {
        base._Ready();

        SetNotifyLocalTransform(true);
    }

    public override void _Notification(int what)
    {
        base._Notification(what);

        if (what == NotificationLocalTransformChanged && Engine.IsEditorHint())
        {
            EmitSignalInEditorMoved(Position);
        }
    }

    public override void _Draw()
    {
        base._Draw();

        if (Engine.IsEditorHint())
        {
            DrawCircle(Vector2.Zero, Radius, Colors.Orange);
        }
    }
}
