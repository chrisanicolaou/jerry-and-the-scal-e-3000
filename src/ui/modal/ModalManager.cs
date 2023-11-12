using Godot;
using System;
using System.Threading.Tasks;
using GithubGameJam2023.ui.modal;

public class ModalManager : CanvasLayer
{
    [Export] private NodePath _largeModalPath;
    [Export] private NodePath _smallModalPath;

    private Modal _largeModal;
    private Modal _smallModal;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _largeModal = GetNode<Modal>(_largeModalPath);
        _smallModal = GetNode<Modal>(_smallModalPath);
        _largeModal.Hide();
        _smallModal.Hide();
    }

    public async Task ShowModal(ModalOptions opts, ModalSize size) => await ShowModalInternal(size == ModalSize.Large ? _largeModal : _smallModal, opts);

    private async Task ShowModalInternal(Modal modal, ModalOptions opts)
    {
        await modal.Show(opts);
    }
}
