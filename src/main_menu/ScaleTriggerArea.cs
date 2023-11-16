using Godot;
using System;
using GithubGameJam2023.player.player_gun;

public class ScaleTriggerArea : Area2D
{
    public override void _Ready()
    {
        Connect("body_entered", this, nameof(OnBodyEntered));
    }

    private void OnBodyEntered(Node body)
    {
        if (body is ScalableItemV2 scalableItem)
        {
            scalableItem.ChangeScale(ScaleType.BigPreserveMass);
        }
    }
}
