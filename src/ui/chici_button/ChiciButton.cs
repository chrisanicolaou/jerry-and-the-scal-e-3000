using Godot;
using System;
using ChiciStudios.GithubGameJam2023.Common.Audio;

namespace ChiciStudios.GithubGameJam2023.Src.UI
{
    public class ChiciButton : Button
    {
        [Export] private MouseCursor _defaultCursor;
        [Export] private MouseCursor _hoverCursor;
        [Export] private MouseCursor _pressedCursor;
        [Export] private AudioStream _hoverSfx;
        [Export] private AudioStream _pressedSfx;

        private AudioManager _audioManager;

        public override void _Ready()
        {
            _audioManager = GetNode<AudioManager>("/root/AudioManager");
            Connect("mouse_entered", this, nameof(OnMouseEntered));
            Connect("mouse_exited", this, nameof(OnMouseExited));
        }

        private void OnMouseEntered()
        {
            SetCursor(_hoverCursor);
            _audioManager.PlaySfx(_hoverSfx);
        }

        private void OnMouseExited()
        {
            SetCursor(_defaultCursor);
        }

        public override void _Pressed()
        {
            SetCursor(_pressedCursor);
            _audioManager.PlaySfx(_pressedSfx);
        }

        public override void _ExitTree()
        {
            SetCursor(_defaultCursor);
        }

        private void SetCursor(MouseCursor cursor) => Input.SetCustomMouseCursor(cursor.Cursor, cursor.Shape, cursor.Hotspot);
    }
}