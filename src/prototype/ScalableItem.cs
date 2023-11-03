using Godot;
using System;

public class ScalableItem : Node2D
{
    [Export] private NodePath _spritePath;
    [Export] private NodePath _interactionAreaPath;
    protected Sprite Sprite { get; private set; }
    protected InteractionArea InteractionArea { get; private set; }

    public override void _Ready()
    {
        Sprite = GetNode<Sprite>(_spritePath);
        InteractionArea = GetNode<InteractionArea>(_interactionAreaPath);
        InteractionArea.Connect(nameof(InteractionArea.InteractionAreaActivated), this, nameof(OnInteractionAreaActivated));
        InteractionArea.Connect(nameof(InteractionArea.InteractionAreaDeactivated), this, nameof(OnInteractionAreaDeactivated));
        (Sprite.Material as ShaderMaterial).SetShaderParam("enabled", false);
    }

    public virtual void OnInteractionAreaActivated()
    {
        (Sprite.Material as ShaderMaterial).SetShaderParam("enabled", true);
    }

    public virtual void OnInteractionAreaDeactivated()
    {
        (Sprite.Material as ShaderMaterial).SetShaderParam("enabled", false);
    }
}
