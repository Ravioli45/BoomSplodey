using Godot;
using System;

public partial class PlayerDisplay : Control
{
    [Signal]
    public delegate void ReadyToggledEventHandler();

    [Export]
    public long OwnerId
    {
        get => field;
        set
        {
            field = value;

            if (IsNodeReady())
            {
                SetUIForOwner();
            }
        }
    } = 1;
    [Export]
    public PlayerInfo Info;

    [ExportSubgroup("MenuElements")]
    [Export]
    private LineEdit PlayerNameEdit;
    [Export]
    private CheckBox ReadyCheckBox;
    [Export]
    private OptionButton WeaponDropdown;
    [Export]
    private OptionButton HatDropdown;

    public override void _Ready()
    {
        base._Ready();

        SetUIForOwner();

        Info.Changed += UpdateMenuValues;
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        Info.Changed -= UpdateMenuValues;
    }

    private void UpdateMenuValues()
    {
        PlayerNameEdit.Text = Info.PlayerName;
        PlayerNameEdit.CaretColumn = Info.PlayerName.Length;

        ReadyCheckBox.SetPressedNoSignal(Info.IsReady);

        WeaponDropdown.Selected = Info.SelectedWeapon;
        HatDropdown.Selected = Info.SelectedHat;
    }

    private void SetUIForOwner()
    {
        if (OwnerId == Multiplayer.GetUniqueId())
        {
            PlayerNameEdit.Editable = true;
            ReadyCheckBox.Disabled = false;
            WeaponDropdown.Disabled = false;
            HatDropdown.Disabled = false;
        }
        else
        {
            PlayerNameEdit.Editable = false;
            ReadyCheckBox.Disabled = true;
            WeaponDropdown.Disabled = true;
            HatDropdown.Disabled = true;
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void ChangePlayerName(string text)
    {
        if (!Multiplayer.IsServer())
        {
            GD.PushWarning("This code should only be reached by server");
            return;
        }

        if (OwnerId == Multiplayer.GetRemoteSenderId())
        {
            Info.PlayerName = text;
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void ChangeReady(bool toggled_on)
    {
        if (!Multiplayer.IsServer())
        {
            GD.PushWarning("This code should only be reached by server");
            return;
        }

        if (OwnerId == Multiplayer.GetRemoteSenderId())
        {
            Info.IsReady = toggled_on;
            EmitSignalReadyToggled();
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void ChangeWeapon(int index)
    {
        if (!Multiplayer.IsServer())
        {
            GD.PushWarning("This code should only be reached by server");
            return;
        }

        if (OwnerId == Multiplayer.GetRemoteSenderId())
        {
            Info.SelectedWeapon = index;
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void ChangeHat(int index)
    {
        if (!Multiplayer.IsServer())
        {
            GD.PushWarning("This code should only be reached by server");
            return;
        }

        if (OwnerId == Multiplayer.GetRemoteSenderId())
        {
            Info.SelectedHat = index;
        }
    }

    private void OnPlayerNameEdited(string text)
    {
        if (OwnerId == Multiplayer.GetUniqueId())
        {
            RpcId(1, MethodName.ChangePlayerName, text);
        }
        else
        {
            PlayerNameEdit.Text = Info.PlayerName;
            PlayerNameEdit.CaretColumn = PlayerNameEdit.Text.Length;
        }
    }
    private void OnReadyToggled(bool toggled_on)
    {
        if (OwnerId == Multiplayer.GetUniqueId())
        {
            RpcId(1, MethodName.ChangeReady, toggled_on);
        }
        else
        {
            //ReadyCheckBox.SetPressedNoSignal(!toggled_on);
            ReadyCheckBox.SetPressedNoSignal(Info.IsReady);
        }
    }

    private void OnWeaponSelected(int index)
    {
        if (OwnerId == Multiplayer.GetUniqueId())
        {
            RpcId(1, MethodName.ChangeWeapon, index);
        }
        else
        {
            WeaponDropdown.Selected = Info.SelectedWeapon;
        }
    }

    private void OnHatSelected(int index)
    {
        if (OwnerId == Multiplayer.GetUniqueId())
        {
            RpcId(1, MethodName.ChangeHat, index);
        }
        else
        {
            HatDropdown.Selected = Info.SelectedHat;
        }
    }
}
