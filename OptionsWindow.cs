using Godot;
using System;

public partial class OptionsWindow : PanelContainer
{
    private Control _titlebar;
    private Control _content;
    private Button _toggleButton;
    private bool _isExpanded;
    [Export] public Vector2 ExpandedSize = new Vector2(200.0f, 0.0f);

    public bool ShowNames;
    public bool ShowStands;

    private CheckBox _showNames;
    private CheckBox _showStands;

    public override void _Ready()
    {
        _titlebar = (Control)GetNode("%TitleBar");
        _content = (Control)GetNode("%Content");
        _toggleButton = (Button)GetNode("%TitleBar");
        GD.Print($"title   size {_titlebar.Size}");
        GD.Print($"content size {_content.Size}");

        _toggleButton.Connect("pressed", new Callable(this, nameof(OnToggleButtonPressed)));

        _showNames = GetNode<CheckBox>("%ShowNames");
        _showNames.Connect("toggled", new Callable(this, nameof(OnShowNamesToggled)));
        ShowNames = _showNames.ButtonPressed;

        _showStands = GetNode<CheckBox>("%ShowStands");
        _showStands.Connect("toggled", new Callable(this, nameof(OnShowStandsToggled)));
        ShowStands = _showStands.ButtonPressed;
    }

    private void OnToggleButtonPressed()
    {
        _isExpanded = !_isExpanded;
        _content.Visible = _isExpanded;
        _content.Visible = _isExpanded;
        _titlebar.CustomMinimumSize = _isExpanded ? ExpandedSize : Vector2.Zero;
    }

    public void OnShowNamesToggled(bool state)
    {
        ShowNames = state;
    }

    public void OnShowStandsToggled(bool state)
    {
        ShowStands = state;
    }
}
