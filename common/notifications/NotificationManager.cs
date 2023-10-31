using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChiciStudios.GithubGameJam2023.Common.Notifications
{
    public class NotificationManager : Node
    {
        // ADD SIGNALS HERE
        [Signal] public delegate void TestNotification(string message);

        public void FireSignal(string signalName, params object[] args)
        {
            EmitSignal(signalName, args);
        }
    }
}