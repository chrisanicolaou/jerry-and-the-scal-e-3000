using Godot;
using System;
using ChiciStudios.GithubGameJam2023.Common.Dialog;

public class DialogBoxTest : Control
{
    [Export] private NodePath _dialogBoxPath;
    [Export] private NodePath _showButtonPath;
    [Export] private NodePath _advanceButtonPath;
    [Export] private NodePath _closeButtonPath;
    [Export] private NodePath _fillColorPickerPath;
    [Export] private NodePath _borderColorPickerPath;
    [Export] private NodePath _titleTextPickerPath;
    [Export] private NodePath _textPickerPath;
    [Export] private NodePath _textSpeedPickerPath;

    private DialogBox _dialogBox;
    private Button _showButton;
    private Button _advanceButton;
    private Button _closeButton;
    private ColorPicker _fillColorPicker;
    private ColorPicker _borderColorPicker;
    private LineEdit _titleTextPicker;
    private LineEdit _textPicker;
    private OptionButton _textSpeedOption;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _dialogBox = GetNode<DialogBox>(_dialogBoxPath);
        _showButton = GetNode<Button>(_showButtonPath);
        // _showButton.Connect("pressed", this, nameof(OnPressed));
        // _advanceButton = GetNode<Button>(_advanceButtonPath);
        // _advanceButton.Connect("pressed", this, nameof(OnAdvancePressed));
        // _closeButton = GetNode<Button>(_closeButtonPath);
        // _closeButton.Connect("pressed", this, nameof(OnClosePressed));
        _fillColorPicker = GetNode<ColorPicker>(_fillColorPickerPath);
        _fillColorPicker.Color = _dialogBox.DefaultFillColor;
        _borderColorPicker = GetNode<ColorPicker>(_borderColorPickerPath);
        _borderColorPicker.Color = _dialogBox.DefaultBorderColor;
        _titleTextPicker = GetNode<LineEdit>(_titleTextPickerPath);
        _titleTextPicker.Text = _dialogBox.DefaultTitle;
        _textPicker = GetNode<LineEdit>(_textPickerPath);
        _textPicker.Text = "This is a dialog prompt! - Dashes mean new prompts! - :D";
        _textSpeedOption = GetNode<OptionButton>(_textSpeedPickerPath);
    }
    //
    // private void OnPressed()
    // {
    //     Enum.TryParse<TextSpeed>(_textSpeedOption.Text, out var textSpeed);
    //     _dialogBox.Show(new DialogPrompt(_textPicker.Text.Split('-'), _titleTextPicker.Text, fillColor: _fillColorPicker.Color, borderColor: _borderColorPicker.Color, textSpeed: textSpeed));
    // }
    //
    // private void OnAdvancePressed()
    // {
    //     _dialogBox.AdvancePrompt();
    // }
    //
    // private void OnClosePressed()
    // {
    //     _dialogBox.Close();
    // }
}
