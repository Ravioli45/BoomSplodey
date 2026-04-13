using Godot;
using Godot.Collections;
using System;

public partial class GlobalResources : Node
{
    [Export]
    public Array<WeaponResource> Weapons;
    [Export(PropertyHint.File)]
    public Array<String> LevelScenePaths;

    public static GlobalResources Instance { get; private set; }

    public override void _Ready()
    {
        base._Ready();

        if (Instance != null)
        {
            GD.PushWarning("More than one GlobalResources detected");
            return;
        }

        Instance ??= this;
    }
}
