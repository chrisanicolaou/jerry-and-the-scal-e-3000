using Godot;
using System;

public class FallZone : Area2D
{
    [Signal] public delegate void PlayerFell();
    
    public override void _Ready()
    {
        Connect("body_exited", this, nameof(OnBodyExited));
    }

    private void OnBodyExited(Node body)
    {
        if (body is Player) EmitSignal(nameof(PlayerFell));
    }
}
