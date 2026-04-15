using Godot;
using System;

public partial class SceneSwitcher : Node
{
    [Export(PropertyHint.FilePath)]
    private string MainMenuScenePath;
    [Export(PropertyHint.FilePath)]
    private string WANLobbyScenePath;

    public static SceneSwitcher Instance { get; set; }

    public override void _Ready()
    {
        base._Ready();

        if (Instance != null)
        {
            GD.PushWarning("two scene switchers detected");
            return;
        }

        Instance ??= this;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);

        if (@event is InputEventKey keyEvent && keyEvent.Keycode == Key.Escape)
        {
            SceneSwitcher.Instance.SwitchToMainMenu();
        }
    }

    public void SwitchToMainMenu()
    {
        Multiplayer.MultiplayerPeer = null;
        GetTree().ChangeSceneToFile(MainMenuScenePath);
    }
    public void SwitchToLobby()
    {
        GetTree().ChangeSceneToFile(WANLobbyScenePath);
    }
}
