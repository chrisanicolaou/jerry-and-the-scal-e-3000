using Godot;
using System;
using System.Threading.Tasks;
using GithubGameJam2023.player.player_gun;

public class ScalableItem : Node2D
{
    [Export(PropertyHint.Range, "2,10,0.1")] protected float ScaleDuration { get; private set; } = 2f;
    [Export] private ScalableItemInfo _itemScaleInfo;
    [Export] private NodePath _spritePath;
    [Export] private NodePath _scalableChildPath;
    private Texture _normalTex;
    private Vector2 _normalScale;
    protected Sprite Sprite { get; private set; }
    protected Node2D ScalableChild { get; private set; }
    protected bool IsMutated { get; set; }

    public override void _Ready()
    {
        Sprite = GetNode<Sprite>(_spritePath);
        ScalableChild = GetNode<Node2D>(_scalableChildPath);
        (Sprite.Material as ShaderMaterial)?.SetShaderParam("enabled", true);
        AddToGroup("scalable");
        _normalScale = ScalableChild.Scale;
        _normalTex = Sprite.Texture;
    }

    public void OnBulletCollide(ScaleType type)
    {
        if (IsMutated) return;
        
        if (type == ScaleType.Big)
        {
            IsMutated = true;
            ScaleUp();
            return;
        }
        if (type == ScaleType.Small)
        {
            IsMutated = true;
            ScaleDown();
        }
    }

    protected virtual void ScaleUp()
    {
        HandleScale(ScaleType.Big);
    }

    protected virtual void ScaleDown()
    {
        HandleScale(ScaleType.Small);
    }

    protected virtual async void HandleScale(ScaleType type)
    {
        var info = _itemScaleInfo.GetScaleInfo(type);
        await PlayPreScaleAnimation(type == ScaleType.Big);
        Sprite.Texture = info.Tex;
        ScalableChild.Scale = info.Scale;
        Position += info.Offset;
        await ToSignal(GetTree().CreateTimer(ScaleDuration, false), "timeout");
        IsMutated = false;
        ScalableChild.Scale = _normalScale;
        Sprite.Texture = _normalTex;
        Position -= info.Offset;
    }

    protected virtual async Task PlayPreScaleAnimation(bool invertScaleFactor = false)
    {
        var duration = 0.5f;
        var rumbleFrequency = 5;
        var rumbleRotationDegrees = 10f;
        var scaleFactor = 0.2f * (invertScaleFactor ? -1 : 1);
        var postTweenDelay = 0.25f;

        var originalRotation = RotationDegrees;
        var originalScale = Scale;
        var finalScale = Scale + (Scale * scaleFactor);
        var durationPerRumble = duration / rumbleFrequency;
        var tween = GetTree().CreateTween();
        GetTree().CreateTween().TweenProperty(this, "scale", finalScale, duration);
        for (var i = 0; i < rumbleFrequency; i++)
        {
            var rotationDegrees = rumbleRotationDegrees * (i % 2 == 0 ? 1 : -1);
            tween.TweenProperty(this, "rotation_degrees", rotationDegrees, durationPerRumble / 2);
            tween.TweenProperty(this, "rotation_degrees", originalRotation, durationPerRumble / 2);
        }

        await ToSignal(tween, "finished");
        await ToSignal(GetTree().CreateTimer(postTweenDelay, false), "timeout");
        Scale = originalScale;
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