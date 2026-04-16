using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class WANLobby : Node
{

    public enum WANLobbyState
    {
        Searching,
        Connected,
    }

    [Export]
    private string ServerIP;
    [Export]
    private int ServerPort;
    [Export]
    private int MaxPlayers;
    [Export]
    private ServerLobby ServerLobby;
    [Export(PropertyHint.FilePath)]
    private string GameScenePath;
    [Export]
    private GameRoot Game;

    [Export]
    public long HostId
    {
        get;
        set
        {
            field = value;
            if (IsInstanceValid(Game))
            {
                Game.HostId = field;
            }
        }
    } = 1;

    private WANMessageStream Stream = new();

    // currently only kept updated on server
    public SortedSet<long> Peers { get; private set; } = [];
    public List<LobbyInfo> Lobbies { get; private set; } = [];
    public WANLobbyState State { get; private set; } = WANLobbyState.Searching;
    private bool IsClient = false;

    public override void _Ready()
    {
        base._Ready();

        // emitted to every old peer when new peer joins
        // and to new peer multiple times
        Multiplayer.PeerConnected += OnPlayerConnected;

        // emitted on remaining peers when one disconnects
        Multiplayer.PeerDisconnected += OnPlayerDisconnected;

        // emitted on peer after successfully connecting to server ?
        //Multiplayer.ConnectedToServer += OnConnectOk;

        // emitted on peer after failing to connect to server ?
        Multiplayer.ConnectionFailed += OnConnectionFail;

        // emitted on server disconnecting ?
        Multiplayer.ServerDisconnected += OnServerDisconnected;

        if (DisplayServer.GetName() == "headless")
        {
            // act as a lobby on master server
            int port = -1;

            foreach (string arg in OS.GetCmdlineArgs())
            {
                //GD.Print(arg);
                if (arg.Contains("--port"))
                {
                    port = int.Parse(arg.Split("=")[1]);
                }
            }

            if (ActAsGameLobby(port) == Error.Ok)
            {
                GD.Print("OK");
                //GD.Print($"Port: {port}");
                //GD.Print(Multiplayer.IsServer());
            }
            else
            {
                GD.Print("ERR");
            }
        }
        else
        {
            // act as client
            ServerIP = GlobalResources.Instance.CustomIp ?? ServerIP;
            Stream.ConnectToHost(ServerIP, ServerPort);
            IsClient = true;
        }
        //Stream.ConnectToHost(ServerIP, ServerPort);
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        Stream.DisconnectFromHost();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (IsClient && Stream.GetStatus() == StreamPeerSocket.Status.None)
        {
            // attempt reconnect
            GD.Print("client try reconnect");
            Stream.ConnectToHost(ServerIP, ServerPort);
        }
        //GD.Print(Stream.GetStatus());
        Stream.Poll();
        //GD.Print();

        if (Stream.TryRecvNextMessage(out WANMessage message))
        {
            GD.Print(message);

            if (message is ListMessage list)
            {
                Lobbies = list.Games;
                ServerLobby.UpdateServerList(Lobbies.Select(element => element.LobbyName).ToList());
            }
            else if (message is CreatedMessage created)
            {
                // TODO: Join
                JoinGame(created.Port);
            }
            else if (message is NotCreatedMessage)
            {
                ServerLobby.CreateOkButton.Disabled = false;
            }
        }

        /*
        if (Input.IsActionJustPressed("ui_accept"))
        {
            //GD.Print("h");
            //GD.Print(Stream.SendMessage(new FetchMessage()));
            //Stream.SendMessage(new FetchMessage());
            Stream.SendMessage(new HostMessage { LobbyName = "Alice" });
        }
        */
    }

    private Error ActAsGameLobby(int port)
    {
        ENetMultiplayerPeer peer = new();

        Error error = peer.CreateServer(port, MaxPlayers + 1);

        if (error != Error.Ok)
        {
            GD.PushError(error);
            return error;
        }

        Multiplayer.MultiplayerPeer = peer;
        Peers.Add(1);

        // TODO: spawn game
        SpawnGame();

        return Error.Ok;
    }

    private void SpawnGame()
    {
        // TODO: implement
        if (!Multiplayer.IsServer())
        {
            GD.PushError("No spawn on client");
        }

        PackedScene gameScene = GD.Load<PackedScene>(GameScenePath);

        GameRoot game = gameScene.Instantiate<GameRoot>();

        // connect signal???
        game.Connect(GameRoot.SignalName.GameStarted, Callable.From(OnGameStart));
        AddChild(game, true);
        Game = game;
    }

    private Error JoinGame(int port)
    {
        GD.Print($"trying to join {ServerIP}:{port}");
        ENetMultiplayerPeer peer = new ENetMultiplayerPeer();

        Error error = peer.CreateClient(ServerIP, port);

        if (error != Error.Ok)
        {
            GD.PushError(error);
            return error;
        }

        ServerLobby.State = ServerLobby.ServerLobbyState.Off;
        Stream.SendMessage(new JoinMessage{Port = (ushort)port});

        Multiplayer.MultiplayerPeer = peer;

        return Error.Ok;
    }

    private void GetGames()
    {
        Stream.SendMessage(new FetchMessage());
    }
    private void OnGameStart()
    {
        GD.Print("START");
        Multiplayer.MultiplayerPeer?.RefuseNewConnections = true;
    }

    private void OnPlayerConnected(long id)
    {
        GD.Print("player connected");
        
        Peers.Add(id);
        if (Multiplayer.IsServer())
        {
            if (HostId == 1) HostId = id;
        }

        if (Multiplayer.IsServer() && IsInstanceValid(Game))
        {
            Game.AddPlayer(id);
        }
    }

    private void OnPlayerDisconnected(long id)
    {
        GD.Print($"{id} left");
        Peers.Remove(id);

        if (Multiplayer.IsServer())
        {
            if (Peers.Count <= 1)
            {
                // all clients left -> close game
                GetTree().Quit();
                return;
            }
            else if (id == HostId)
            {
                // host left, assign new host
                HostId = Peers.ElementAt(1);
            }

            if (IsInstanceValid(Game))
            {
                //Game.AddPlayer(id);
                Game.RemovePlayer(id);
            }
        }
    }
    private void OnConnectionFail()
    {
        //Multiplayer.MultiplayerPeer?.Close();
        Multiplayer.MultiplayerPeer = null;

        SceneSwitcher.Instance.SwitchToMainMenu();
    }
    private void OnServerDisconnected()
    {
        Multiplayer.MultiplayerPeer = null;
        SceneSwitcher.Instance.SwitchToMainMenu();
    }

    private void OnFind()
    {
        GetGames();
    }

    private void OnHost(String lobby_name)
    {
        Stream.SendMessage(new HostMessage { LobbyName = lobby_name});
    }

    private void OnJoin(int index)
    {
        GD.Print("On join");
        if (index == -1)
        {
            return;
        }
        GD.Print(Lobbies);
        GD.Print(index);
        ushort port = Lobbies[index].Port;
        //GD.Print(JoinGame(port));
        JoinGame(port);
        Stream.SendMessage(new JoinMessage { Port = port });
    }

    private void OnRefresh()
    {
        GetGames();
    }
}
