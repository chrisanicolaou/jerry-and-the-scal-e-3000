using Godot;
using System;
using ChiciStudios.GithubGameJam2023.Common.Audio;

public class Key : RigidBody2D
{
    [Signal] public delegate void KeyFound();

    [Export] protected AudioStream PickupSfx { get; set; }
    [Export] private NodePath _interactionAreaPath;
    protected InteractionArea InteractionArea { get; private set; }
    protected AudioManager AudioManager { get; set; }

    public override void _Ready()
    {
        AudioManager = GetNode<AudioManager>("/root/AudioManager");
        InteractionArea = GetNode<InteractionArea>(_interactionAreaPath);
        InteractionArea.Connect(nameof(InteractionArea.InteractionAreaActivated), this, nameof(OnInteractionAreaActivated));
    }

    public virtual void OnInteractionAreaActivated()
    {
        EmitSignal(nameof(KeyFound));
        AudioManager.PlaySfx(PickupSfx);
    }
}
