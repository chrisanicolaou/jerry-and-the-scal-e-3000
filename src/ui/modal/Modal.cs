using Godot;
using System;
using System.Threading.Tasks;

public class Modal : Control
{
    [Signal] public delegate void ModalCloseRequested();
    [Export] private float _continueDelay = 2;
    [Export] private NodePath _iconSpritePath;
    [Export] private NodePath _titleLabelPath;
    [Export] private NodePath _descriptionLabelPath;
    [Export] private NodePath _continueLabelPath;

    private Sprite _icon;
    private Label _titleLabel;
    private Label _descriptionLabel;
    private Label _continueLabel;
    private bool _shouldReceiveInput;

    public override void _Ready()
    {
        _icon = GetNode<Sprite>(_iconSpritePath);
        _titleLabel = GetNode<Label>(_titleLabelPath);
        _descriptionLabel = GetNode<Label>(_descriptionLabelPath);
        _continueLabel = GetNode<Label>(_continueLabelPath);
    }

    public async Task Show(ModalOptions opts)
    {
        _shouldReceiveInput = false;
        var continueDelay = opts.ContinueDelayDuration < 0 ? _continueDelay : opts.ContinueDelayDuration;
        _icon.Texture = opts.IconTex;
        _titleLabel.Text = opts.Title;
        _descriptionLabel.Text = opts.Description;
        Show();
        _continueLabel.Hide();
        await ToSignal(GetTree().CreateTimer(continueDelay), "timeout");
        _continueLabel.Show();
        _shouldReceiveInput = true;
        await ToSignal(this, nameof(ModalCloseRequested));
        _shouldReceiveInput = false;
        Hide();
    }

    public override void _Input(InputEvent @event)
    {
        if (!_shouldReceiveInput) return;

        if (@event is InputEventKey)
        {
            EmitSignal(nameof(ModalCloseRequested));
            GetTree().SetInputAsHandled();
        }
    }
}
