using Godot;
using System;

public partial class InputSynchronizer : MultiplayerSynchronizer
{
    [Export]
    public float InputMove { get; private set; } = new();
    [Export]
    public Vector2 MousePosition { get; private set; } = new();

    [Export]
    public bool Jumping { get; set; } = false;
    [Export]
    public bool Shooting { get; set; } = false;

    public override void _Ready()
    {
        base._Ready();

        SetPhysicsProcess(GetMultiplayerAuthority() == Multiplayer.GetUniqueId());
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        InputMove = Input.GetAxis("left", "right");

        // TODO: reevaluate
        //MousePosition = GetViewport().GetCamera2D().GetGlobalMousePosition();
        if (GetViewport().GetCamera2D() is Camera2D c && IsInstanceValid(c))
        {
            MousePosition = c.GetGlobalMousePosition();
        }
        else
        {
            MousePosition = Vector2.Zero;
        }

        if (Input.IsActionPressed("jump"))
        {
            Rpc(MethodName.Jump);
        }

        if (Input.IsActionJustPressed("shoot"))
        {
            Rpc(MethodName.Shoot);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Jump()
    {
        Jumping = true;
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Shoot()
    {
        Shooting = true;
    }
}
