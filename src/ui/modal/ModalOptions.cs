using Godot;
using System;

public class ModalOptions : Resource
{
    [Export] public Texture IconTex { get; private set; }
    [Export] public float IconScale { get; private set; } = 1f;
    [Export] public string Title { get; private set; }
    [Export] public string Description { get; private set; }
    [Export] public float ContinueDelayDuration { get; private set; } = 2;
}
