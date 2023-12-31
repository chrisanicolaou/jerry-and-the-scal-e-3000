using Godot;
using System;
using System.Threading.Tasks;
using ChiciStudios.GithubGameJam2023.Common.Audio;

public class Modal : Control
{
    [Signal] public delegate void ModalCloseRequested();
    [Export] private float _continueDelay = 2;
    [Export] private NodePath _iconSpritePath;
    [Export] private NodePath _titleLabelPath;
    [Export] private NodePath _descriptionLabelPath;
    [Export] private NodePath _continueLabelPath;
    [Export] private AudioStream _openSfx;
    [Export] private AudioStream _closeSfx;

    private AudioManager _audioManager;
    private Sprite _icon;
    private Label _titleLabel;
    private Label _descriptionLabel;
    private Label _continueLabel;
    private bool _shouldReceiveInput;

    public override void _Ready()
    {
        _audioManager = GetNode<AudioManager>("/root/AudioManager");
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
        _icon.Scale = new Vector2(opts.IconScale, opts.IconScale);
        _titleLabel.Text = opts.Title;
        _descriptionLabel.Text = opts.Description;
        _audioManager.PlaySfx(_openSfx);
        Show();
        _continueLabel.Hide();
        await ToSignal(GetTree().CreateTimer(continueDelay), "timeout");
        _continueLabel.Show();
        _shouldReceiveInput = true;
        await ToSignal(this, nameof(ModalCloseRequested));
        _shouldReceiveInput = false;
        Hide();
        _audioManager.PlaySfx(_closeSfx);
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
