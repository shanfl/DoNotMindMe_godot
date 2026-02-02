using Godot;
using System;

public partial class Bullet : Area2D
{
	[Export] Timer _timer;

	private static readonly PackedScene _boomScen  = GD.Load<PackedScene>("res://Scenes/Boom/Boom.tscn");

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var p = GetTree().GetFirstNodeInGroup(Player.GroupName) as Player;
		if(p != null)
		{
			LookAt(p.GlobalPosition);
		}

		BodyEntered 	+= OnBodyEnter;
		_timer.Timeout 	+= OnTimeOut;
	}

    private void OnTimeOut()
    {
        CallDeferred(MethodName.QueueFree);
		CreateBoom();
    }


	private void CreateBoom()
	{
		var boom = _boomScen.Instantiate<Boom>();
		GetTree().Root.AddChild(boom);
		boom.GlobalPosition = GlobalPosition;
		CallDeferred(MethodName.QueueFree);
	}

    private void OnBodyEnter(Node2D body)
    {
		GD.Print($"OnBodyEnter body-name:{body.Name}");
        if(body is Player)
		{			
			_timer.Stop();
			SignalManager.EmitOnGameOver();
		} 

		CreateBoom();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
		MoveLocalX((float)delta*200.0f);
	}
}
