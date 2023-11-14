using Godot;
using System;

public class GameDataManager : Node
{
    [Export] public LevelCollection AllLevelsCollection { get; private set; }

    public LevelData GetLevelData(int index) => index >= AllLevelsCollection.Levels.Count ? null : AllLevelsCollection.Levels[index];
    public PlayerData PlayerData { get; set; } = new PlayerData();
}
