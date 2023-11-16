using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GithubGameJam2023.player.player_gun;

public class Laser : Line2D
{
    [Export] private Color _scaleUpCol;
    [Export] private Color _scaleDownCol;
    [Export] private float _width = 10f;
    [Export] private float _holdDuration = 0.5f;
    [Export] private float _powerUpDuration = 0.3f;
    [Export] private Tween.TransitionType _transType = Tween.TransitionType.Cubic;
    [Export] private Tween.EaseType _ease = Tween.EaseType.InOut;
    [Export] private float _particleStartDelay = 0.1f;
    [Export] private float _particleImpactMultiplier = 1.5f;
    [Export] private PackedScene _laserContactParticlesScene;
    [Export] private ParticlesMaterial _laserContactParticleMat;
    [Export] private ParticlesMaterial _laserContactParticleCollisionMat;
    [Export] private PackedScene _laserBeamParticlesScene;
    [Export] private bool _enableBeamParticles;
    // [Export] private float _beamParticleTravelDuration = 0.1f;
    // [Export] private int _beamParticleAmount = 32;

    public override void _Ready()
    {
        Hide();
    }

    public async Task ActivateLaser(List<(Vector2, Vector2)> corePathPoints, ScaleType type)
    {
        Show();
        var color = type == ScaleType.Big ? _scaleUpCol : _scaleDownCol;
        DefaultColor = color;
        Points = corePathPoints.Select(cp => cp.Item1).ToArray();
        var tween = GetTree().CreateTween().SetTrans(_transType).SetEase(_ease);
        tween.TweenProperty(this, "width", _width, _powerUpDuration).From(0f);
        await ToSignal(GetTree().CreateTimer(_particleStartDelay, false), "timeout");
        var particles = new List<Particles2D>();
        (Vector2, Vector2)? previousPoint = null;
        foreach (var point in corePathPoints)
        {
            var particle = _laserContactParticlesScene.Instance<Particles2D>();
            particle.Position = point.Item1;
            particle.ProcessMaterial = previousPoint == null ? _laserContactParticleMat : _laserContactParticleCollisionMat;
            particle.ProcessMaterial.Set("color", color);
            AddChild(particle);
            particle.OneShot = true;
            particle.Emitting = true;
            particle.GlobalRotation = point.Item2.Angle();
            particles.Add(particle);
            if (previousPoint != null && _enableBeamParticles)
            {
                var prevPoint = previousPoint.Value;
                var beamParticle = _laserBeamParticlesScene.Instance<Particles2D>();
                beamParticle.Position = (prevPoint.Item1 + point.Item1) / 2;
                beamParticle.GlobalRotation = prevPoint.Item2.Angle();
                beamParticle.ProcessMaterial.Set("emission_box_extents", new Vector3(point.Item2.DistanceTo(prevPoint.Item1) / 4, Width, 1));
                beamParticle.ProcessMaterial.Set("color", color);
                AddChild(beamParticle);
                beamParticle.OneShot = true;
                beamParticle.Emitting = true;
                particles.Add(beamParticle);
            }
            previousPoint = point;
        }
        await ToSignal(GetTree().CreateTimer(_holdDuration, false), "timeout");
        tween = GetTree().CreateTween().SetTrans(_transType).SetEase(_ease);
        tween.TweenProperty(this, "width", 0f, _powerUpDuration).From(_width);
        await ToSignal(tween, "finished");
        particles.ForEach(p => p.QueueFree());
        Hide();
    }
}
