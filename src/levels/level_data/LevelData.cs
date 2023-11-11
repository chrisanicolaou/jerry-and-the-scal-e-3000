using Godot;
using System;

public class LevelData : Resource
{
    [Export] public string DisplayName { get; private set; }
    [Export] public PackedScene Scene { get; private set; }
}
