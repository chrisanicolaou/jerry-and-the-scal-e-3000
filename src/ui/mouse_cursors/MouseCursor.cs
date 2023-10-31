using Godot;
using System;

namespace ChiciStudios.GithubGameJam2023.Src.UI
{
    public class MouseCursor : Resource
    {
        [Export] public Texture Cursor { get; private set; }
        [Export] public Input.CursorShape Shape { get; private set; }
        [Export] public Vector2 Hotspot { get; private set; }
    }
}