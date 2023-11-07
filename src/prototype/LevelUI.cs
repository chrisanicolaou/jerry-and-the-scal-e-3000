using Godot;
using System;
using System.Linq;

public class LevelUI : CanvasLayer
{
    [Export(PropertyHint.Range, "0,1")] private float _transparency;
    [Export] private NodePath _coinContainerPath;
    [Export] private NodePath _keyContainerPath;
    [Export] private NodePath _bulletContainerPath;
    [Export] private PackedScene _controlWrappedSpriteScene;
    [Export] private Texture _coinTex;
    [Export] private Texture _keyTex;
    [Export] private Texture _bulletTex;

    private Control _coinContainer;
    private Control _keyContainer;
    private Control _bulletContainer;
    private int _keysCollected;
    private int _coinsCollected;
    private int _shotsFired;
    private ControlWrappedSprite[] _keys;
    private ControlWrappedSprite[] _coins;
    private ControlWrappedSprite[] _bullets;

    public override void _Ready()
    {
        _coinContainer = GetNode<Control>(_coinContainerPath);
        _keyContainer = GetNode<Control>(_keyContainerPath);
        _bulletContainer = GetNode<Control>(_bulletContainerPath);
        // Lets us leave placeholders in the UI for testing
        RemoveAllChildren(_coinContainer);
        RemoveAllChildren(_keyContainer);
        RemoveAllChildren(_bulletContainer);
    }

    public void Initialise(int numOfKeys, int numOfCoins, int numOfBullets)
    {
        _keys = Enumerable.Range(0, numOfKeys).Select(_ => AddNewControlWrappedSprite(_keyTex, _keyContainer)).ToArray();
        _coins = Enumerable.Range(0, numOfCoins).Select(_ => AddNewControlWrappedSprite(_coinTex, _coinContainer)).ToArray();
        _bullets = Enumerable.Range(0, numOfBullets).Select(_ => AddNewControlWrappedSprite(_bulletTex, _bulletContainer, false, false)).ToArray();
    }

    public void AddCollectedCoin()
    {
        _coins[_coinsCollected].Sprite.Modulate = new Color(_coins[_coinsCollected++].Sprite.Modulate);
    }

    public void AddCollectedKey()
    {
        _keys[_keysCollected].Sprite.Modulate = new Color(_keys[_keysCollected++].Sprite.Modulate);
    }

    public void RemoveBullet()
    {
        _bullets[_bullets.Length - ++_shotsFired].Sprite.Modulate = new Color(_bullets[_bullets.Length - _shotsFired].Sprite.Modulate, _transparency);
    }

    // Lots of tight coupling here between specific sprite/sizes. NOT good
    private ControlWrappedSprite AddNewControlWrappedSprite(Texture tex, Node parent, bool transparent = true, bool doubleScale = true)
    {
        var control = _controlWrappedSpriteScene.Instance<ControlWrappedSprite>();
        parent.AddChild(control);
        control.Sprite.Texture = tex;
        if (tex == _coinTex) control.Sprite.Hframes = 8;
        if (tex == _keyTex) control.Sprite.Position = new Vector2(0, -4);
        control.Sprite.Modulate = new Color(control.Sprite.Modulate, transparent ? _transparency : 1);
        control.Sprite.Scale = doubleScale ? Vector2.One * 2 : Vector2.One;
        return control;
    }

    private void RemoveAllChildren(Node node)
    {
        foreach (Node child in node.GetChildren())
        {
            child?.QueueFree();
        }
    }
}
