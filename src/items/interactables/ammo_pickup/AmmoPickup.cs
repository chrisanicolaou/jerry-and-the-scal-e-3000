using Godot;
using System;

public class AmmoPickup : Key
{
    [Signal] public delegate void AmmoPickupFound();
    public override void OnInteractionAreaActivated()
    {
        EmitSignal(nameof(AmmoPickupFound));
    }
}
