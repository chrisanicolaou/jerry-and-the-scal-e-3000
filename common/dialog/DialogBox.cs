using Godot;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChiciStudios.GithubGameJam2023.Common.Dialog
{
    public class DialogBox : Control
    {
        private const float TextTweenSpeedFactor = 0.125f;
        [Export] private Texture _defaultIcon;
        [Export] public Color DefaultBorderColor { get; set; }
        [Export] public Color DefaultFillColor { get; set; }
        [Export] public string DefaultTitle { get; set; }
        [Export(PropertyHint.Enum, "Slow,Normal,Fast,VeryFast")] private string _textSpeed { get; set; }
        
        [Export] private NodePath _borderPath;
        [Export] private NodePath _iconPath;
        [Export] private NodePath _fillPath;
        [Export] private NodePath _titlePath;
        [Export] private NodePath _textPath;

        private Sprite _border;
        private Sprite _icon;
        private Sprite _fill;
        private Label _titleLabel;
        private Label _textLabel;
        private TextSpeed _defaultTextSpeed;
        private DialogPrompt _currentPrompt;
        private int _currentPromptIndex;
        private Task _currentTextTweenTask;
        private CancellationTokenSource _cancellationTokenSource;

        public bool CanAdvance => _currentPrompt != null && _currentPromptIndex < _currentPrompt.Texts.Length;

        public override void _Ready()
        {
            _border = GetNode<Sprite>(_borderPath);
            _icon = GetNode<Sprite>(_iconPath);
            _fill = GetNode<Sprite>(_fillPath);
            _titleLabel = GetNode<Label>(_titlePath);
            _textLabel = GetNode<Label>(_textPath);
            var textSpeedValid = Enum.TryParse(_textSpeed, out _defaultTextSpeed);
            if (!textSpeedValid)
            {
                GD.PrintErr($"Selected text speed is invalid. Defaulting to {TextSpeed.Normal.ToString()} instead");
                _defaultTextSpeed = TextSpeed.Normal;
            }
            Hide();
        }

        public async Task Show(DialogPrompt prompt)
        {
            if (prompt.Texts.Length < 1)
            {
                GD.PrintErr($"Attempting to show dialog with no text! {prompt}");
                return;
            }
            _currentPrompt = prompt;
            _currentPromptIndex = 0;
            _titleLabel.Text = !prompt.Title.Empty() ? prompt.Title : DefaultTitle;
            _border.SelfModulate = prompt.BorderColor ?? DefaultBorderColor;
            _fill.SelfModulate = prompt.FillColor ?? DefaultFillColor;
            _icon.Texture = prompt.Icon ?? _defaultIcon;
            _textLabel.Text = "";
            Show();
            var tween = GetTree().CreateTween();
            tween.TweenProperty(this, "rect_scale", Vector2.One, 0.25f).From(Vector2.Zero);
            await ToSignal(tween, "finished");
            _currentTextTweenTask = TweenText(prompt.Texts[_currentPromptIndex], speed: prompt.TextSpeed ?? _defaultTextSpeed);
            await _currentTextTweenTask;
        }

        public async Task AdvancePrompt()
        {
            if (_currentPrompt == null)
            {
                GD.PrintErr("Cannot advance a prompt before it has been loaded!");
                return;
            }
            // We're currently tweening, so advance to the end of the current prompt.
            if (!_currentTextTweenTask?.IsCompleted ?? false)
            {
                _cancellationTokenSource.Cancel();
                _textLabel.Text = _currentPrompt.Texts[_currentPromptIndex];
                return;
            }

            if (++_currentPromptIndex >= _currentPrompt.Texts.Length)
            {
                GD.PrintErr("Trying to advance a dialog that has completed. Closing instead");
                await Close();
                return;
            }

            _currentTextTweenTask = TweenText(_currentPrompt.Texts[_currentPromptIndex], speed: _currentPrompt.TextSpeed ?? _defaultTextSpeed);
            await _currentTextTweenTask;
        }

        public async Task Close()
        {
            _cancellationTokenSource?.Cancel();
            var tween = GetTree().CreateTween();
            tween.TweenProperty(this, "rect_scale", Vector2.Zero, 0.25f).From(Vector2.One);
            await ToSignal(tween, "finished");
            Hide();
        }

        // TODO: optimise this, its garbage
        private async Task TweenText(string finalText, TextSpeed? speed = null, string startText = "")
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var textSpeed = (int)(speed ?? _defaultTextSpeed);
            var timeBetweenCharacters = TextTweenSpeedFactor / textSpeed;
            _textLabel.Text = startText;
            for (var i = 0; i < finalText.Length; i++)
            {
                if (_cancellationTokenSource.IsCancellationRequested) return;
                // Only occurs when we have some startText, so we want to replace individual characters for that spooky effect
                if (i < startText.Length)
                {
                    var sb = new StringBuilder(_textLabel.Text)
                    {
                        [i] = finalText[i]
                    };
                    _textLabel.Text = sb.ToString();
                }
                else
                {
                    _textLabel.Text += finalText[i];
                }

                var timer = GetTree().CreateTimer(timeBetweenCharacters);
                await ToSignal(timer, "timeout");
            }
        }
    }
}