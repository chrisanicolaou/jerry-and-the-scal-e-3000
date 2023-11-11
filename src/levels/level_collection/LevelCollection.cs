using Godot;
using System;
using Godot.Collections;

public class LevelCollection : Resource
{
    [Export] public Array<LevelData> Levels { get; private set; }
}