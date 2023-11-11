using Godot;
using System;
using System.Threading.Tasks;

public class PreLevel : Control
{
    [Export(PropertyHint.Range, "1,5,0.5")] private float _holdDuration = 2.5f;
    [Export] private NodePath _levelNumLabelPath;
    [Export] private NodePath _levelNameLabelPath;

    private Label _levelNumLabel;
    private Label _levelNameLabel;

    public override void _Ready()
    {
        _levelNameLabel = GetNode<Label>(_levelNameLabelPath);
        _levelNumLabel = GetNode<Label>(_levelNumLabelPath);
    }

    public void SetLevelLabels(int num, string name)
    {
        _levelNumLabel.Text = num.ToString();
        _levelNameLabel.Text = name;
    }

    public async Task HoldForDuration()
    {
        await ToSignal(GetTree().CreateTimer(_holdDuration), "timeout");
    }
}
