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

    public PlayerInfo() : this("", 0, false) { }
    public PlayerInfo(string playerName, int selectedWeapon, bool isReady)
    {
        _playerName = playerName;
        _selectedWeapon = selectedWeapon;

        _isReady = isReady;
    }

    public override string ToString()
    {
        return $"PlayerInfo[Name: {PlayerName}, Weapon: {SelectedWeapon}, Hat: {SelectedHat}]";
    }
}
