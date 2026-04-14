using Godot;
using System;
using Godot.Collections;
using System.Linq;

public partial class GameRoot : Node
{
    [Signal]
    public delegate void GameStartedEventHandler();

    [Export]
    public long HostId
    {
        get => field;
        set
        {
            field = value;

            // update host stuff
            if (IsNodeReady())
            {
                UpdateHostUI();
            }
        }
    } = 1;

    // TODO: replace byte with something useful
    [Export]
    private Dictionary<long, PlayerDisplay> Displays = [];

    [Export]
    private Control GameLobbyUI;
    [Export]
    private Button StartButton;
    [Export]
    private int SelectedLevel
    {
        get => field;
        set
        {
            field = Mathf.Clamp(value, 0, 2);

            //TODO: change level
            if (IsNodeReady())
            {
                LevelDropdown.Selected = field;
            }
        }
    } = 0;
    [Export]
    private OptionButton LevelDropdown;

    [Export(PropertyHint.FilePath)]
    private string LevelScenePath;
    [Export]
    private Level Level;

    [Export]
    private PackedScene PlayerDisplayScene;
    [Export]
    private Container PlayerDisplayContainer;

    public bool Started { get; private set; } = false;

    public override void _Ready()
    {
        base._Ready();

        UpdateHostUI();

        SelectedLevel = SelectedLevel;
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        if (Level != null)
        {
            Level.PlayerDamageDealtUpdated -= OnPlayerDamageUpdate;
            Level.PlayerKilledUpdate -= OnPlayerKillUpdate;
        }
    }

    public void AddPlayer(long id)
    {
        if (!Multiplayer.IsServer())
        {
            GD.PushWarning("only server should try to add players");
            return;
        }

        PlayerDisplay newDisplay = PlayerDisplayScene.Instantiate<PlayerDisplay>();
        newDisplay.OwnerId = id;
        // TODO: connect signals
        newDisplay.ReadyToggled += OnReadyToggled;

        Displays.Add(id, newDisplay);
        PlayerDisplayContainer.AddChild(newDisplay, true);

        if (Started && IsInstanceValid(Level))
        {
            Level.AddPlayer(id);
        }

        OnReadyToggled();
    }

    public void RemovePlayer(long id)
    {
        if (!Multiplayer.IsServer())
        {
            GD.PushWarning("only server should try to remove players");
            return;
        }

        //Displays.Remove(id);
        if (Displays.TryGetValue(id, out PlayerDisplay display))
        {
            Displays.Remove(id);
            // TODO: disconnect signals
            display.ReadyToggled -= OnReadyToggled;

            display.QueueFree();
        }

        if (Started && IsInstanceValid(Level))
        {
            Level.RemovePlayer(id);
        }

        OnReadyToggled();
    }

    private void UpdateHostUI()
    {
        if (Multiplayer.GetUniqueId() == HostId)
        {
            StartButton.Visible = true;
            LevelDropdown.Disabled = false;
        }
        else
        {
            StartButton.Visible = false;
            LevelDropdown.Disabled = true;
        }
    }

    private bool CanGameStart()
    {
        return Displays.Count >= 2 && Displays.Values.All(display => display.Info.IsReady);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void StartGame()
    {
        if (!Multiplayer.IsServer() || Multiplayer.GetRemoteSenderId() != HostId)
        {
            GD.PushWarning("game start must be requested to server by host");
            return;
        }

        GD.Print("Starting game");

        //PackedScene levelScene = GD.Load<PackedScene>(LevelScenePath);
        PackedScene levelScene = GD.Load<PackedScene>(GlobalResources.Instance.LevelScenePaths[SelectedLevel]);
        Level = levelScene.Instantiate<Level>();
        Level.PlayerDamageDealtUpdated += OnPlayerDamageUpdate;
        Level.PlayerKilledUpdate += OnPlayerKillUpdate;

        AddChild(Level, true);

        foreach ((long id, PlayerDisplay display) in Displays)
        {
            GD.Print(display.Info);
            //Level.AddPlayer(id, display.Info);
        }
        
        Level.InitialSpawnPlayers(Displays.Select(kvp => (kvp.Key, kvp.Value.Info)).ToArray());

        Rpc(MethodName.DisableGameLobbyUI);

        Started = true;
        EmitSignalGameStarted();
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SelectLevel(int index)
    {
        if (!Multiplayer.IsServer() || Multiplayer.GetRemoteSenderId() != HostId)
        {
            GD.PushWarning("level select must be requested to server by host");
            return;
        }

        SelectedLevel = index;
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void DisableGameLobbyUI()
    {
        GameLobbyUI.ProcessMode = ProcessModeEnum.Disabled;
        GameLobbyUI.Visible = false;
    }

    private void OnStartPressed()
    {
        if (Multiplayer.GetUniqueId() != HostId)
        {
            GD.PushWarning("only host should be pressing start button");
            return;
        }

        RpcId(1, MethodName.StartGame);
    }
    private void OnLevelSelected(int index)
    {
        if (Multiplayer.GetUniqueId() != HostId)
        {
            GD.PushWarning("only host should be selecting level");
            return;
        }

        RpcId(1, MethodName.SelectLevel, index);
    }

    private void OnReadyToggled()
    {
        //GD.Print("game root ready toggle");
        if (!Multiplayer.IsServer())
        {
            GD.PushWarning("Only server should be here");
            return;
        }

        //RpcId(HostId, MethodName.SetStartButtonDisabled, !CanGameStart());
        Rpc(MethodName.SetStartButtonDisabled, !CanGameStart());
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SetStartButtonDisabled(bool disabled)
    {
        /*
        if (Multiplayer.GetRemoteSenderId() == 1 && Multiplayer.GetUniqueId() == HostId)
        {
            StartButton.Disabled = disabled;
        }
        else
        {
            GD.PushWarning("bad rpc call for SetStartButton");
        }
        */
        StartButton.Disabled = disabled;
    }

    private void OnPlayerDamageUpdate(long id, int damage)
    {
        if (Displays.TryGetValue(id, out PlayerDisplay d))
        {
            d.Info.DamageDealt += damage;
        }
    }
    private void OnPlayerKillUpdate(long id)
    {
        if (Displays.TryGetValue(id, out PlayerDisplay d))
        {
            d.Info.Kills += 1;
        }
    }
}
