using Godot;
using System;
using System.Collections.Generic;

public partial class ServerLobby : Control
{
    public enum ServerLobbyState
    {
        ServerList,
        Create,
        Off,
    }

    [Signal]
    public delegate void JoinedEventHandler(int index);
    [Signal]
    public delegate void RefreshedEventHandler();
    [Signal]
    public delegate void CreatedEventHandler(string serverName);

    [Export]
    private Control ServerListMenu;
    [Export]
    private ItemList ServerList;
    [Export]
    private Control CreateMenu;

    public ServerLobbyState State
    {
        get => field;
        set
        {
            field = value;

            if (IsNodeReady())
                UpdateUI();
        }
    } = ServerLobbyState.ServerList;
    private int SelectedIndex = -1;
    private string ServerName = "";

    public override void _Ready()
    {
        base._Ready();

        UpdateUI();
    }

    public void UpdateServerList(List<string> serverNames)
    {
        foreach (string s in serverNames)
        {
            ServerList.AddItem(s);
        }
    }

    private void UpdateUI()
    {
        switch (State)
        {
            case ServerLobbyState.ServerList:
                Visible = true;
                ProcessMode = ProcessModeEnum.Inherit;
                ServerListMenu.Visible = true;
                ServerListMenu.ProcessMode = ProcessModeEnum.Inherit;
                CreateMenu.Visible = false;
                CreateMenu.ProcessMode = ProcessModeEnum.Disabled;
                break;
            case ServerLobbyState.Create:
                ServerListMenu.Visible = false;
                ServerListMenu.ProcessMode = ProcessModeEnum.Disabled;
                CreateMenu.Visible = true;
                CreateMenu.ProcessMode = ProcessModeEnum.Inherit;
                break;
            case ServerLobbyState.Off:
                Visible = false;
                ProcessMode = ProcessModeEnum.Disabled;
                break;
        }
    }

    private void OnServerSelected(int index)
    {
        SelectedIndex = index;
    }
    private void OnJoinPressed()
    {
        EmitSignalJoined(SelectedIndex);
    }
    private void OnRefreshPressed()
    {
        ServerList.Clear();
        SelectedIndex = -1;
        EmitSignalRefreshed();
    }
    private void OnCreatePressed()
    {
        State = ServerLobbyState.Create;
    }

    private void OnLobbyNameChanged(string newText)
    {
        ServerName = newText;
    }
    private void OnCreateBackPressed()
    {
        State = ServerLobbyState.ServerList;
    }
    private void OnCreateOkPressed()
    {
        EmitSignalCreated(ServerName);
    }
}
