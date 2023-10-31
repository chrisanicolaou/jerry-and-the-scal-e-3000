using Godot;
using System;
using ChiciStudios.GithubGameJam2023.Common.Notifications;

public class NotificationManagerTests : Node
{
    [Export] private NodePath _emitTestNotificationButton;
    [Export] private NodePath _notificationManagerPath;

    private NotificationManager _notificationManager;

    public override void _Ready()
    {
        _notificationManager = GetNode<NotificationManager>(_notificationManagerPath);
        _notificationManager.Connect(nameof(NotificationManager.TestNotification), this, nameof(OnNotificationReceived));
        
        var testNotifButton = GetNode<Button>(_emitTestNotificationButton);
        testNotifButton.Connect("pressed", this, nameof(OnTestButtonPressed));
    }

    private void OnTestButtonPressed()
    {
        _notificationManager.FireSignal(nameof(NotificationManager.TestNotification), "Boopidepop");
    }

    private void OnNotificationReceived(string message)
    {
        GD.Print($"Test notification received! Message: {message}");
    }
}
