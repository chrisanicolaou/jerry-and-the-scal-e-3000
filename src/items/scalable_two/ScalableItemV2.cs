using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GithubGameJam2023.player.player_gun;

public class ScalableItemV2 : RigidBody2D
{
    [Signal] public delegate void ItemInteractionRequested(ScalableItemV2 item);

    [Export] private NodePath _spritePath;
    [Export] private NodePath _animPlayerPath;
    [Export] private NodePath _interactionAreaPath;
    [Export] private ShaderMaterial _whiteShaderMat;
    [Export] private ShaderMaterial _outlineShaderMat;
    
    [Export(PropertyHint.Range, "5,10,0.1")] protected float DefaultScaleDuration { get; private set; } = 4f;
    [Export] public bool CanBeCarried { get; set; }
    [Export] public Vector2 CarryOffset { get; set; }

    private Sprite _sprite;
    private AnimationPlayer _animPlayer;
    private InteractionArea _interactionArea;
    private List<BreakableItem> _breakableItemsInContact = new List<BreakableItem>();
    
    public bool IsMutated { get; set; }

    public override void _Ready()
    {
        _sprite = GetNode<Sprite>(_spritePath);
        _sprite.Material = null;
        _animPlayer = GetNode<AnimationPlayer>(_animPlayerPath);
        _interactionArea = GetNode<InteractionArea>(_interactionAreaPath);
        _interactionArea.Connect(nameof(InteractionArea.InteractionAreaTriggered), this, nameof(OnInteractionAreaTriggered));
        _interactionArea.SetDisabled(!CanBeCarried);
        Connect("body_entered", this, nameof(OnBodyEntered));
        Connect("body_exited", this, nameof(OnBodyExited));
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

    public void AddOutline()
    {
        _sprite.Material = _outlineShaderMat;
    }

    public void RemoveOutline()
    {
        _sprite.Material = null;
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
        var animName = type == ScaleType.Big ? "scale_up" : "scale_down";
        _animPlayer.Play(animName);
        await ToSignal(_animPlayer, "animation_finished");
        _interactionArea.SetDisabled(!CanBeCarried);

        PlayScaleDurationAnimation();
        await ToSignal(GetTree().CreateTimer(DefaultScaleDuration, false), "timeout");
        
        _animPlayer.Play(animName, customSpeed: -1, fromEnd: true);
        await ToSignal(_animPlayer, "animation_finished");
        _interactionArea.SetDisabled(!CanBeCarried);
        IsMutated = false;
    }

    private async void PlayScaleDurationAnimation()
    {
        var preFlashDuration = DefaultScaleDuration - 3.75f;
        await ToSignal(GetTree().CreateTimer(preFlashDuration, false), "timeout");
        await PlayWhiteFlash(0.5f);
        await PlayWhiteFlash(0.5f);
        await PlayWhiteFlash(0.25f);
        await PlayWhiteFlash(0.25f);
        await PlayWhiteFlash(0.125f);
        await PlayWhiteFlash(0.125f);
        await PlayWhiteFlash(0.125f);
        await PlayWhiteFlash(0.125f);
    }

    private async Task PlayWhiteFlash(float duration)
    {
        _sprite.Material = _whiteShaderMat;
        await ToSignal(GetTree().CreateTimer(duration, false), "timeout");
        _sprite.Material = null;
        await ToSignal(GetTree().CreateTimer(duration, false), "timeout");
    }

    public override void _PhysicsProcess(float delta)
    {
        _interactionArea.RotationDegrees = -RotationDegrees;
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

    private void OnBodyEntered(Node body)
    {
        if (body is BreakableItem breakableItem) _breakableItemsInContact.Add(breakableItem);
    }

    private void OnBodyExited(Node body)
    {
        if (body is BreakableItem breakableItem) _breakableItemsInContact.Remove(breakableItem);
    }
}
