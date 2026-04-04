using Godot;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
public class ServerInfo
{
    public string IP { get; set; }
    public string LobbyName { get; set; }

    public ServerInfo() : this("", "") { }
    public ServerInfo(string ip, string lobbyname)
    {
        IP = ip;
        LobbyName = lobbyname;
    }

    public override string ToString()
    {
        return $"ServerInfo{{{IP}:{LobbyName}}}";
    }
}
public partial class LANLobby : Node
{

    [Signal]
    public delegate void PlayerConnectedEventHandler(long id);
    [Signal]
    public delegate void PlayerDisconnectedEventHandler(long id);
    [Signal]
    public delegate void ServerDisconnectedEventHandler();
    [Signal]
    public delegate void OnServerListUpdateEventHandler();

    public enum LobbyState
    {
        Offline,
        Host,
        Client,
    }

    [Export]
    public int MaxConnections { get; private set; }

    [Export]
    public int ServerPort { get; private set; }

    [Export]
    private int LANListenerPort;

    [Export]
    private int LANBroadcasterLowerPort;

    [Export]
    private int LANBroadcasterUpperPort;

    [Export]
    private ServerLobby ServerLobby;

    [Export(PropertyHint.FilePath)]
    private string GameScenePath;

    [Export]
    private GameRoot Game;
    private PacketPeerUdp SearchConnection;
    private LobbyState State = LobbyState.Offline;
    public List<ServerInfo> ServerList { get; private set; } = [];
    //public PlayerInfo MyInfo = new("");
    public string LobbyName { get; private set; } = "";
    //public SortedDictionary<long, PlayerInfo> PeerInfo { get; private set; } = [];
    public SortedSet<long> Peers { get; private set; } = [];

    //public static LANLobby Instance { get; private set; }

    public override void _EnterTree()
    {
        base._EnterTree();

        //Instance ??= this;
    }
    public override void _Ready()
    {
        base._Ready();
        SearchConnection = null;

        // emitted to every old peer when new peer joins
        // and to new peer multiple times
        Multiplayer.PeerConnected += OnPlayerConnected;

        // emitted on remaining peers when one disconnects
        Multiplayer.PeerDisconnected += OnPlayerDisconnected;

        // emitted on peer after successfully connecting to server ?
        Multiplayer.ConnectedToServer += OnConnectOk;

        // emitted on peer after failing to connect to server ?
        Multiplayer.ConnectionFailed += OnConnectionFail;

        // emitted on server disconnecting ?
        Multiplayer.ServerDisconnected += OnServerDisconnected;

        SetUpLANBroadcaster();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (SearchConnection?.GetAvailablePacketCount() > 0)
        {

            byte[] packet = SearchConnection.GetPacket();
            string ip = SearchConnection.GetPacketIP();
            int port = SearchConnection.GetPacketPort();

            string data = packet.GetStringFromAscii();

            //GD.Print("Message from: " + ip + ":" + port);

            //LANMessage message = JsonSerializer.Deserialize<LANMessage>(data);
            LANMessage message = null;

            try
            {
                message = JsonSerializer.Deserialize<LANMessage>(data);
            }
            catch
            {
                return;
            }


            if (message.GameID == LANMessageBuilder.GameID &&
                State == LobbyState.Host &&
                message is SearchMessage searchMessage)
            {
                //GD.Print(searchMessage);

                SearchConnection.SetDestAddress(ip, port);
                FoundMessage found_response = LANMessageBuilder.BuildFoundMessage(LobbyName);
                SendMessage(found_response);
            }
            else if (message.GameID == LANMessageBuilder.GameID &&
                State == LobbyState.Offline &&
                message is FoundMessage foundMessage)
            {
                //GD.Print(foundMessage);
                ServerList.Add(new ServerInfo(ip, foundMessage.LobbyName));
                //EmitSignalOnServerListUpdate();
                ServerLobby.UpdateServerList(ServerList.Select(element => element.LobbyName).ToList());
            }
        }
    }

    // listeners are used to listen for queries from broadcasters
    public Error SetUpLANListener()
    {
        SearchConnection = new PacketPeerUdp();

        Error error = SearchConnection.Bind(LANListenerPort);
        //GD.Print($"listening on {LANListenerPort}");

        if (error != Error.Ok)
        {
            GD.PushError(error);
            return error;
        }

        //GD.Print(Multiplayer.GetUniqueId() + ": " + "listening on port " + SearchConnection.GetLocalPort());

        SearchConnection.SetBroadcastEnabled(true);

        return Error.Ok;
    }


    // broadcaster are used to query for game servers
    public Error SetUpLANBroadcaster()
    {
        //GD.Print("h");
        SearchConnection = new PacketPeerUdp();


        SearchConnection.SetBroadcastEnabled(true);
        SearchConnection.SetDestAddress("255.255.255.255", LANListenerPort);

        for (int i = LANBroadcasterLowerPort; i <= LANBroadcasterUpperPort; i++)
        {
            Error error = SearchConnection.Bind(i);

            if (error == Error.Ok)
            {
                //GD.Print("broadcaster on port: " + SearchConnection.GetLocalPort());
                return error;
            }
        }

        return Error.CantCreate;
    }

    public void CloseSearchConnection()
    {
        //GD.Print("c");
        //Multiplayer.MultiplayerPeer?.RefuseNewConnections = true;
        SearchConnection.Close();
        SearchConnection = null;
    }

