using Godot;
using System;

public partial class Player : CharacterBody2D
{
	public static string GroupName = "player";
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print("player ready");
		AddToGroup(GroupName);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		Vector2 nv = Vector2.Zero;

		nv.X = Input.GetActionStrength("right") - Input.GetActionStrength("left") ;
		nv.Y = Input.GetActionStrength("down")- Input.GetActionStrength("up") ;

		Velocity = nv.Normalized() * 120.0f;

		Rotation = Velocity.Angle();

		MoveAndSlide();
	}
}
