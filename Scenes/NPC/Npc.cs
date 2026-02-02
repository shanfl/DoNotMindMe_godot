using Godot;
using System;
using System.Collections.Generic;

public partial class Npc : CharacterBody2D
{
	private enum EnemyState {Patrolling,Chasing,Searching}


	[Export] private NavigationAgent2D _navAgent;
	[Export] private Node2D _patrolPoint;

	[Export] private Node2D _playerScanner;
	[Export] private RayCast2D _ray;
 
	private List<Vector2> _wayPoints = [];
	private int _currentWp = 0;

	private EnemyState _state = EnemyState.Patrolling;

	private Player _playerRef;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print("npc ready");
		
		SetPhysicsProcess(false);
		CreateWayPoints();
		CallDeferred(MethodName.LateSetup);
	}

	private async void LateSetup()
	{
		await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);
		SetPhysicsProcess(true);
		_playerRef = GetTree().GetFirstNodeInGroup(Player.GroupName) as Player;
	}

	private float GetFovAngle()
	{
		var dir = GlobalPosition.DirectionTo(_playerRef.GlobalPosition).Normalized();
		var dotP = dir.Dot(Velocity.Normalized());
		if(dotP >= -1.0f & dotP <= 1.0f)
		{
			return Mathf.RadToDeg(Mathf.Acos(dotP));
		}

		return 0.0f;
	}

	private  bool PlayerInFov()
	{
		return  GetFovAngle() < 60.0f;
	}


	 private void RaycastToPlayer()
	{
		if(!IsInstanceValid(_playerRef)) return;

		_playerScanner.LookAt(_playerRef.GlobalPosition);
	}


	private void SetNextWaypoint()
	{
		_currentWp %= _wayPoints.Count;
		_navAgent.TargetPosition = _wayPoints[_currentWp];
		_currentWp++;
	}


	private void CreateWayPoints()
	{
		foreach(Node2D item in _patrolPoint.GetChildren())
		{
			_wayPoints.Add(item.GlobalPosition);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if (Input.IsActionJustPressed("test"))
		{
			_navAgent.TargetPosition = GetGlobalMousePosition();
		}
		RaycastToPlayer();
		UpdateMovement();
		UpdateDebugLabel();
		UpdateNavigation();
	}

    private void UpdateMovement()
    {
		switch (_state)
		{
			case EnemyState.Patrolling:
				ProcessPatrolling();
				break;
		}
    }

    private void ProcessPatrolling()
    {
		if (_navAgent.IsNavigationFinished())
		{
			SetNextWaypoint();
		}
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
		s += $"Target:{_navAgent.TargetPosition}\n";
		s += $"Fov:{GetFovAngle():F2}  PlayerInFov:{PlayerInFov()} \n";
		SignalManager.EmitOnDebugLabel(s);
	}
}
