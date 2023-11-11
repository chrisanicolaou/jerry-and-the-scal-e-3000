using Godot;
using System;

public class GameDataManager : Node
{
    [Export] public LevelCollection AllLevelsCollection { get; private set; }
    [Export] public int CurrentLevelIndex { get; set; }
    
    public LevelData CurrentLevel => AllLevelsCollection.Levels[CurrentLevelIndex];
}
