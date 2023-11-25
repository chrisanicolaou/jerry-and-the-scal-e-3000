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
    private string _actionToBlock = "";

    private int _currentPromptIndex = -1;

    private string[] _tutorialPrompts = new[]
    {
        "Press W or D to move. Press space to jump",
        "Aim the Scal-E 3000 at the pillar, then left click to embiggen",
        "The Scal-E 3000 can also shrink items. Aim at the pillar again, then right click to shrink",
        "You'll have to find a key to progress",
        "If you ever get stuck, press ESC to pause and restart. Good luck!"
    };

    public override void _Ready()
    {
        base._Ready();
        
        // Suspend key for now
        Key.Mode = RigidBody2D.ModeEnum.Static;
        _tutorialItem = GetNode<ScalableItemV2>(_tutorialItemPath);
        _tutorialPrompt = _tutorialPromptScene.Instance<TutorialPrompt>();
        UI.AddChild(_tutorialPrompt);
        
        var gunPickup = GetNode<GunPickup>(_gunPickupPath);
        gunPickup.Connect(nameof(GunPickup.GunPickupRequested), this, nameof(OnGunPickupRequested));
    }

    public override void _Input(InputEvent inputEvent)
    {
        if (_actionToBlock == "") return;
        if (inputEvent.IsActionPressed(_actionToBlock))
        {
            GetTree().SetInputAsHandled();
        }
    }

    public override async void StartLevel()
    {
        // Replace this with logic to "wake the player up" etc
        Player.GlobalPosition = EntranceDoor.GlobalPosition;
        await EntranceDoor.Open();
        Player.Visible = true;
        await Player.EnterLevel(EntranceDoor);
        await EntranceDoor.Close();
        AdvanceTutorial();
        
        Player.Freeze = false;
    }

    private void OnGunPickupRequested() => HandleGunPickup();

    private async void HandleGunPickup()
    {
        Player.PickupGun();
        await ShowModal(_gunPickupModalOpts);
        AdvanceTutorial();
        _tutorialItem.Connect(nameof(ScalableItemV2.ChangedScale), this, nameof(OnTutorialItemEmbiggened));
        _actionToBlock = "shoot_small";
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
        _actionToBlock = "shoot_big";
    }

    private void OnTutorialItemShrunk(ScalableItemSize size)
    {
        if (size == ScalableItemSize.Big) return;
        _tutorialItem.Disconnect(nameof(ScalableItemV2.ChangedScale), this, nameof(OnTutorialItemShrunk));
        AdvanceTutorial();
        DropKey();
    }

    private void AdvanceTutorial()
    {
        // Any animations to hide/show tutorial prompt would go here
        if (_tutorialPrompt.Visible) _tutorialPrompt.Hide();
        if (++_currentPromptIndex >= _tutorialPrompts.Length) return;
        
        var prompt = _tutorialPrompts[_currentPromptIndex];
        ShowTutorialText(prompt);
    }

    private void DropKey()
    {
        _actionToBlock = "";
        Key.Mode = RigidBody2D.ModeEnum.Rigid;
    }

    protected override void OnKeyFound()
    {
        base.OnKeyFound();
        AdvanceTutorial();
    }
}
