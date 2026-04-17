using Godot;
using System;
using System.Collections.Generic;

public partial class AudioManager : Node
{

    public static AudioManager Instance { get; private set; }

    private AudioStreamPlayer BGMPlayer;

    private Godot.Collections.Array<AudioStreamPlayer> available = [];
    private Godot.Collections.Array<StringName> queue = [];

    [ExportSubgroup("BGM")]
    [Export] public Godot.Collections.Dictionary<StringName, AudioStream> bgm_list = [];

    [ExportSubgroup("SFX")]
    [Export] private int streams = 1;

    [Export] public Godot.Collections.Dictionary<StringName, AudioStream> sfx_list = [];

    public override void _Ready()
    {
        if (Instance != null)
        {
            GD.PushWarning("More than one audio manager detected");
            return;
        }

        for (int i = 0; i < streams; i++)
        {
            AudioStreamPlayer player = new();
            player.Name = $"SFX {i}";

            AddChild(player);
            available.Add(player);
            player.Finished += () => OnStreamFinished(player);
        }

        BGMPlayer = new AudioStreamPlayer();
        BGMPlayer.Name = "BGM";
        BGMPlayer.Bus = "BGM";
        AddChild(BGMPlayer);

        Instance ??= this;
    }

    public override void _Process(double delta)
    {

        // if there is an avaiable audio player and a sound to play
        if (available.Count != 0 && queue.Count != 0)
        {
            StringName sfx_name = queue[0];
            queue.RemoveAt(0);
            AudioStreamPlayer player = available[0];
            available.RemoveAt(0);

            // SAFETY: PlaySFX guaranteed that sfx_name is valid key
            // so this should never throw KeyNotFound
            player.Stream = sfx_list[sfx_name];
            player.Play();
        }
    }

    public void PlayBGM(StringName bgm_name)
    {
        if (bgm_list.TryGetValue(bgm_name, out AudioStream audio))
        {
            BGMPlayer.Stop();
            BGMPlayer.Stream = audio;
            BGMPlayer.Play();
        }
        else
        {
            throw new KeyNotFoundException($"'{bgm_name}' is not recognized as a bgm name");
        }
    }

    public void PlaySFX(StringName sfx_name)
    {
        if (sfx_list.ContainsKey(sfx_name))
        {
            queue.Add(sfx_name);
        }
        else
        {
            throw new KeyNotFoundException($"'{sfx_name}' is not recognized as a sound effect name");
        }
    }

    private void OnStreamFinished(AudioStreamPlayer player)
    {
        available.Add(player);
    }
}
