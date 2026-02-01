using Godot;
using System;
using System.Collections.Generic;

public partial class Main : Control
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {   
        GetTree().Paused = false;
    }
    
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        if (Input.IsActionJustPressed("space"))
        {
            GameManager.LoadGameScene();
        }
	}
}
