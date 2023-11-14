using Godot;
using System;

public class LevelSelectNode : Button
{
    [Signal] public delegate void LevelNodeSelected();
    
    [Export] private NodePath _numberLabelPath;
    [Export] private NodePath _nameLabelPath;
    [Export] private NodePath _disabledControlPath;
    [Export] private Color _disabledModulate;

    private Label _numberLabel;
    private Label _nameLabel;
    private Control _disabledControl;

    public override void _Ready()
    {
        _numberLabel = GetNode<Label>(_numberLabelPath);
        _nameLabel = GetNode<Label>(_nameLabelPath);
        _disabledControl = GetNode<Control>(_disabledControlPath);
        Connect("pressed", this, nameof(OnLevelSelected));
    }

    public void Initialize(LevelData levelData, int levelIndex, bool disabled = false)
    {
        if (!disabled)
        {
            _numberLabel.Text = levelIndex.ToString();
            _nameLabel.Text = levelData.DisplayName;
            _disabledControl.Hide();
        }
        else
        {
            _numberLabel.Hide();
            _nameLabel.Hide();
            Disabled = true;
            _disabledControl.Show();
            Modulate = _disabledModulate;
        }
    }

    public void OnLevelSelected() => EmitSignal(nameof(LevelNodeSelected));
}
