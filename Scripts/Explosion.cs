using Godot;
using System;

public partial class Explosion : GpuParticles2D
{
    [Export]
    private AudioStreamPlayer2D BoomSoundPlayer;
    public override void _Ready()
    {
        base._Ready();

        Emitting = true;
        BoomSoundPlayer.Stream = GlobalResources.Instance.ExplosionSounds.PickRandom();
        BoomSoundPlayer.Play();
    }

    public void OnTimeout()
    {
        if (!Multiplayer.IsServer())
        {
            return;
        }

        QueueFree();
    }
}
