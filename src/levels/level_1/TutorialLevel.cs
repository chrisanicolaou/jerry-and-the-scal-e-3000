using Godot;
using System;

public class TutorialLevel : Level
{
    public override async void StartLevel()
    {
        // Replace this with logic to "wake the player up" etc
        Player.GlobalPosition = EntranceDoor.GlobalPosition;
        await EntranceDoor.Open();
        Player.Visible = true;
        await Player.EnterLevel(EntranceDoor);
        await EntranceDoor.Close();

        
        Player.Freeze = false;
    }
}
