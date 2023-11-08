using Godot;
using System;
using GithubGameJam2023.player.player_gun;
using GithubGameJam2023.prototype;

public class ScalableItemInfo : Resource
{
    [Export] public Texture BigTex { get; set; }
    [Export] public Vector2 BigScale { get; set; }
    [Export] public float BigMass { get; set; } = 10;
    [Export] public float BigFriction { get; set; } = 0.5f;
    [Export] public Vector2 BigOffset { get; set; }
    [Export] public Vector2 BigCarryOffset { get; set; }
    [Export] public Texture SmallTex { get; set; }
    [Export] public Vector2 SmallScale { get; set; }
    [Export] public float SmallMass { get; set; } = 10;
    [Export] public float SmallFriction { get; set; } = 0.5f;
    [Export] public Vector2 SmallOffset { get; set; }
    [Export] public bool CanBeCarriedWhenSmall { get; set; }
    [Export] public Vector2 SmallCarryOffset { get; set; }

    public ScaleInfo GetScaleInfo(ScaleType type)
    {
        Texture tex;
        Vector2 scale;
        Vector2 offset;
        var carryable = false;
        var carryOffset = Vector2.Zero;
        var mass = 0f;
        var friction = 0f;

        switch (type)
        {
            case ScaleType.Big:
                tex = BigTex;
                scale = BigScale;
                offset = BigOffset;
                carryOffset = BigCarryOffset;
                mass = BigMass;
                friction = BigFriction;
                break;
            case ScaleType.Small:
                tex = SmallTex;
                scale = SmallScale;
                offset = SmallOffset;
                carryable = CanBeCarriedWhenSmall;
                carryOffset = SmallCarryOffset;
                mass = SmallMass;
                friction = SmallFriction;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        return new ScaleInfo { Tex = tex, Offset = offset, Scale = scale, Carryable = carryable, CarryOffset = carryOffset, Mass = mass, Friction = friction };
    }
}
