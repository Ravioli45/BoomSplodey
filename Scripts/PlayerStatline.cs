using Godot;
using System;

public partial class PlayerStatline : HBoxContainer
{
    [Export]
    public string PlayerName
    {
        get => field;
        set
        {
            field = value;

            if (IsNodeReady()) UpdateName();
        }
    } = "";
    [Export]
    private Label PlayerNameLabel;

    [Export]
    public int Kills
    {
        get => field;
        set
        {
            field = value;

            if (IsNodeReady()) UpdateKills();
        }
    } = 0;
    [Export]
    private Label KillsLabel;

    [Export]
    public int DamageDealt
    {
        get => field;
        set
        {
            field = value;

            if (IsNodeReady()) UpdateDamageDealt();
        }
    } = 0;
    [Export]
    private Label DamageDealtLabel;

    public override void _Ready()
    {
        base._Ready();

        UpdateName();
        UpdateKills();
        UpdateDamageDealt();
    }

    private void UpdateName()
    {
        PlayerNameLabel.Text = PlayerName;
    }
    private void UpdateKills()
    {
        KillsLabel.Text = $"{Kills}";
    }
    private void UpdateDamageDealt()
    {
        DamageDealtLabel.Text = $"{DamageDealt}";
    }
}
