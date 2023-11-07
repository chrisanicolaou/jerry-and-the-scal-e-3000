using Godot;
using System;
using System.Threading.Tasks;
using GithubGameJam2023.player.player_gun;

public class ScalableItem : KinematicBody2D
{
    [Signal] public delegate void ItemInteractionRequested(ScalableItem item);
    [Export] protected bool PhysicsBased { get; set; } = true;
    [Export(PropertyHint.Range, "0,2,0.01")] protected float DefaultFriction { get; set; } = 0.5f;
    [Export(PropertyHint.Range, "2,10,0.1")] protected float DefaultScaleDuration { get; private set; } = 2f;

    [Export(PropertyHint.Range, "1,100,0.1")] protected float DefaultMass { get; set; } = 10;
    public bool IsBeingCarried { get; set; }
    public Vector2 CarryOffset { get; set; }
    public bool DisablePhysics { get; set; } = false;

    [Export] protected bool CanBeCarriedByDefault { get; set; }
    [Export] protected Vector2 DefaultCarryOffset { get; set; }
    
    [Export] private ScalableItemInfo _itemScaleInfo;
    [Export] private NodePath _spritePath;
    [Export] private NodePath _collisionShapePath;
    [Export] private NodePath _otherChildToScalePath;
    [Export] private NodePath _interactionAreaPath;
    protected Sprite Sprite { get; private set; }
    protected Node2D CollisionShapeNode { get; private set; }
    protected Node2D OtherChildToScale { get; private set; }
    protected InteractionArea InteractionArea { get; private set; }
    protected bool IsMutated { get; set; }
    protected Vector2 Velocity;
    
    private Texture _normalTex;
    private Vector2 _normalScale;
    private float _mass;
    private float _friction;
    private float _gravity = Convert.ToInt32(ProjectSettings.GetSetting("physics/2d/default_gravity"));
    private float _massFactor = 0.1f;
    
    public bool IsCurrentlyCarryable { get; set; }
    public Vector2 ExertingForce => (Vector2.One + _previousVelocity) * _mass;
    private Vector2 _previousVelocity;

    public override void _Ready()
    {
        AddToGroup("scalable");
        Sprite = GetNode<Sprite>(_spritePath);
        CollisionShapeNode = GetNode<Node2D>(_collisionShapePath);
        OtherChildToScale = GetNode<Node2D>(_otherChildToScalePath);
        InteractionArea = GetNode<InteractionArea>(_interactionAreaPath);
        InteractionArea.Connect(nameof(InteractionArea.InteractionAreaTriggered), this, nameof(OnInteractionAreaTriggered));
        _normalScale = CollisionShapeNode.Scale;
        _normalTex = Sprite.Texture;
        _mass = DefaultMass;
        _friction = DefaultFriction;
        IsCurrentlyCarryable = CanBeCarriedByDefault;
        CarryOffset = DefaultCarryOffset;
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

    public virtual void OnInteractionAreaTriggered()
    {
        if (!IsCurrentlyCarryable) return;
        EmitSignal(nameof(ItemInteractionRequested), this);
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
        _mass = 0;
        var info = _itemScaleInfo.GetScaleInfo(type);
        await PlayPreScaleAnimation(type == ScaleType.Big);
        Sprite.Texture = info.Tex;
        GetTree().CreateTween().TweenProperty(CollisionShapeNode, "scale", info.Scale, 0.1f);
        OtherChildToScale.Scale = info.Scale;
        IsCurrentlyCarryable = info.Carryable;
        CarryOffset = info.CarryOffset;
        _mass = info.Mass;
        _friction = info.Friction;
        Position += info.Offset;
        await ToSignal(GetTree().CreateTimer(DefaultScaleDuration, false), "timeout");
        IsMutated = false;
        _mass = 0;
        GetTree().CreateTween().TweenProperty(CollisionShapeNode, "scale", _normalScale, 0.1f);
        OtherChildToScale.Scale = _normalScale;
        Sprite.Texture = _normalTex;
        IsCurrentlyCarryable = CanBeCarriedByDefault;
        CarryOffset = DefaultCarryOffset;
        _mass = DefaultMass;
        _friction = DefaultFriction;
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

    public override void _PhysicsProcess(float delta)
    {
        if (!PhysicsBased || DisablePhysics) return;

        _previousVelocity = new Vector2(Velocity);
        
        if (!IsOnFloor())
        {
            Velocity.y += _gravity * delta * (_mass * _massFactor);
        }

        Velocity = MoveAndSlide(Velocity, Vector2.Up);
        if (Velocity.x != 0)
        {
            var directionSign = Velocity.x > 0 ? 1 : -1;
            Velocity.x -= directionSign == -1 ? -_friction : _friction;
            if (Mathf.Sign(Velocity.x) != directionSign) Velocity.x = 0;
        }

        for (var i = 0; i < GetSlideCount(); i++)
        {
            var col = GetSlideCollision(i);
            if (col.Collider is BreakableItem breakableItem)
            {
                if (ExertingForce.x >= breakableItem.ForceRequiredToBreak || ExertingForce.y >= breakableItem.ForceRequiredToBreak)
                {
                    breakableItem.Break();
                }
            }
            if (col.Collider is Player || col.Collider is Bullet && Mathf.Abs(Velocity.x) > 0.001)
            {
                Velocity = Vector2.Zero;
            }
        }
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