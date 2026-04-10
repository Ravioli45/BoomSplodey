using Godot;
using System;
using Godot.Collections;

public partial class HealthBar : Control
{
    [Export]
    private Texture2D FullHeartTexture;
    [Export]
    private Texture2D HalfHeartTexture;
    [Export]
    private Array<TextureRect> Hearts;

    public override void _Ready()
    {
        base._Ready();

        if (Hearts.Count != 3)
        {
            GD.PushError("Health bar configured incorrectly");
            return;
        }

        UpdateHearts(6);
    }

    public void UpdateHearts(int health)
    {
        //int healthCopy = Health;

        foreach (TextureRect h in Hearts)
        {
            if (health >= 2)
            {
                h.Texture = FullHeartTexture;
            }
            else if (health == 1)
            {
                h.Texture = HalfHeartTexture;
            }
            else
            {
                h.Texture = null;
            }

            health -= 2;
        }
    }
}
