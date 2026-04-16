using Godot;
using System;

public partial class KillPlane : Area2D
{
    private void OnBodyEntered(Node2D body)
    {
        //GD.Print("kill plane killing!!!!!!");

        if (Multiplayer.IsServer() && body is Player p)
        {
            p.TakeDamage(-1, 500);
        }
    }
}
