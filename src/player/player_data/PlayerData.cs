using Godot;
using System;

public class PlayerData : Resource
{
    [Export] public int CurrentLevelIndex { get; set; }
    [Export] public int HighestUnlockedLevelIndex { get; set; }
    [Export] public float MasterDb { get; set; }
    [Export] public float MusicDb { get; set; }
    [Export] public float SfxDb { get; set; }
    [Export] public bool AimLineEnabled { get; set; } = true;
    [Export] public bool EasyModeEnabled { get; set; }
}
