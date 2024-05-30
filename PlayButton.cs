using Godot;
using System;

public partial class PlayButton : Button
{
    [Export] public Texture2D IconPlay;
    [Export] public Texture2D IconPause;

    private bool _isPlaying;
    public bool IsPlaying
    {
        get => _isPlaying;
        set
        {
            _isPlaying = value;
            Icon = IsPlaying ? IconPause : IconPlay;
        }
    }
}
