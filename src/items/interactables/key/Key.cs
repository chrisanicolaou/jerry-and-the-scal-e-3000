using Godot;
using System;
using ChiciStudios.GithubGameJam2023.Common.Audio;

public class Key : RigidBody2D
{
    [Signal] public delegate void KeyFound();

    [Export] private AudioStream _pickupSfx;
    [Export] private NodePath _interactionAreaPath;
    protected InteractionArea InteractionArea { get; private set; }
    private AudioManager _audioManager;

    public override void _Ready()
    {
        _audioManager = GetNode<AudioManager>("/root/AudioManager");
        InteractionArea = GetNode<InteractionArea>(_interactionAreaPath);
        InteractionArea.Connect(nameof(InteractionArea.InteractionAreaActivated), this, nameof(OnInteractionAreaActivated));
    }

    public virtual void OnInteractionAreaActivated()
    {
        EmitSignal(nameof(KeyFound));
        _audioManager.PlaySfx(_pickupSfx);
    }
}