    public Error HostGame()
    {
        ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
        //GD.Print($"hosting on {ServerPort}");
        Error error = peer.CreateServer(ServerPort, MaxConnections);

        if (error != Error.Ok)
        {
            GD.PushError(error);
            return error;
        }

        ServerLobby.State = ServerLobby.ServerLobbyState.Off;

        Multiplayer.MultiplayerPeer = peer;
        State = LobbyState.Host;

        CloseSearchConnection();

        SetUpLANListener();

        //PeerInfo[1] = MyInfo;
        Peers.Add(1);

        SpawnGame();
        //RpcId(1, MethodName.SpawnGame);

        //GD.Print("hosting on port: " + ServerPort);
        return Error.Ok;
    }

    public Error JoinGame(string address, int port)
    {
        //GD.Print($"trying to join {address}:{port}");
        ENetMultiplayerPeer peer = new ENetMultiplayerPeer();

        Error error = peer.CreateClient(address, port);

        if (error != Error.Ok)
        {
            GD.PushError(error);
            return error;
        }

        ServerLobby.State = ServerLobby.ServerLobbyState.Off;
        //GD.Print("joined OK");
        Multiplayer.MultiplayerPeer = peer;
        State = LobbyState.Client;

        // the LAN search connection is no longer needed at this point
        CloseSearchConnection();

        return Error.Ok;
    }

    //[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SpawnGame()
    {
        if (Multiplayer.IsServer())
        {
            //GD.Print("start game");
            //GD.Print(GameSpawner.SpawnPath);
            //PackedScene level = GD.Load<PackedScene>(GameSpawner._SpawnableScenes[0]);
            PackedScene gameScene = GD.Load<PackedScene>(GameScenePath);

            //Node node = level.Instantiate();
            GameRoot game = gameScene.Instantiate<GameRoot>();
            // connected using Connect method so that it is automatically disconnected 
            // whenever gameroot is freed
            game.Connect(GameRoot.SignalName.GameStarted, Callable.From(OnGameStart));
            //GameSpawner.GetNode(GameSpawner.SpawnPath).AddChild(node, true);
            AddChild(game, true);
            Game = game;
            Game.AddPlayer(1);
        }
        else
        {
            GD.PushError("only server can spawn the game");
        }
    }

    public void SearchForLAN()
    {
        ServerList.Clear();
        SearchMessage message = LANMessageBuilder.BuildSearchMessage();
        SendMessage(message);
    }

    public void SendMessage(LANMessage message)
    {
        //GD.Print(message);
        string data = JsonSerializer.Serialize(message);
        byte[] packet = data.ToAsciiBuffer();

        if (SearchConnection == null)
        {
            GD.PushError("Initialize UPD before sending a message");
            return;
        }

        SearchConnection.PutPacket(packet);
    }

    // when peer connects, send my info
    private void OnPlayerConnected(long id)
    {
        //GD.Print($"{id} connected to {Multiplayer.GetUniqueId()}");
        //string data = JsonSerializer.Serialize(MyInfo);
        //RpcId(id, MethodName.RegisterPlayer, data);
        if (Multiplayer.IsServer() && IsInstanceValid(Game))
        {
            Game.AddPlayer(id);
        }

        RpcId(id, MethodName.RegisterPlayer);
    }


    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RegisterPlayer()
    {
        //GD.Print($"{Multiplayer.GetUniqueId()}:{Multiplayer.GetRemoteSenderId()}");
        //PlayerInfo newInfo = JsonSerializer.Deserialize<PlayerInfo>(newInfoJson);
        //GD.Print(newInfo);
        long newId = Multiplayer.GetRemoteSenderId();
        //PeerInfo[newId] = newInfo;
        Peers.Add(newId);

        EmitSignalPlayerConnected(newId);
    }

    private void OnPlayerDisconnected(long id)
    {
        if (Multiplayer.IsServer() && IsInstanceValid(Game))
        {
            Game.RemovePlayer(id);
        }
        //PeerInfo.Remove(id);
        Peers.Remove(id);

        EmitSignalPlayerDisconnected(id);
    }

    private void OnConnectOk()
    {
        int myPeerId = Multiplayer.GetUniqueId();
        //PeerInfo[myPeerId] = MyInfo;
        Peers.Add(myPeerId);

        // TODO: maybe have a custom signal here
    }

    private void OnConnectionFail()
    {
        Multiplayer.MultiplayerPeer = null;

        // TODO: maybe have a custom signal here
    }

    private void OnServerDisconnected()
    {
        Multiplayer.MultiplayerPeer = null;
        //PeerInfo.Clear();
        Peers.Clear();

        EmitSignalServerDisconnected();
    }

    private void OnHost(string lobbyName)
    {
        LobbyName = lobbyName;
        HostGame();
    }

    private void OnFind()
    {
        SetUpLANBroadcaster();
        SearchForLAN();
    }

    private void OnCancelFind()
    {
        GD.Print("closing search on find cancel");
        CloseSearchConnection();
    }

    private void OnRefresh()
    {
        if (SearchConnection != null)
            SearchForLAN();
        else
        {
            GD.PushWarning("search connection should not be null here");
        }
    }

    private void OnJoin(int index)
    {
        if (index == -1)
        {
            return;
        }
        string ip = ServerList[index].IP;
        JoinGame(ip, ServerPort);
    }

    private void OnGameStart()
    {
        GD.Print("closing search connection on game start");
        CloseSearchConnection();
    }
}
