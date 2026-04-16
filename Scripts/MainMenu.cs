using Godot;
using System;

public partial class MainMenu : Control
{
    [Export]
    private Control Main;
    [Export]
    private Control Options;
    [Export]
    private LineEdit IpEdit;


    public override void _Ready()
    {
        base._Ready();
        Multiplayer.MultiplayerPeer = null;

        if (DisplayServer.GetName() == "headless")
        {
            // being run as child process of master server
            SceneSwitcher.Instance.SwitchToLobby();
        }
        //GD.Print(GlobalResources.Instance.CustomIP == null);
        IpEdit.Text = GlobalResources.Instance.CustomIp ?? "";
        IpEdit.CaretColumn = IpEdit.Text.Length;
    }
    private void OnPlayPressed()
    {
        // switch to main scene
        SceneSwitcher.Instance.SwitchToLobby();
    }
    private void OnOptionsPressed()
    {
        Main.Visible = false;
        Options.Visible = true;
    }
    private void OnQuitPressed()
    {
        GetTree().Quit();
    }

    private void OnOptionsOkPressed()
    {
        if (IpEdit.Text.StripEdges() != "")
        {
            GlobalResources.Instance.CustomIp = IpEdit.Text.StripEdges();
        }
        else
        {
            GlobalResources.Instance.CustomIp = null;
        }
    }
    private void OnOptionsBackPressed()
    {
        Main.Visible = true;
        Options.Visible = false;
    }
}
