using Godot;
using System;
using System.Linq;

public class LevelUI : CanvasLayer
{
    [Export(PropertyHint.Range, "0,1")] private float _transparency;
    [Export] private NodePath _coinContainerPath;
    [Export] private NodePath _keyContainerPath;
    [Export] private PackedScene _controlWrappedSpriteScene;
    [Export] private Texture _coinTex;
    [Export] private Texture _keyTex;

    private Control _coinContainer;
    private Control _keyContainer;
    private int _keysCollected;
    private int _coinsCollected;
    private ControlWrappedSprite[] _keys;
    private ControlWrappedSprite[] _coins;

    public override void _Ready()
    {
        _coinContainer = GetNode<Control>(_coinContainerPath);
        _keyContainer = GetNode<Control>(_keyContainerPath);
        // Lets us leave placeholders in the UI for testing
        RemoveAllChildren(_coinContainer);
        RemoveAllChildren(_keyContainer);
    }

    public void Initialise(int numOfKeys, int numOfCoins)
    {
        _keys = Enumerable.Range(0, numOfKeys).Select(_ => AddNewControlWrappedSprite(_keyTex, _keyContainer)).ToArray();
        _coins = Enumerable.Range(0, numOfKeys).Select(_ => AddNewControlWrappedSprite(_coinTex, _coinContainer)).ToArray();
    }

    public void AddCollectedCoin()
    {
        _coins[_coinsCollected].Sprite.Modulate = new Color(_coins[_coinsCollected++].Sprite.Modulate);
    }

    public void AddCollectedKey()
    {
        _keys[_keysCollected].Sprite.Modulate = new Color(_keys[_keysCollected++].Sprite.Modulate);
    }

    // Lots of tight coupling here between specific sprite/sizes. NOT good
    private ControlWrappedSprite AddNewControlWrappedSprite(Texture tex, Node parent)
    {
        var control = _controlWrappedSpriteScene.Instance<ControlWrappedSprite>();
        parent.AddChild(control);
        control.Sprite.Texture = tex;
        if (tex == _coinTex) control.Sprite.Hframes = 8;
        if (tex == _keyTex) control.Sprite.Position = new Vector2(0, -4);
        control.Sprite.Modulate = new Color(control.Sprite.Modulate, _transparency);
        control.Sprite.Scale = Vector2.One * 2;
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
