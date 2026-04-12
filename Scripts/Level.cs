using Godot;
using System;
using Godot.Collections;

public partial class Level : Node2D
{
    [Export]
    private Node SpawnpointsParent;

    [Export]
    private PackedScene PlayerScene;

    private Dictionary<long, Player> PlayerObjects = [];

    private Array<Node2D> Spawnpoints = [];

    public override void _Ready()
    {
        base._Ready();

        foreach (Node n in SpawnpointsParent.GetChildren())
        {
            if (n is Node2D n2d)
            {
                Spawnpoints.Add(n2d);
            }
        }
    }

    public void AddPlayer(long id, PlayerInfo info = null)
    {
        if (!Multiplayer.IsServer())
        {
            GD.PushWarning("only server can spawn players");
            return;
        }
        GD.Print($"adding player for {id}");

        Player newPlayer = PlayerScene.Instantiate<Player>();

        newPlayer.Position = Spawnpoints.PickRandom().Position;
        //newPlayer.WeaponIndex = GD.RandRange(0, GlobalResources.Instance.Weapons.Count - 1);
        newPlayer.WeaponIndex = info?.SelectedWeapon ?? 0;
        newPlayer.HatIndex = info?.SelectedHat ?? 0;
        newPlayer.PlayerName = info?.PlayerName ?? "";
        newPlayer.OwnerId = id;
        AddChild(newPlayer, true);

        PlayerObjects.Add(id, newPlayer);
    }

    public void RemovePlayer(long id)
    {
        if (!Multiplayer.IsServer())
        {
            GD.PushWarning("only server can despawn players");
            return;
        }

        Player oldPlayer = PlayerObjects[id];
        PlayerObjects.Remove(id);

        oldPlayer.QueueFree();
    }
}
