using Godot;
using System;

public class PlayerData : Resource
{
    [Export] public int CurrentLevelIndex { get; set; }
    [Export] public int HighestUnlockedLevelIndex { get; set; }
}
