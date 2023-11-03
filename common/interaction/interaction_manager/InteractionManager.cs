using Godot;
using System;
using System.Collections.Generic;

public class InteractionManager : Node
{
    [Export(PropertyHint.Layers2dPhysics)] public uint InteractionCollisionMask { get; set; }
    [Export] private string _interactionTargetGroupName;
    public Node2D InteractionTarget { get; private set; }

    private List<InteractionArea> _nearbyInteractionAreas = new List<InteractionArea>();
    private InteractionArea _activeInteractionArea;

    public void RegisterNearbyInteractionArea(InteractionArea area) => _nearbyInteractionAreas.Add(area);

    public void DeregisterNearbyInteractionArea(InteractionArea area)
    {
        if (area == _activeInteractionArea)
        {
            _activeInteractionArea.Deactivate();
            _activeInteractionArea = null;
        }
        _nearbyInteractionAreas.Remove(area);
    }

    public override void _Process(float _)
    {
        if (InteractionTarget == null)
        {
            var nodesInGroup = GetTree().GetNodesInGroup(_interactionTargetGroupName);
            if (nodesInGroup.Count < 1) return;
            InteractionTarget = nodesInGroup[0] as Node2D;
            if (InteractionTarget == null) return;
        }
        if (_nearbyInteractionAreas.Count <= 0) return;
        var interactionTargetPos = InteractionTarget.GlobalPosition;
        _nearbyInteractionAreas.Sort((area1, area2) => SortAreasByDistance(area1, area2, interactionTargetPos));
        if (_activeInteractionArea != null && _activeInteractionArea.Equals(_nearbyInteractionAreas[0])) return;
        _activeInteractionArea?.Deactivate();
        _activeInteractionArea = _nearbyInteractionAreas[0];
        _activeInteractionArea.Activate();
    }

    private int SortAreasByDistance(InteractionArea area1, InteractionArea area2, Vector2 targetPos)
    {
        if (area1 == null || area2 == null) return 0;
        var area1Pos = area1.GlobalPosition;
        var area2Pos = area2.GlobalPosition;
        return area1Pos.DistanceSquaredTo(targetPos) > area2Pos.DistanceSquaredTo(targetPos) ? 1 : -1;
    }
}
