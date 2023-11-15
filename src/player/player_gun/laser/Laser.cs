using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Laser : Line2D
{
    [Export] private float _width = 10f;
    [Export] private float _holdDuration = 0.5f;
    [Export] private float _powerUpDuration = 0.3f;
    [Export] private Tween.TransitionType _transType = Tween.TransitionType.Cubic;
    [Export] private Tween.EaseType _ease = Tween.EaseType.InOut;
    [Export] private float _particleStartDelay = 0.1f;
    [Export] private PackedScene _laserContactParticlesScene;

    public override void _Ready()
    {
        Hide();
    }

    public async Task ActivateLaser(Vector2[] corePathPoints)
    {
        Show();
        Points = corePathPoints;
        var tween = GetTree().CreateTween().SetTrans(_transType).SetEase(_ease);
        tween.TweenProperty(this, "width", _width, _powerUpDuration).From(0f);
        await ToSignal(GetTree().CreateTimer(_particleStartDelay, false), "timeout");
        var particles = new List<Particles2D>();
        foreach (var point in corePathPoints)
        {
            var particle = _laserContactParticlesScene.Instance<Particles2D>();
            particle.Position = point;
            AddChild(particle);
            particle.OneShot = true;
            particle.Emitting = true;
            particles.Add(particle);
        }
        await ToSignal(GetTree().CreateTimer(_holdDuration, false), "timeout");
        tween = GetTree().CreateTween().SetTrans(_transType).SetEase(_ease);
        tween.TweenProperty(this, "width", 0f, _powerUpDuration).From(_width);
        await ToSignal(tween, "finished");
        particles.ForEach(p => p.QueueFree());
        Hide();
    }
}
