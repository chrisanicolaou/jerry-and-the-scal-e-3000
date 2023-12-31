using Godot;
using System;
using System.Collections.Generic;
using ChiciStudios.GithubGameJam2023.Common.Audio;
using GithubGameJam2023.items.scalable_two;
using GithubGameJam2023.player.player_gun;

// Over time, I dumped (and am continuing to dump) a fair bit of stuff in here that's specific to individual items.
// In retrospect, I should have made separate inheritors of ScalableItemV2 (which should also now just be ScalableItem) to reduce
// some of this clutter and complexity. Oops!
public class ScalableItemV2 : RigidBody2D
{
    [Signal] public delegate void ItemInteractionRequested(ScalableItemV2 item);
    [Signal] public delegate void ChangedScale(ScalableItemSize newSize);

    // THIS IS ONLY IN THE GAME BECAUSE GODOT DOESN'T CACHE SCENES PROPERLY ---- PLEASE IGNORE -------------------- //
    private bool _continuousMovementCalled;
    public bool ContinuousMovement { get; set; }
    public Vector2 ContinuousMovementStartPosition = new Vector2(-209, 271);
    public Vector2 ContinuousMovementLinearVelocity = new Vector2(70, 0);
    public float ContinuousMovementAngularVelocity = 3;
    // THIS IS ONLY IN THE GAME BECAUSE GODOT DOESN'T CACHE SCENES PROPERLY ---- PLEASE IGNORE -------------------- //
    
    [Export] private NodePath _spritePath;
    [Export] private NodePath _animPlayerPath;
    [Export] private NodePath _interactionAreaPath;
    [Export] private ShaderMaterial _whiteShaderMat;
    [Export] private ShaderMaterial _outlineShaderMat;
    [Export] private AudioStream _scaleUpSfx;
    [Export] private AudioStream _scaleDownSfx;
    [Export(PropertyHint.Layers2dPhysics)] private uint _colLayerWhenHeld;
    
    [Export(PropertyHint.Range, "5,10,0.1")] protected float DefaultScaleDuration { get; private set; } = 5f;
    [Export] public bool CanBeCarried { get; set; }
    [Export] public Vector2 CarryOffset { get; set; }

    private AudioManager _audioManager;
    private Sprite _sprite;
    private AnimationPlayer _animPlayer;
    private InteractionArea _interactionArea;
    private List<BreakableItem> _breakableItemsInContact = new List<BreakableItem>();
    private uint _originalColLayer;
    private uint _originalColMask;
    
    public bool IsMutated { get; set; }
    public bool IsMutating { get; set; }
    public ScalableItemSize Size { get; private set; } = ScalableItemSize.Normal;

    public override void _Ready()
    {
        _audioManager = GetNode<AudioManager>("/root/AudioManager");
        _sprite = GetNode<Sprite>(_spritePath);
        _sprite.Material = null;
        _animPlayer = GetNode<AnimationPlayer>(_animPlayerPath);
        _animPlayer.Play("RESET");
        if (_interactionAreaPath != null) _interactionArea = GetNode<InteractionArea>(_interactionAreaPath);
        _interactionArea?.Connect(nameof(InteractionArea.InteractionAreaTriggered), this, nameof(OnInteractionAreaTriggered));
        _interactionArea?.SetDisabled(!CanBeCarried);
        Connect("body_entered", this, nameof(OnBodyEntered));
        Connect("body_exited", this, nameof(OnBodyExited));
        _originalColLayer = CollisionLayer;
        _originalColMask = CollisionMask;
    }

    public void ChangeScale(ScaleType type)
    {
        if (type == ScaleType.Small && Size == ScalableItemSize.Small || type == ScaleType.Big && Size == ScalableItemSize.Big) return;
        IsMutating = true;
        HandleScale(type);
    }

    public void AddOutline()
    {
        _sprite.Material = _outlineShaderMat;
    }

    public void RemoveOutline()
    {
        _sprite.Material = null;
    }

    protected virtual void HandleScale(ScaleType type)
    {
        string animName;
        var fromEnd = Size != ScalableItemSize.Normal;
        switch (type)
        {
            case ScaleType.Big:
                animName = fromEnd ? "scale_down" : "scale_up";
                break;
            case ScaleType.BigPreserveMass:
                animName = fromEnd ? "scale_down" : "scale_up_preserve_mass";
                break;
            case ScaleType.Small:
                animName = fromEnd ? "scale_up" : "scale_down";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        Size = GetSizeFromScaleType(type);
        _audioManager.PlaySfx(type == ScaleType.Small ? _scaleDownSfx : _scaleUpSfx);
        _animPlayer.Play(animName, customSpeed: fromEnd ? -1 : 1, fromEnd: fromEnd);
        _animPlayer.Connect("animation_finished", this, nameof(HandleScaleAnimationFinished));
        EmitSignal(nameof(ChangedScale), Size);
    }

    public void HandleScaleAnimationFinished(string _)
    {
        _animPlayer.Disconnect("animation_finished", this, nameof(HandleScaleAnimationFinished));
        _interactionArea?.CallDeferred(nameof(InteractionArea.SetDisabled), !CanBeCarried);
        IsMutating = false;
    }
    
    public override void _PhysicsProcess(float delta)
    {
        if (_interactionArea != null) _interactionArea.RotationDegrees = -RotationDegrees;
        if (_breakableItemsInContact.Count <= 0) return;
        
        foreach (var breakableItem in _breakableItemsInContact)
        {
            if (Weight > breakableItem?.WeightRequiredToBreak) breakableItem?.Break();
        }
    }

    public virtual void OnInteractionAreaTriggered()
    {
        if (!CanBeCarried) return;
        EmitSignal(nameof(ItemInteractionRequested), this);
    }

    public void SwitchToHeldCollisionLayer()
    {
        CollisionMask = _colLayerWhenHeld;
        CollisionLayer = _colLayerWhenHeld;
    }

    public void SwitchToNormalCollisionLayer()
    {
        CollisionMask = _originalColMask;
        CollisionLayer = _originalColLayer;
    }

    private void OnBodyEntered(Node body)
    {
        if (body is BreakableItem breakableItem) _breakableItemsInContact.Add(breakableItem);
    }

    private void OnBodyExited(Node body)
    {
        if (body is BreakableItem breakableItem) _breakableItemsInContact.Remove(breakableItem);
    }

    public override void _IntegrateForces(Physics2DDirectBodyState state)
    {
        if (!ContinuousMovement) return;
        if (!_continuousMovementCalled)
        {
            Position = ContinuousMovementStartPosition;
            _animPlayer.Play("RESET");
        }
        _continuousMovementCalled = true;
        LinearVelocity = ContinuousMovementLinearVelocity;
        AngularVelocity = ContinuousMovementAngularVelocity;
    }

    private ScalableItemSize GetSizeFromScaleType(ScaleType type)
    {
        if (Size != ScalableItemSize.Normal) return ScalableItemSize.Normal;
        switch (type)
        {
            case ScaleType.Small:
                return ScalableItemSize.Small;
            case ScaleType.Big:
            case ScaleType.BigPreserveMass:
                return ScalableItemSize.Big;
            default:
                return ScalableItemSize.Normal;
        }
    }
}
