using Godot;
using System;
using System.ComponentModel.Design;
using ChiciStudios.GithubGameJam2023.Common.Audio;
using GithubGameJam2023.items.scalable_two;
using Godot.Collections;
using Array = Godot.Collections.Array;

public class TutorialLevel : Level
{
    [Export] private NodePath _gunPickupPath;
    [Export] private ModalOptions _gunPickupModalOpts;
    [Export] private PackedScene _tutorialPromptScene;
    [Export] private NodePath _tutorialItemPath;
    [Export] private NodePath _gunSpinSpritePath;
    [Export] private NodePath _gunSpinSpriteTwoPath;
    [Export] private NodePath _gunSpinAnimPlayerPath;
    [Export] private float _gunSpinPostAnimDelay = 0.5f;
    [Export] private AudioStream _scalEPickUpSfx;

    private AudioManager _audioManager;
    private TutorialPrompt _tutorialPrompt;
    private ScalableItemV2 _tutorialItem;
    private Sprite _gunSpinSprite;
    private Sprite _gunSpinSpriteTwo;
    private AnimationPlayer _gunSpinAnimPlayer;
    private string _actionToBlock = "";

    private int _currentPromptIndex = -1;

    private string[] _tutorialPrompts = {
        "Aim the Scal-E 3000 at the pillar, then left click to embiggen",
        "The Scal-E 3000 can also shrink items. Aim at the pillar again, then right click to shrink",
        "You'll have to find a key to progress",
        "If you ever get stuck, press P to pause and restart. Good luck!"
    };

    public override void _Ready()
    {
        base._Ready();

        _audioManager = GetNode<AudioManager>("/root/AudioManager");
        
        // Suspend key for now
        Key.Mode = RigidBody2D.ModeEnum.Static;
        Key.Hide();
        _tutorialItem = GetNode<ScalableItemV2>(_tutorialItemPath);
        _tutorialPrompt = _tutorialPromptScene.Instance<TutorialPrompt>();
        UI.AddChild(_tutorialPrompt);
        
        var gunPickup = GetNode<GunPickup>(_gunPickupPath);
        gunPickup.Connect(nameof(GunPickup.GunPickupRequested), this, nameof(OnGunPickupRequested));
        _gunSpinSprite = GetNode<Sprite>(_gunSpinSpritePath);
        _gunSpinSpriteTwo = GetNode<Sprite>(_gunSpinSpriteTwoPath);
        _gunSpinSprite.Hide();
        _gunSpinSpriteTwo.Hide();
        _gunSpinAnimPlayer = GetNode<AnimationPlayer>(_gunSpinAnimPlayerPath);
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
        _audioManager.PlaySfx(_scalEPickUpSfx, new AudioOptions { BusName = AudioBusName.Sfx, PauseMode = PauseModeEnum.Process });
        Player.PickupGun();
        _gunSpinSprite.Show();
        _gunSpinSpriteTwo.Show();
        _gunSpinAnimPlayer.Play("gun_spin");
        // await ToSignal(_gunSpinAnimPlayer, "animation_finished");
        // if (_gunSpinPostAnimDelay > 0.01f) await ToSignal(GetTree().CreateTimer(_gunSpinPostAnimDelay, false), "timeout");
        // _gunSpinSprite.Hide();
        await ShowModal(_gunPickupModalOpts);
        _gunSpinSprite.Hide();
        _gunSpinSpriteTwo.Hide();
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
        Key.Show();
    }

    protected override void OnKeyFound()
    {
        base.OnKeyFound();
        AdvanceTutorial();
    }
}
