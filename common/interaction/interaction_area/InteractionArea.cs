using Godot;


public class InteractionArea : Area2D
{
    [Signal] public delegate void InteractionAreaTriggered();
    [Signal] public delegate void InteractionAreaActivated();
    [Signal] public delegate void InteractionAreaDeactivated();

    private InteractionManager _interactionManager;
    [Export] private bool _disabled;

    [Export] public bool UseKeyHint { get; set; } = true;
    [Export] private NodePath _collisionShapePath;
    [Export] private NodePath _keyHintPath;
    [Export] private Vector2 _keyHintOffset = new Vector2(20, -20);

    private CollisionShape2D _collisionShape;
    private Control _keyHint;

    public override void _Ready()
    {
        Connect("area_entered", this, nameof(OnAreaEntered));
        Connect("area_exited", this, nameof(OnAreaExited));
        _interactionManager = GetNode<InteractionManager>($"/root/{nameof(InteractionManager)}");
        _collisionShape = GetNode<CollisionShape2D>(_collisionShapePath);
        _keyHint = GetNode<Control>(_keyHintPath);
        _keyHint.RectPosition += _keyHintOffset;
        _keyHint.Hide();
        if (_disabled) Disable();
    }

    public void SetDisabled(bool disabled)
    {
        if (disabled) Disable();
        else Enable();
    }

    public void Disable()
    {
        _collisionShape.Disabled = true;
    }

    public void Enable()
    {
        _collisionShape.Disabled = false;
    }

    public void Trigger()
    {
        EmitSignal(nameof(InteractionAreaTriggered));
    }

    public void Activate()
    {
        EmitSignal(nameof(InteractionAreaActivated));
        if (UseKeyHint) _keyHint.Show();
    }

    public void Deactivate()
    {
        EmitSignal(nameof(InteractionAreaDeactivated));
        if (UseKeyHint) _keyHint.Hide();
    }

    private void OnAreaEntered(Area2D area)
    {
        _interactionManager.RegisterNearbyInteractionArea(this);
    }

    private void OnAreaExited(Area2D area)
    {
        _interactionManager.DeregisterNearbyInteractionArea(this);
    }

    public override void _ExitTree()
    {
        _interactionManager.DeregisterNearbyInteractionArea(this);
    }
}
