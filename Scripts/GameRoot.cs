using Godot;
using System;
using Godot.Collections;

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
    private Dictionary<long, byte> PlayerInfo = [];

    [Export]
    private Control GameLobbyUI;
    [Export]
    private Button StartButton;

    [Export(PropertyHint.FilePath)]
    private string LevelScenePath;
    [Export]
    private Level Level;

    public bool Started { get; private set; } = false;

    public override void _Ready()
    {
        base._Ready();

        UpdateHostUI();
    }

    public void AddPlayer(long id)
    {
        if (!Multiplayer.IsServer())
        {
            GD.PushWarning("only server should try to add players");
            return;
        }

        PlayerInfo.Add(id, 0);

        if (Started && IsInstanceValid(Level))
        {
            Level.AddPlayer(id);
        }
    }

    public void RemovePlayer(long id)
    {
        if (!Multiplayer.IsServer())
        {
            GD.PushWarning("only server should try to remove players");
            return;
        }

        PlayerInfo.Remove(id);

        if (Started && IsInstanceValid(Level))
        {
            Level.RemovePlayer(id);
        }
    }

    private void UpdateHostUI()
    {
        if (Multiplayer.GetUniqueId() == HostId)
        {
            StartButton.Visible = true;
        }
        else
        {
            StartButton.Visible = false;
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void StartGame()
    {
        if (!Multiplayer.IsServer() || Multiplayer.GetRemoteSenderId() != HostId)
        {
            GD.PushWarning("game start must be requested to server by host");
            return;
        }

        GD.Print("Starting game");

        PackedScene levelScene = GD.Load<PackedScene>(LevelScenePath);
        Level = levelScene.Instantiate<Level>();
        AddChild(Level, true);

        foreach ((long id, var _options) in PlayerInfo)
        {
            Level.AddPlayer(id);
        }

        Rpc(MethodName.DisableGameLobbyUI);

        Started = true;
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
}
