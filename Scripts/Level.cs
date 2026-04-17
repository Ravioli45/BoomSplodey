using Godot;
using System;
using Godot.Collections;

public partial class Level : Node2D
{
    [Signal]
    public delegate void PlayerDamageDealtUpdatedEventHandler(long id, int damage);
    [Signal]
    public delegate void PlayerKilledUpdateEventHandler(long id);
    [Signal]
    public delegate void RoundEndedEventHandler();

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

        AudioManager.Instance.PlayBGM("battle");
    }

    public void AddPlayer(long id, PlayerInfo info = null, int? spawnPointIndex = null)
    {
        if (!Multiplayer.IsServer())
        {
            GD.PushWarning("only server can spawn players");
            return;
        }
        GD.Print($"adding player for {id}");

        Player newPlayer = PlayerScene.Instantiate<Player>();

        newPlayer.DamagedBy += OnPlayerDamagedBy;
        newPlayer.KilledBy += OnPlayerKilledBy;
        //newPlayer.Position = Spawnpoints.PickRandom().Position;
        newPlayer.Position = spawnPointIndex.HasValue ? Spawnpoints[spawnPointIndex.Value].Position : Spawnpoints.PickRandom().Position;
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

        //Player oldPlayer = PlayerObjects[id];
        //PlayerObjects.Remove(id);

        //oldPlayer.QueueFree();

        if (PlayerObjects.TryGetValue(id, out Player p))
        {
            PlayerObjects.Remove(id);
            p.DamagedBy -= OnPlayerDamagedBy;
            p.KilledBy -= OnPlayerKilledBy;

            p.QueueFree();
        }
    }

    public void InitialSpawnPlayers((long, PlayerInfo)[] players)
    {
        if (!Multiplayer.IsServer())
        {
            GD.Print("only server can initialize level");
            return;
        }

        GD.Print(players);

        for (int i = 0; i < players.Length; i++)
        {
            var (id, info) = players[i];
            AddPlayer(id, info, i);
        }
    }

    private void OnPlayerDamagedBy(long damagedBy, long damaged, int damage)
    {
        GD.Print($"{damagedBy} dealt {damage} damage");

        if (damagedBy != damaged) EmitSignalPlayerDamageDealtUpdated(damagedBy, damage);
    }
    private void OnPlayerKilledBy(long killer, long died)
    {
        GD.Print($"{killer} killed {died}");

        if (PlayerObjects.TryGetValue(died, out Player p))
        {
            p.Disabled = true;
            //p.Velocity = Vector2.Zero;
            //p.nextRecoil = Vector2.Zero;
            //p.Position = Spawnpoints.PickRandom().Position;
            //p.currentHP = p.maxHP;
            p.SetDeferred(Player.PropertyName.Position, Spawnpoints.PickRandom().Position);
            p.SetDeferred(Player.PropertyName.Velocity, Vector2.Zero);
            p.SetDeferred(Player.PropertyName.nextRecoil, Vector2.Zero);
            p.SetDeferred(Player.PropertyName.currentHP, p.maxHP);
            p.SetDeferred(Player.PropertyName.Disabled, false);
        }

        if (killer != died) EmitSignalPlayerKilledUpdate(killer);
    }

    private void OnLevelTimeout()
    {
        if (!Multiplayer.IsServer())
        {
            return;
        }

        GD.Print("round ended");

        //foreach (Player p in PlayerObjects.Values)
        //{
        //    //p.Disabled = true;
        //}
        //GetTree().Paused = true;
        Rpc(MethodName.StopLevel);

        EmitSignalRoundEnded();
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void StopLevel()
    {
        ProcessMode = ProcessModeEnum.Disabled;
    }
}
