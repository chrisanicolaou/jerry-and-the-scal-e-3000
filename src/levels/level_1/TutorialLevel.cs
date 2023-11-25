using Godot;
using System;
using GithubGameJam2023.items.scalable_two;
using Godot.Collections;
using Array = Godot.Collections.Array;

public class TutorialLevel : Level
{
    [Export] private NodePath _gunPickupPath;
    [Export] private ModalOptions _gunPickupModalOpts;
    [Export] private PackedScene _tutorialPromptScene;
    [Export] private NodePath _tutorialItemPath;

    private TutorialPrompt _tutorialPrompt;
    private ScalableItemV2 _tutorialItem;

    private int _currentPromptIndex = -1;

    private string[] _tutorialPrompts = new[]
    {
        "Aim the Scal-E 3000 at the boulder, then left click to embiggen",
        "The Scal-E 3000 can also shrink items. Aim at the boulder again, then right click to shrink",
    };

    public override void _Ready()
    {
        base._Ready();
        _tutorialItem = GetNode<ScalableItemV2>(_tutorialItemPath);
        _tutorialPrompt = _tutorialPromptScene.Instance<TutorialPrompt>();
        UI.AddChild(_tutorialPrompt);
        
        var gunPickup = GetNode<GunPickup>(_gunPickupPath);
        gunPickup.Connect(nameof(GunPickup.GunPickupRequested), this, nameof(OnGunPickupRequested));
    }

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

    private void OnGunPickupRequested() => HandleGunPickup();

    private async void HandleGunPickup()
    {
        Player.PickupGun();
        await ShowModal(_gunPickupModalOpts);
        AdvanceTutorial();
        _tutorialItem.Connect(nameof(ScalableItemV2.ChangedScale), this, nameof(OnTutorialItemEmbiggened));
    }

    private void ShowTutorialText(string text)
    {
        _tutorialPrompt.ShowPrompt(text);
    }

    private void OnTutorialItemEmbiggened(ScalableItemSize size)
    {
        if (size != ScalableItemSize.Big) return;
        _tutorialItem.Disconnect(nameof(ScalableItemV2.ChangedScale), this, nameof(OnTutorialItemEmbiggened));
        AdvanceTutorial();
        _tutorialItem.Connect(nameof(ScalableItemV2.ChangedScale), this, nameof(OnTutorialItemShrunk));
    }

    private void OnTutorialItemShrunk(ScalableItemSize size)
    {
        if (size == ScalableItemSize.Big) return;
        _tutorialItem.Disconnect(nameof(ScalableItemV2.ChangedScale), this, nameof(OnTutorialItemShrunk));
        AdvanceTutorial();
    }

    private void AdvanceTutorial()
    {
        // Any animations to hide/show tutorial prompt would go here
        if (_tutorialPrompt.Visible) _tutorialPrompt.Hide();
        if (++_currentPromptIndex >= _tutorialPrompts.Length) return;
        
        var prompt = _tutorialPrompts[_currentPromptIndex];
        ShowTutorialText(prompt);
    }
}
