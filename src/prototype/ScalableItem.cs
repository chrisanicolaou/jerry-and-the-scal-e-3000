using Godot;
using System;
using GithubGameJam2023.player.player_gun;

public class ScalableItem : Node2D
{
    [Export] private Vector2 _bigScale;
    [Export] private Vector2 _smallScale;
    [Export] private Texture _smallTex;
    [Export] private Texture _bigTex;
    [Export] private Vector2 _smallOffset;
    [Export] private bool _startsBig;
    [Export] private NodePath _spritePath;
    [Export] private NodePath _scalableChildPath;
    protected Sprite Sprite { get; private set; }
    protected Node2D ScalableChild { get; private set; }
    
    protected bool IsBig { get; set; }

    public override void _Ready()
    {
        Sprite = GetNode<Sprite>(_spritePath);
        ScalableChild = GetNode<Node2D>(_scalableChildPath);
        (Sprite.Material as ShaderMaterial).SetShaderParam("enabled", true);
        AddToGroup("scalable");
        IsBig = _startsBig;
    }

    public void OnBulletCollide(BulletType type)
    {
        if (type == BulletType.ShootBig && !IsBig)
        {
            ScaleUp();
            IsBig = true;
            return;
        }
        if (type == BulletType.ShootSmall && IsBig)
        {
            ScaleDown();
            IsBig = false;
        }
    }

    protected virtual void ScaleUp()
    {
        Sprite.Texture = _bigTex;
        ScalableChild.Scale = _bigScale;
        Position -= _smallOffset;
    }

    protected virtual void ScaleDown()
    {
        Sprite.Texture = _smallTex;
        ScalableChild.Scale = _smallScale;
        Position += _smallOffset;
    }
}











// [Export] private NodePath _interactionAreaPath;
// protected InteractionArea InteractionArea { get; private set; }

// InteractionArea = GetNode<InteractionArea>(_interactionAreaPath);
// InteractionArea.Connect(nameof(InteractionArea.InteractionAreaActivated), this, nameof(OnInteractionAreaActivated));
// InteractionArea.Connect(nameof(InteractionArea.InteractionAreaDeactivated), this, nameof(OnInteractionAreaDeactivated));


// public virtual void OnInteractionAreaActivated()
// {
//     (Sprite.Material as ShaderMaterial).SetShaderParam("enabled", true);
// }
//
// public virtual void OnInteractionAreaDeactivated()
// {
//     (Sprite.Material as ShaderMaterial).SetShaderParam("enabled", false);
// }