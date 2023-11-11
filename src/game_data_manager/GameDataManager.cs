using Godot;
using System;

public class GameDataManager : Node
{
    [Export] public LevelCollection AllLevelsCollection { get; private set; }
    [Export] public int CurrentLevelIndex { get; set; }
    [Export] public int HighestUnlockedLevelIndex { get; set; }
    
    public LevelData CurrentLevel => CurrentLevelIndex < AllLevelsCollection.Levels.Count ? AllLevelsCollection.Levels[CurrentLevelIndex] : null;
}
