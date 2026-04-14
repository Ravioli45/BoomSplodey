using Godot;
using System;

public partial class LevelTimer : CanvasLayer
{
    [Signal]
    public delegate void TimeoutEventHandler();

    [Export]
    private float WaitTime;
    [Export]
    private Timer InternalTimer;
    [Export]
    private Label TimerLabel;

    public override void _Ready()
    {
        base._Ready();

        InternalTimer.Start(WaitTime);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        int secondsLeft = (int)InternalTimer.TimeLeft;
        int minutes = secondsLeft / 60;
        int seconds = secondsLeft - (minutes * 60);
        TimerLabel.Text = $"{minutes}:{seconds:D2}";
    }

    private void OnInternalTimeout()
    {
        EmitSignalTimeout();
    }
}
