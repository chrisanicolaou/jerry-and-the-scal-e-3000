using Godot;
using System;

public class WaterDropGenerator : Node2D
{
    [Export] private float _delayBetweenDrops;
    [Export] private PackedScene _waterDropScene;
    [Export] private NodePath _waterDropSpawnPointPath;
    [Export] private NodePath _waterDropGeneratorSpritePath;
    [Export] private NodePath _animationPlayerPath;

    private float _timeSinceLastDrop;
    private Node2D _waterDropSpawnPoint;
    private Sprite _waterDropGeneratorSprite;
    private AnimationPlayer _animationPlayer;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _animationPlayer = GetNode<AnimationPlayer>(_animationPlayerPath);
        _waterDropSpawnPoint = GetNode<Node2D>(_waterDropSpawnPointPath);
        _waterDropGeneratorSprite = GetNode<Sprite>(_waterDropGeneratorSpritePath);
        _waterDropGeneratorSprite.Frame = 0;
    }

    public override void _PhysicsProcess(float delta)
    {
        _timeSinceLastDrop += delta;

        if (_timeSinceLastDrop > _delayBetweenDrops)
        {
            _timeSinceLastDrop = 0;
            PlayWaterDropAnimation();
        }
    }

    private void PlayWaterDropAnimation()
    {
        _animationPlayer.Play("drop_form");
        _animationPlayer.Connect("animation_finished", this, nameof(GenerateWaterDrop));
    }

    private void GenerateWaterDrop(string _)
    {
        _animationPlayer.Disconnect("animation_finished", this, nameof(GenerateWaterDrop));
        _animationPlayer.Play("drop_unform");
        var waterDrop = _waterDropScene.Instance();
        _waterDropSpawnPoint.AddChild(waterDrop);
    }
}
