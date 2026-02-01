using Godot;
using System;

public partial class Npc : CharacterBody2D
{

	[Export] private NavigationAgent2D _navAgent;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if (Input.IsActionJustPressed("test"))
		{
			_navAgent.TargetPosition = GetGlobalMousePosition();
		}
		UpdateDebugLabel();
		UpdateNavigation();
	}

	private void UpdateNavigation()
	{
		if (!_navAgent.IsNavigationFinished())
		{
			var npp = _navAgent.GetNextPathPosition();
			LookAt(npp);
			Velocity = Position.DirectionTo(npp) * 100.0f;
			MoveAndSlide();
		}
	}

	private void UpdateDebugLabel()
	{
		string s = "";
		s += $"IsTargetReachable:{_navAgent.IsTargetReachable()}\n";
		s += $"IsTargetReached:{_navAgent.IsTargetReached()}\n";
		s += $"IsNavigationFinished:{_navAgent.IsNavigationFinished()}\n";
		s += $"Target:{_navAgent.TargetPosition}";
		SignalManager.EmitOnDebugLabel(s);
	}
}
