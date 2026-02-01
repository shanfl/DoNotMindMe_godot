using Godot;
using System;

public partial class GameUi : Control
{
	[Export] private Label _debugLabel;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SignalManager.Instance.OnDebugLabel += OnDebugLabel;
	}

    private void OnDebugLabel(string s)
    {
        _debugLabel.Text = s;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
