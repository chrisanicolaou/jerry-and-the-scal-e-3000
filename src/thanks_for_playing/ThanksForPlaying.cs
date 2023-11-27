using Godot;
using System;

public class ThanksForPlaying : Control
{
    [Signal] public delegate void CreditsFinished();
    
    [Export] private float _holdDuration = 1f;
    [Export] private float _creditScrollDuration = 5f;
    [Export] private float _thanksForPlayingFadeDuration = 2f;
    [Export] private int _finalScrollValue = 745;
    [Export] private NodePath _scrollContainerPath;
    [Export] private NodePath _thanksForPlayingLabelPath;

    private ScrollContainer _scrollContainer;
    private Label _thanksForPlayingLabel;

    public override void _Ready()
    {
        _scrollContainer = GetNode<ScrollContainer>(_scrollContainerPath);
        _thanksForPlayingLabel = GetNode<Label>(_thanksForPlayingLabelPath);
        _scrollContainer.ScrollVertical = 0;
    }

    public async void ScrollCredits()
    {
        var tween = GetTree().CreateTween();
        tween.TweenProperty(_scrollContainer, "scroll_vertical", _finalScrollValue, _creditScrollDuration);
        await ToSignal(tween, "finished");
        tween = GetTree().CreateTween();
        tween.TweenProperty(_thanksForPlayingLabel, "modulate:a", 1, _thanksForPlayingFadeDuration);
        await ToSignal(tween, "finished");
        await ToSignal(GetTree().CreateTimer(_holdDuration), "timeout");
        EmitSignal(nameof(CreditsFinished));
    }
}
