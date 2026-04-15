using Godot;
using System;

public partial class MainMenu : Control
{
    public override void _Ready()
    {
        base._Ready();
        Multiplayer.MultiplayerPeer = null;

        if (DisplayServer.GetName() == "headless")
        {
            // being run as child process of master server
            SceneSwitcher.Instance.SwitchToLobby();
        }
    }
    private void OnPlayPressed()
    {
        // switch to main scene
        SceneSwitcher.Instance.SwitchToLobby();
    }
    private void OnQuitPressed()
    {
        GetTree().Quit();
    }
}
