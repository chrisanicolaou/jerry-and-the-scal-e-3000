using Godot;
using System;
using System.Threading.Tasks;
using GithubGameJam2023.player.player_gun;

public class ScalableItemV2 : RigidBody2D
{
    [Export] private NodePath _spritePath;
    [Export] private NodePath _animPlayerPath;
    [Export] private ShaderMaterial _whiteShaderMat;
    
    [Export(PropertyHint.Range, "2,10,0.1")] protected float DefaultScaleDuration { get; private set; } = 2f;

    private Sprite _sprite;
    private AnimationPlayer _animPlayer;
    
    protected bool IsMutated { get; set; }

    public override void _Ready()
    {
        _sprite = GetNode<Sprite>(_spritePath);
        _animPlayer = GetNode<AnimationPlayer>(_animPlayerPath);
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
        _animPlayer.Play("scale_up");
        await ToSignal(_animPlayer, "animation_finished");
        await ToSignal(GetTree().CreateTimer(DefaultScaleDuration, false), "timeout");
        _animPlayer.Play("scale_down_to_normal");
        await ToSignal(_animPlayer, "animation_finished");
        IsMutated = false;
        // // _mass = 0;
        // // var info = _itemScaleInfo.GetScaleInfo(type);
        // await PlayPreScaleAnimation();
        // _sprite.Texture = _largeTexture;
        // SwitchCollisionShape(_largeCollisionShape);
        // // GetTree().CreateTween().TweenProperty(CollisionShapeNode, "scale", info.Scale, 0.1f);
        // // OtherChildToScale.Scale = info.Scale;
        // // IsCurrentlyCarryable = info.Carryable;
        // // InteractionArea.SetDisabled(!IsCurrentlyCarryable);
        // // CarryOffset = info.CarryOffset;
        // // _mass = info.Mass;
        // // _friction = info.Friction;
        // // Position += info.Offset;
        // await ToSignal(GetTree().CreateTimer(DefaultScaleDuration, false), "timeout");
        // IsMutated = false;
        // SwitchCollisionShape(_normalCollisionShape);
        // // _mass = 0;
        // // GetTree().CreateTween().TweenProperty(CollisionShapeNode, "scale", _normalScale, 0.1f);
        // // OtherChildToScale.Scale = _normalScale;
        // _sprite.Texture = _normalTexture;
        // // IsCurrentlyCarryable = CanBeCarriedByDefault;
        // // InteractionArea.SetDisabled(!IsCurrentlyCarryable);
        // // CarryOffset = DefaultCarryOffset;
        // // _mass = DefaultMass;
        // _friction = DefaultFriction;
        // Position -= info.Offset;
    }

    // protected virtual async Task PlayPreScaleAnimation()
    // {
    //     var duration = 0.3f;
    //     var rumbleFrequency = 5;
    //     var rumbleRotationDegrees = 5f;
    //     var postTweenDelay = 0.1f;
    //
    //     var originalRotation = RotationDegrees;
    //     var durationPerRumble = duration / rumbleFrequency;
    //     var tween = GetTree().CreateTween();
    //     for (var i = 0; i < rumbleFrequency; i++)
    //     {
    //         var rotationDegrees = rumbleRotationDegrees * (i % 2 == 0 ? 1 : -1);
    //         tween.TweenProperty(this, "rotation_degrees", rotationDegrees, durationPerRumble / 2);
    //         tween.TweenProperty(this, "rotation_degrees", originalRotation, durationPerRumble / 2);
    //     }
    //
    //     await ToSignal(tween, "finished");
    //     await ToSignal(GetTree().CreateTimer(postTweenDelay, false), "timeout");
    // }

    // private void SwitchCollisionShape(CollisionPolygon2D shape)
    // {
    //     _normalCollisionShape.
    //     // _currentlyEnabledCollisionShape.Disabled = true;
    //     // shape.Disabled = false;
    //     // _currentlyEnabledCollisionShape = shape;
    //     // ForceUpdateTransform();
    // }
}
