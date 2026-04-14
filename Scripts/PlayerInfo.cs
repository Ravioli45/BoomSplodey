using Godot;
using System;

[GlobalClass]
public partial class PlayerInfo : Resource
{
    private string _playerName;
    [Export]
    public string PlayerName
    {
        get => _playerName;
        set
        {
            _playerName = value;
            EmitChanged();
        }
    }

    private int _selectedWeapon;
    [Export]
    public int SelectedWeapon
    {
        get => _selectedWeapon;
        set
        {
            _selectedWeapon = value;
            EmitChanged();
        }
    }

    private int _selectedHat = 0;
    [Export]
    public int SelectedHat
    {
        get => _selectedHat;
        set
        {
            _selectedHat = value;
            EmitChanged();
        }
    }

    private bool _isReady = false;
    [Export]
    public bool IsReady
    {
        get => _isReady;
        set
        {
            _isReady = value;
            EmitChanged();
        }
    }

    private int _damageDealt = 0;
    [Export]
    public int DamageDealt
    {
        get => _damageDealt;
        set
        {
            _damageDealt = value;
            EmitChanged();
        }
    }

    private int _kills = 0;
    [Export]
    public int Kills
    {
        get => _kills;
        set
        {
            _kills = value;
            EmitChanged();
        }
    }

    public PlayerInfo() : this("", 0, false, 0, 0) { }
    public PlayerInfo(string playerName, int selectedWeapon, bool isReady, int damageDealt, int kills)
    {
        _playerName = playerName;
        _selectedWeapon = selectedWeapon;

        _isReady = isReady;

        _damageDealt = damageDealt;
        _kills = kills;
    }

    public override string ToString()
    {
        return $"PlayerInfo[Name: {PlayerName}, Weapon: {SelectedWeapon}, Hat: {SelectedHat}, DamageDealt: {DamageDealt}, Kills: {Kills}]";
    }
}
